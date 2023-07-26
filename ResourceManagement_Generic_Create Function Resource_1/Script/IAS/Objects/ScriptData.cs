namespace Script.IAS
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using Skyline.Automation.IAS;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Automation;
	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class ScriptData
	{
		#region Fields
		private readonly IEngine engine;

		private readonly string resourceName;

		private IDms dms;

		private SrmHelpers srmHelpers;

		private Dictionary<string, FunctionMapper> protocolFunctionsByName;

		private Dictionary<string, IDmsElement> elementsByName;

		private Dictionary<string, List<IDmsElement>> elementsByProtocolName;

		private Dictionary<Guid, EntryPointDataMapper> entryPointDataMapperByFunctionId;

		private Dictionary<Guid, List<IDmsElement>> filteredElementsByFunctionId;

		private string selectedFunction;

		private string selectedElement;
		#endregion

		public ScriptData(IEngine engine, string resourceName)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));

			Init();
		}

		#region Properties
		public string ResourceName => resourceName;

		public IEnumerable<string> Functions => protocolFunctionsByName.Keys;

		public string SelectedFunction
		{
			get
			{
				return selectedFunction;
			}

			set
			{
				if (selectedFunction == value)
				{
					return;
				}

				selectedFunction = value;
				selectedElement = string.Empty;
				SelectedTableIndex = string.Empty;

				LoadElementsForSelectedFunction();
				VerifyFunctionEntryPoints();
				GetFilteredElements();
			}
		}

		public bool FunctionHasEntryPoints { get; private set; }

		public IEnumerable<string> Elements { get; private set; }

		public string SelectedElement
		{
			get
			{
				return selectedElement;
			}

			set
			{
				if (selectedElement == value)
				{
					return;
				}

				selectedElement = value;
				SelectedTableIndex = string.Empty;

				if (FunctionHasEntryPoints)
				{
					LoadTableIndexesForSelectedElement();
				}
			}
		}

		public IEnumerable<string> TableIndexes { get; private set; }

		public string SelectedTableIndex { get; set; }

		internal Guid CreatedResourceId { get; private set; }
		#endregion

		#region Methods
		public void CreateFunctionResource()
		{
			var existingResource = srmHelpers.ResourceManagerHelper.GetResourceByName(ResourceName);
			if (existingResource != null)
			{
				throw new InvalidOperationException($"Resource '{ResourceName}' already exists with ID '{existingResource.ID}'.");
			}

			var functionDefinition = protocolFunctionsByName[selectedFunction].FunctionDefinition;
			var linkedElement = elementsByName[selectedElement];

			var resource = new FunctionResource
			{
				Name = ResourceName,
				FunctionGUID = functionDefinition.GUID,
				MainDVEDmaID = linkedElement.AgentId,
				MainDVEElementID = linkedElement.Id,
				MaxConcurrency = 1,
			};

			if (FunctionHasEntryPoints)
			{
				resource.LinkerTableEntries = new[] { new Tuple<int, string>(functionDefinition.EntryPoints[0].ParameterId, SelectedTableIndex) };
			}

			CreatedResourceId = srmHelpers.ResourceManagerHelper.AddOrUpdateResources(resource)[0].ResourceId.Id;
		}

		public List<string> GetFilteredElements()
		{
			if (!protocolFunctionsByName.TryGetValue(SelectedFunction, out var functionMapper) || !elementsByProtocolName.TryGetValue(functionMapper.ProtocolName, out var elements))
			{
				return new List<string>();
			}

			var functionEntryPoint = functionMapper.FunctionDefinition.EntryPoints.FirstOrDefault();
			if (functionEntryPoint == null)
			{
				var filteredElements = GetFilteredElementsByMaxInstances(elements, functionMapper.FunctionDefinition);

				return filteredElements.Select(x => x.Name).ToList();
			}
			else
			{
				var filteredElements = GetFilteredElementsByEntryPoints(elements, functionMapper.FunctionDefinition);

				return filteredElements.Select(x => x.Name).ToList();
			}
		}

		public bool IsSelectedElementAvailable()
		{
			if (!protocolFunctionsByName.TryGetValue(SelectedFunction, out var functionMapper) || !elementsByName.TryGetValue(selectedElement, out var element))
			{
				return false;
			}

			if (!filteredElementsByFunctionId.TryGetValue(functionMapper.FunctionDefinition.GUID, out var filteredElements) || !filteredElements.Exists(x => x.DmsElementId.Value == element.DmsElementId.Value))
			{
				return false;
			}

			return true;
		}

		private void Init()
		{
			dms = engine.GetDms();
			srmHelpers = new SrmHelpers(engine);

			entryPointDataMapperByFunctionId = new Dictionary<Guid, EntryPointDataMapper>();
			filteredElementsByFunctionId = new Dictionary<Guid, List<IDmsElement>>();

			LoadFunctions();
			LoadElements();
		}

		private void LoadFunctions()
		{
			protocolFunctionsByName = new Dictionary<string, FunctionMapper>();

			srmHelpers.ProtocolFunctionHelper.GetAllProtocolFunctions(true).Where(x => !x.IsSystemFunction()).ForEach(x =>
			{
				x.ProtocolFunctionVersions.Single().FunctionDefinitions.ForEach(y =>
				{
					protocolFunctionsByName.Add(y.Name, new FunctionMapper
					{
						ProtocolName = x.ProtocolName,
						FunctionDefinition = y,
					});
				});
			});
		}

		private void LoadElements()
		{
			elementsByName = new Dictionary<string, IDmsElement>();
			elementsByProtocolName = new Dictionary<string, List<IDmsElement>>();

			dms.GetElements().Where(x => x.State == Skyline.DataMiner.Core.DataMinerSystem.Common.ElementState.Active).ForEach(x =>
			{
				if (!elementsByProtocolName.TryGetValue(x.Protocol.Name, out List<IDmsElement> elements))
				{
					elements = new List<IDmsElement>();

					elementsByProtocolName.Add(x.Protocol.Name, elements);
				}

				elements.Add(x);
				elementsByName.Add(x.Name, x);
			});
		}

		private void LoadElementsForSelectedFunction()
		{
			if (!protocolFunctionsByName.TryGetValue(SelectedFunction, out var functionMapper) || !elementsByProtocolName.TryGetValue(functionMapper.ProtocolName, out var elements))
			{
				Elements = new List<string>();

				return;
			}

			Elements = elements.Select(x => x.Name);
		}

		private void LoadTableIndexesForSelectedElement()
		{
			if (!protocolFunctionsByName.TryGetValue(SelectedFunction, out var functionMapper) || !elementsByName.TryGetValue(selectedElement, out var element) || !entryPointDataMapperByFunctionId.TryGetValue(functionMapper.FunctionDefinition.GUID, out var entryPointDataMapper) || !entryPointDataMapper.TryGetEntryPointData(element, out var entryPointData))
			{
				TableIndexes = new List<string>();

				return;
			}

			TableIndexes = entryPointData.Select(x => x.DisplayValue);
		}

		private void VerifyFunctionEntryPoints()
		{
			if (!protocolFunctionsByName.TryGetValue(SelectedFunction, out var functionMapper))
			{
				return;
			}

			FunctionHasEntryPoints = functionMapper.FunctionDefinition.EntryPoints.Any();
		}

		private List<IDmsElement> GetFilteredElementsByMaxInstances(List<IDmsElement> elements, FunctionDefinition functionDefinition)
		{
			if (filteredElementsByFunctionId.TryGetValue(functionDefinition.GUID, out var filteredElements))
			{
				return filteredElements;
			}

			if (functionDefinition.MaxInstances == 0)
			{
				filteredElementsByFunctionId.Add(functionDefinition.GUID, elements);

				return elements;
			}

			FilterElement<Resource> filter = new ORFilterElement<Resource>(elements.Select(element => FunctionResourceExposers.MainDVEDmaID.Equal(element.AgentId).AND(FunctionResourceExposers.MainDVEElementID.Equal(element.Id))).ToArray());
			filter = filter.AND(FunctionResourceExposers.FunctionGUID.Equal(functionDefinition.GUID));

			var resources = srmHelpers.ResourceManagerHelper.GetResources(filter);

			var numberOfResourcesByElement = new Dictionary<string, int>();
			foreach (var resource in resources)
			{
				if (!(resource is FunctionResource fResource))
				{
					continue;
				}

				var elementInfo = $"{fResource.MainDVEDmaID}/{fResource.MainDVEElementID}";
				if (!numberOfResourcesByElement.TryGetValue(elementInfo, out var numberOfResources))
				{
					numberOfResources = 0;

					numberOfResourcesByElement.Add(elementInfo, numberOfResources);
				}

				numberOfResourcesByElement[elementInfo] += 1;
			}

			var elementInfoOfElementsWithMaxInstances = numberOfResourcesByElement.Where(x => x.Value >= functionDefinition.MaxInstances).Select(x => x.Key).ToList();
			filteredElements = elements.Where(x => !elementInfoOfElementsWithMaxInstances.Contains($"{x.AgentId}/{x.Id}")).ToList();

			filteredElementsByFunctionId.Add(functionDefinition.GUID, filteredElements);

			return filteredElements;
		}

		private List<IDmsElement> GetFilteredElementsByEntryPoints(List<IDmsElement> elements, FunctionDefinition functionDefinition)
		{
			if (filteredElementsByFunctionId.TryGetValue(functionDefinition.GUID, out var filteredElements))
			{
				return filteredElements;
			}

			var entryPointDataMapper = new EntryPointDataMapper();

			filteredElements = new List<IDmsElement>();
			foreach (var element in elements)
			{
				var entryPoint = srmHelpers.ProtocolFunctionHelper.GetFunctionEntryPoints(functionDefinition.GUID, element.AgentId, element.Id).FirstOrDefault();
				if (entryPoint == null)
				{
					continue;
				}

				var entryPointData = entryPoint.Data.Where(x => x.Element == null).ToList();
				if (!entryPointData.Any())
				{
					continue;
				}

				entryPointDataMapper.AddEntryPointData(element, entryPointData);

				filteredElements.Add(element);
			}

			filteredElementsByFunctionId.Add(functionDefinition.GUID, filteredElements);
			entryPointDataMapperByFunctionId.Add(functionDefinition.GUID, entryPointDataMapper);

			return filteredElements;
		}
		#endregion

		private sealed class FunctionMapper
		{
			public string ProtocolName { get; set; }

			public FunctionDefinition FunctionDefinition { get; set; }
		}

		private sealed class EntryPointDataMapper
		{
			private readonly Dictionary<string, List<EntryPointData>> entryPointDataByElementInfo;

			internal EntryPointDataMapper()
			{
				entryPointDataByElementInfo = new Dictionary<string, List<EntryPointData>>();
			}

			public void AddEntryPointData(IDmsElement element, List<EntryPointData> entryPointData)
			{
				entryPointDataByElementInfo.Add(element.DmsElementId.Value, entryPointData);
			}

			public bool TryGetEntryPointData(IDmsElement element, out List<EntryPointData> entryPointData)
			{
				return entryPointDataByElementInfo.TryGetValue(element.DmsElementId.Value, out entryPointData);
			}
		}
	}
}
