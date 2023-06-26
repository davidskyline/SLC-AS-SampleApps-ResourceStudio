﻿namespace Skyline.Automation.DOM
{
	//------------------------------------------------------------------------------
	// <auto-generated>
	//     This code was generated by the Dom Editor automation script.
	//     Changes to this file will be lost if the code is regenerated.
	// </auto-generated>
	//------------------------------------------------------------------------------
	namespace DomIds
	{
		using System;

		using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
		using Skyline.DataMiner.Net.Sections;

		public static class Resourcemanagement
		{
			public const string ModuleId = "resourcemanagement";
			public static class Enums
			{
				public enum CostUnit
				{
					PerMinute = 0,
					PerHour = 1,
					PerDay = 2
				}

				public enum Currency
				{
					EUR = 0,
					USD = 1
				}

				public enum Status
				{
					Available = 0,
					Maintenance = 1,
					Unavailable = 2
				}

				public enum InventoryType
				{
					Installed = 0,
					NotInstalled = 1,
					DynamicInventory = 2
				}

				public enum Type
				{
					Element = 0,
					VirtualFunction = 1,
					Service = 2,
					UnlinkedResource = 3
				}

				public enum CapabilityType
				{
					String = 0,
					Enum = 1
				}
			}

			public static class Sections
			{
				public static class ResourceInternalProperties
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("5a5c6591-a833-48d5-8fd4-10733cb1eddb"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID Pool_Ids
					{
						get;
					}

					= new FieldDescriptorID(new Guid("0733a8d7-6a8d-4dee-8762-5ebf77c1eeb2"));
					public static FieldDescriptorID Resource_Id
					{
						get;
					}

					= new FieldDescriptorID(new Guid("235b137d-3aea-4ce0-af4b-5fe9b027f894"));
				}

				public static class ResourceControl
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("972a33ff-fec9-4172-87b8-5523a49a49a5"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID AutomationScriptName
					{
						get;
					}

					= new FieldDescriptorID(new Guid("b83e17c2-ba3b-4952-9e1d-8fc5dcd92751"));
				}

				public static class ResourceCost
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("5e15622c-bc1e-4e84-a996-f7086c1e9b92"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID Cost
					{
						get;
					}

					= new FieldDescriptorID(new Guid("c4a72db5-3b37-4a2d-a6fc-2faa4d89fdda"));
					public static FieldDescriptorID CostUnit
					{
						get;
					}

					= new FieldDescriptorID(new Guid("ec809ebd-f73b-4dcb-923d-4af1a57ab12c"));
					public static FieldDescriptorID Currency
					{
						get;
					}

					= new FieldDescriptorID(new Guid("43124d0d-e9a6-4f3c-a6de-9c1533bf2d1c"));
					public static FieldDescriptorID FLECost
					{
						get;
					}

					= new FieldDescriptorID(new Guid("379bc449-48aa-4033-a057-e10c1652cd25"));
				}

				public static class ResourcePoolInternalProperties
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("962d3326-b0d5-4d03-9f53-bba208996af7"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID Resource_Ids
					{
						get;
					}

					= new FieldDescriptorID(new Guid("8acca71d-3c64-4272-84b0-e7371f373d8e"));
					public static FieldDescriptorID Pool_Resource_Id
					{
						get;
					}

					= new FieldDescriptorID(new Guid("a1dd9b79-f502-4a39-b1ce-5c213aa86987"));
					public static FieldDescriptorID Resource_Pool_Id
					{
						get;
					}

					= new FieldDescriptorID(new Guid("3239b3f5-0370-40a6-af5c-e6630e9745ed"));
				}

				public static class CapacityInfo
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("de65f1af-ae5a-41a1-aa16-5afbe056fdc4"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID CapacityName
					{
						get;
					}

					= new FieldDescriptorID(new Guid("2e332da3-cd08-496d-b6f1-b978287ceb32"));
					public static FieldDescriptorID Units
					{
						get;
					}

					= new FieldDescriptorID(new Guid("7c072027-fba9-44e8-8fba-0844240fdeea"));
					public static FieldDescriptorID MinRange
					{
						get;
					}

					= new FieldDescriptorID(new Guid("f9dfd2ba-cd8b-4499-949b-135775bd725f"));
					public static FieldDescriptorID StepSize
					{
						get;
					}

					= new FieldDescriptorID(new Guid("5a7de8d8-d6fc-489a-8b2d-142b9c6f768f"));
					public static FieldDescriptorID MaxRange
					{
						get;
					}

					= new FieldDescriptorID(new Guid("995a826c-720f-47cf-8909-c02eceb34c2a"));
					public static FieldDescriptorID Decimals
					{
						get;
					}

					= new FieldDescriptorID(new Guid("3b12c5d9-f0b6-4253-bc14-93804d7f56f6"));
				}

				public static class ResourceCapacities
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("2e079f3e-6f71-46bf-8fc8-d4203ad448a6"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID Capacity
					{
						get;
					}

					= new FieldDescriptorID(new Guid("e9f44bb7-0adf-4ac4-bd60-e9af0ae8f8d9"));
					public static FieldDescriptorID CapacityValue
					{
						get;
					}

					= new FieldDescriptorID(new Guid("8c47f671-be72-4caa-a394-68eeac5bfef8"));
				}

				public static class ResourceProperties
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("08995f51-a59d-4ac9-a224-9481dbc4c782"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID Property
					{
						get;
					}

					= new FieldDescriptorID(new Guid("18bacef1-5489-414b-8de8-ba44e62f6ea5"));
					public static FieldDescriptorID PropertyValue
					{
						get;
					}

					= new FieldDescriptorID(new Guid("6ab17d00-f8be-4eb9-a67c-073c60d0be21"));
				}

				public static class ResourceInfo
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("930c7bca-fff9-49f7-b141-1cba4aa28afd"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID Name
					{
						get;
					}

					= new FieldDescriptorID(new Guid("13833c8f-6874-44e9-9aeb-9a9914e26771"));
					public static FieldDescriptorID Status
					{
						get;
					}

					= new FieldDescriptorID(new Guid("422506c5-ec72-42bc-aebc-adba2e7341f1"));
					public static FieldDescriptorID InventoryType
					{
						get;
					}

					= new FieldDescriptorID(new Guid("07f93cbe-ab1c-4a05-b25f-9da461acd218"));
					public static FieldDescriptorID Type
					{
						get;
					}

					= new FieldDescriptorID(new Guid("5136f426-546f-4144-9a87-44194b10fc3b"));
					public static FieldDescriptorID ErrorDetails
					{
						get;
					}

					= new FieldDescriptorID(new Guid("a90911ea-8799-4c1c-a925-9607c814c0e4"));
					public static FieldDescriptorID Favourite
					{
						get;
					}

					= new FieldDescriptorID(new Guid("a554a107-77da-492f-8054-9ad98dc63541"));
					public static FieldDescriptorID Element
					{
						get;
					}

					= new FieldDescriptorID(new Guid("00886e7d-79d0-466c-b704-24aca6d361c8"));
					public static FieldDescriptorID Concurrency
					{
						get;
					}

					= new FieldDescriptorID(new Guid("5846db60-e57f-491a-8958-848695e57921"));
				}

				public static class ResourcePoolInfo
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("2377a750-f9e6-4a91-9d5c-4b9a4bee25ac"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID Name
					{
						get;
					}

					= new FieldDescriptorID(new Guid("2cebe78f-be32-455d-8daf-86f815aa3b82"));
					public static FieldDescriptorID Bookable
					{
						get;
					}

					= new FieldDescriptorID(new Guid("f7348534-eb3d-4475-abfa-9bef82c52f9e"));
					public static FieldDescriptorID ErrorDetails
					{
						get;
					}

					= new FieldDescriptorID(new Guid("bd09b565-8c33-4f70-b7a1-42b909e9c907"));
				}

				public static class ResourcePoolCapabilities
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("702ec27e-46b7-4edf-a3ea-42aa26a8aa24"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID Capability
					{
						get;
					}

					= new FieldDescriptorID(new Guid("8371a9dd-2289-424a-8ff9-e060110d62f1"));
					public static FieldDescriptorID Capability_Enum_Values
					{
						get;
					}

					= new FieldDescriptorID(new Guid("2af65872-4184-4c45-966b-ca336064a92f"));
					public static FieldDescriptorID Capability_String_Value
					{
						get;
					}

					= new FieldDescriptorID(new Guid("b18906a6-b99a-46c3-b677-eaa9d40ccdcc"));
				}

				public static class ResourceConnectionManagement
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("8d6d47d1-493d-4dec-9add-6203a908d7e6"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID InputVsgs
					{
						get;
					}

					= new FieldDescriptorID(new Guid("fb0a663e-d43f-406c-b339-67737ce79678"));
					public static FieldDescriptorID OutputVsgs
					{
						get;
					}

					= new FieldDescriptorID(new Guid("129e4c6d-3b33-4e57-96bb-9dd9c206c6ab"));
				}

				public static class CapabilityEnumValueDetails
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("549e9df5-b399-40b7-aa3f-33b5fb679e68"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID Capability
					{
						get;
					}

					= new FieldDescriptorID(new Guid("5dbf2043-a11b-49bf-a96d-a0fd745a700d"));
					public static FieldDescriptorID Value
					{
						get;
					}

					= new FieldDescriptorID(new Guid("4198ecbf-3316-485d-a7d5-356266ff6444"));
				}

				public static class CapabilityInfo
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("9856996f-faeb-4d62-8a2a-31f0f0acddbb"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID CapabilityName
					{
						get;
					}

					= new FieldDescriptorID(new Guid("6990044c-986b-4083-a878-9a7f07a55cd1"));
					public static FieldDescriptorID CapabilityType
					{
						get;
					}

					= new FieldDescriptorID(new Guid("f13f55e4-bc51-49cd-84d9-1808ba2878ac"));
				}

				public static class PropertyInfo
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("6477c6b5-34df-4379-86ed-1d098f42d0e7"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID PropertyName
					{
						get;
					}

					= new FieldDescriptorID(new Guid("0c1f8559-198c-40ff-9d52-60e2e1a90988"));
				}

				public static class ResourceOther
				{
					public static SectionDefinitionID Id
					{
						get;
					}

					= new SectionDefinitionID(new Guid("ff481176-1bbe-4216-ad77-214fd50ebdf3"))
					{ ModuleId = "resourcemanagement" };
					public static FieldDescriptorID IconImage
					{
						get;
					}

					= new FieldDescriptorID(new Guid("a968293e-5c83-4572-9dbe-6a5ba5eca43e"));
					public static FieldDescriptorID URL
					{
						get;
					}

					= new FieldDescriptorID(new Guid("4e1a48b0-fb2c-42da-bffd-34af527c423f"));
				}
			}

			public static class Definitions
			{
				public static DomDefinitionId Resourcepool
				{
					get;
				}

				= new DomDefinitionId(new Guid("a8262bd8-6f6c-47d4-964e-87de26d1e32e"))
				{ ModuleId = "resourcemanagement" };
				public static DomDefinitionId Capabilityenumvalue
				{
					get;
				}

				= new DomDefinitionId(new Guid("d27f4cd5-a933-4d88-b13e-a26eea0967c2"))
				{ ModuleId = "resourcemanagement" };
				public static DomDefinitionId Capacity
				{
					get;
				}

				= new DomDefinitionId(new Guid("568d0446-0652-43e0-ab4d-47cf19139ba2"))
				{ ModuleId = "resourcemanagement" };
				public static DomDefinitionId Capability
				{
					get;
				}

				= new DomDefinitionId(new Guid("6c22db22-98d1-429f-9769-b651aa94d8ca"))
				{ ModuleId = "resourcemanagement" };
				public static DomDefinitionId Resourceproperty
				{
					get;
				}

				= new DomDefinitionId(new Guid("60450534-b5bd-43c2-b088-37e171725fa0"))
				{ ModuleId = "resourcemanagement" };
				public static DomDefinitionId Resource
				{
					get;
				}

				= new DomDefinitionId(new Guid("d2afc1dc-f39c-49c8-a70f-9120bfbfc0a0"))
				{ ModuleId = "resourcemanagement" };
			}

			public static class Behaviors
			{
				public static class Capability_Behavior
				{
					public static DomBehaviorDefinitionId Id
					{
						get;
					}

					= new DomBehaviorDefinitionId(new Guid("d3538695-c431-4289-9038-79b68f7bdb16"))
					{ ModuleId = "resourcemanagement" };
					public static class Statuses
					{
						public const string Draft = "draft";
					}

					public static class Transitions
					{
					}
				}

				public static class Resource_Behavior
				{
					public static DomBehaviorDefinitionId Id
					{
						get;
					}

					= new DomBehaviorDefinitionId(new Guid("6bac6e39-e58d-43c0-b354-81119f5828dc"))
					{ ModuleId = "resourcemanagement" };
					public static class Statuses
					{
						public const string Draft = "draft";
						public const string Complete = "complete";
						public const string Deprecated = "deprecated";
						public const string Error = "395b5650-adab-4abe-9c15-f46ac5c66da2";
					}

					public static class Transitions
					{
						public const string Draft_To_Complete = "draft_to_complete";
						public const string Complete_To_Deprecated = "complete_to_deprecated";
						public const string Draft_To_Error = "draft_to_error";
						public const string Complete_To_Error = "complete_to_error";
						public const string Error_To_Complete = "error_to_complete";
					}
				}

				public static class Capacity_Behavior
				{
					public static DomBehaviorDefinitionId Id
					{
						get;
					}

					= new DomBehaviorDefinitionId(new Guid("01ce98da-d16f-4697-b40a-0f2f1672366d"))
					{ ModuleId = "resourcemanagement" };
					public static class Statuses
					{
						public const string Draft = "draft";
					}

					public static class Transitions
					{
					}
				}

				public static class Capabilityenumvalue_Behavior
				{
					public static DomBehaviorDefinitionId Id
					{
						get;
					}

					= new DomBehaviorDefinitionId(new Guid("2850a8f6-36ad-4a60-8ced-1fe7d31c2115"))
					{ ModuleId = "resourcemanagement" };
					public static class Statuses
					{
						public const string Draft = "draft";
					}

					public static class Transitions
					{
					}
				}

				public static class Resourcepool_Behavior
				{
					public static DomBehaviorDefinitionId Id
					{
						get;
					}

					= new DomBehaviorDefinitionId(new Guid("cc539721-a544-415b-a45c-8ce7ed102975"))
					{ ModuleId = "resourcemanagement" };
					public static class Statuses
					{
						public const string Draft = "draft";
						public const string Complete = "complete";
						public const string Deprecated = "deprecated";
						public const string Error = "9b95fc0e-71d1-42a2-8438-d285a4396105";
					}

					public static class Transitions
					{
						public const string Draft_To_Complete = "draft_to_complete";
						public const string Complete_To_Deprecated = "complete_to_deprecated";
						public const string Draft_To_Error = "draft_to_error";
						public const string Complete_To_Error = "complete_to_error";
						public const string Error_To_Complete = "error_to_complete";
					}
				}
			}
		}
	}
}
