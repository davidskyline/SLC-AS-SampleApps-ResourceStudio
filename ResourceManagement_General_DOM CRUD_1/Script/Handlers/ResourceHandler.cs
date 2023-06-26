namespace Script
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.Automation.DOM;
	using Skyline.Automation.DOM.DomIds;
	using Skyline.Automation.SRM;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;

	public class ResourceHandler
	{
		#region Fields
		private readonly IEngine engine;

		private readonly DomHelper domHelper;

		private readonly DomInstance domInstance;

		private Lazy<ResourceManagerHandler> resourceManagerHandler;

		private Lazy<SrmHelpers> srmHelpers;

		private Lazy<ResourceData> resourceData;
		#endregion

		public ResourceHandler(IEngine engine, DomHelper domHelper, DomInstance domInstance)
		{
			this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
			this.domHelper = domHelper ?? throw new ArgumentNullException(nameof(domHelper));
			this.domInstance = domInstance ?? throw new ArgumentNullException(nameof(domInstance));

			Init();
		}

		#region Properties
		private ResourceManagerHandler ResourceManagerHandler => resourceManagerHandler.Value;

		private SrmHelpers SrmHelpers => srmHelpers.Value;

		private ResourceData ResourceData => resourceData.Value;
		#endregion

		#region Methods
		public bool ValidateNameChange()
		{
			var existingResource = SrmHelpers.ResourceManagerHelper.GetResourceByName(ResourceData.Name);
			if (existingResource != null)
			{
				if (existingResource.ID != ResourceData.ResourceId)
				{
					SetErrorMessage($"Resource '{ResourceData.Name}' already exists with ID '{existingResource.ID}'.");

					return false;
				}

				if (existingResource.Name == ResourceData.Name)
				{
					return true;
				}
			}

			var resource = SrmHelpers.ResourceManagerHelper.GetResource(ResourceData.ResourceId);
			if (resource == null)
			{
				SetErrorMessage($"Resource '{ResourceData.Name}' with ID '{ResourceData.ResourceId}' does not exist.");

				return false;
			}

			resource.Name = ResourceData.Name;
			SrmHelpers.ResourceManagerHelper.AddOrUpdateResources(resource);

			return true;
		}

		public bool ValidateProperties()
		{
			if (ResourceData.ResourceId == Guid.Empty)
			{
				return true;
			}

			var resource = SrmHelpers.ResourceManagerHelper.GetResource(ResourceData.ResourceId);

			var propertyNamesManagedByDom = ResourceManagerHandler.Properties.Select(x => x.Name).ToList();
			var requiredProperties = GetRequiredResourceProperties();

			var added = requiredProperties.Where(x => !resource.Properties.Select(y => y.Name).Contains(x.Name)).ToList();
			var updated = requiredProperties.Except(added).ToList();
			var removed = resource.Properties.Where(x => !requiredProperties.Select(y => y.Name).Contains(x.Name) && propertyNamesManagedByDom.Contains(x.Name)).ToList();

			if (ResourceHasChangedProperties(resource, added, updated, removed))
			{
				SrmHelpers.ResourceManagerHelper.AddOrUpdateResources(resource);
			}

			return true;
		}

		private void Init()
		{
			resourceManagerHandler = new Lazy<ResourceManagerHandler>(() => new ResourceManagerHandler(domHelper));
			srmHelpers = new Lazy<SrmHelpers>(() => new SrmHelpers(engine));

			resourceData = new Lazy<ResourceData>(() => ResourceManagerHandler.Resources.Single(x => x.Instance.ID.Id == domInstance.ID.Id));
		}

		private void SetErrorMessage(string errorMessage)
		{
			var resourceInfoSection = domInstance.Sections.Single(x => x.SectionDefinitionID.Id == Resourcemanagement.Sections.ResourceInfo.Id.Id);
			resourceInfoSection.AddOrReplaceFieldValue(new FieldValue(Resourcemanagement.Sections.ResourceInfo.ErrorDetails, new ValueWrapper<string>($"[{DateTime.Now}] >>> {errorMessage}")));

			domHelper.DomInstances.Update(domInstance);
			domHelper.DomInstances.DoStatusTransition(domInstance.ID, Resourcemanagement.Behaviors.Resource_Behavior.Transitions.Complete_To_Error);
		}

		private List<ResourceManagerProperty> GetRequiredResourceProperties()
		{
			var propertiesById = ResourceManagerHandler.Properties.ToDictionary(x => x.Instance.ID.Id, x => x);

			var properties = new List<ResourceManagerProperty>();
			foreach (var resourceProperty in ResourceData.Properties)
			{
				if (!propertiesById.TryGetValue(resourceProperty.PropertyId, out var propertyData))
				{
					continue;
				}

				var property = new ResourceManagerProperty(propertyData.Name, resourceProperty.Value);

				properties.Add(property);
			}

			return properties;
		}

		private bool ResourceHasChangedProperties(Resource resource, List<ResourceManagerProperty> added, List<ResourceManagerProperty> updated, List<ResourceManagerProperty> removed)
		{
			var hasChangedData = false;

			foreach (var resourceProperty in removed)
			{
				resource.Properties.Remove(resourceProperty);

				hasChangedData = true;
			}

			foreach (var resourceProperty in updated)
			{
				var property = resource.Properties.Single(x => x.Name == resourceProperty.Name);
				if (property.Value.Equals(resourceProperty.Value))
				{
					continue;
				}

				property.Value = resourceProperty.Value;

				hasChangedData = true;
			}

			foreach (var resourceProperty in added)
			{
				resource.Properties.Add(resourceProperty);

				hasChangedData = true;
			}

			return hasChangedData;
		}
		#endregion
	}
}
