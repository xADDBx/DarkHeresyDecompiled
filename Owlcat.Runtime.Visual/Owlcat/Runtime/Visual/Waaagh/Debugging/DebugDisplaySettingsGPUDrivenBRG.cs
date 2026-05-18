using System;
using System.Text;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class DebugDisplaySettingsGPUDrivenBRG : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	private static class Strings
	{
		public static readonly DebugUI.Widget.NameAndTooltip Enabled = new DebugUI.Widget.NameAndTooltip
		{
			name = "Enabled"
		};

		public static readonly DebugUI.Widget.NameAndTooltip LogResources = new DebugUI.Widget.NameAndTooltip
		{
			name = "Log Resources",
			tooltip = "Log Material and Mesh usage."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OverrideCameraToMain = new DebugUI.Widget.NameAndTooltip
		{
			name = "Override Camera To Main",
			tooltip = "Force main camera frustum planes for culling."
		};

		public static readonly DebugUI.Widget.NameAndTooltip FrustumCullingMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Frustum Culling Mode",
			tooltip = "Select whether to perform frustum culling fully on GPU or not."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OpaqueSortingCPU = new DebugUI.Widget.NameAndTooltip
		{
			name = "Opaque Sorting (CPU)",
			tooltip = "Select whether to perform sorting of opaque draw calls on CPU."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OpaqueSortingGPU = new DebugUI.Widget.NameAndTooltip
		{
			name = "Opaque Sorting (GPU)",
			tooltip = "Select whether to perform sorting of opaque instances calls on GPU."
		};

		public static readonly DebugUI.Widget.NameAndTooltip CullingStats = new DebugUI.Widget.NameAndTooltip
		{
			name = "Collect",
			tooltip = "Collect the stats of how many instances are visible and how many are culled."
		};

		public static readonly DebugUI.Widget.NameAndTooltip CullingStatsMainPassRow = new DebugUI.Widget.NameAndTooltip
		{
			name = "Main"
		};

		public static readonly DebugUI.Widget.NameAndTooltip CullingStatsShadowsPassRow = new DebugUI.Widget.NameAndTooltip
		{
			name = "Shadows"
		};

		public static readonly DebugUI.Widget.NameAndTooltip SkipRendering = new DebugUI.Widget.NameAndTooltip
		{
			name = "Skip Rendering",
			tooltip = "Disables rendering and GPU culling, so it may prevent certain GPU crashes."
		};

		public static readonly DebugUI.Widget.NameAndTooltip SkipSubmittingDrawCommands = new DebugUI.Widget.NameAndTooltip
		{
			name = "Skip Submitting Draw Commands",
			tooltip = "Build draw commands but do not submit them. Disables rendering, so it may prevent certain GPU crashes."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ViewTypeFilter = new DebugUI.Widget.NameAndTooltip
		{
			name = "View Type Filter",
			tooltip = "Allow culling and drawing only the selected view type."
		};

		public static readonly DebugUI.Widget.NameAndTooltip LogRendererGroups = new DebugUI.Widget.NameAndTooltip
		{
			name = "Log Renderer Groups",
			tooltip = "Log the info of all renderer groups."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawOneByOne = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw One By One",
			tooltip = "Force split all instances into individual draw calls."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DecalSorting = new DebugUI.Widget.NameAndTooltip
		{
			name = "Decal Sorting",
			tooltip = "Toggle Decal Sorting."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DecalBatching = new DebugUI.Widget.NameAndTooltip
		{
			name = "Decal Batching",
			tooltip = "Toggle Decal Batching after sorting."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DisableGroupSorting = new DebugUI.Widget.NameAndTooltip
		{
			name = "Disable Group Sorting",
			tooltip = "Do not sort renderer groups (sorting reduces state switching and improves batching)."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DisableGroupMerging = new DebugUI.Widget.NameAndTooltip
		{
			name = "Disable Group Merging",
			tooltip = "Do not merge renderer groups with equivalent render state (merging reduces state switching and improves batching)."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ForceBuildRenderGroupsEachFrame = new DebugUI.Widget.NameAndTooltip
		{
			name = "Force Build Render Groups Each Frame",
			tooltip = "Invalid render groups slice cache each frame."
		};

		public static readonly DebugUI.Widget.NameAndTooltip GPUDataUploadDebugMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "GPU Data Upload Debug Mode",
			tooltip = "Select the mode of debug GPU upload. These uploads will be in addition to the necessary ones."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OnModifiedRendererGroups = new DebugUI.Widget.NameAndTooltip
		{
			name = "On Modified Renderer Groups",
			tooltip = "Rebuild cached renderer groups data."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OcclusionCulling = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occlusion Culling",
			tooltip = "Toggle Occlusion Culling in the pipeline asset."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DepthReprojection = new DebugUI.Widget.NameAndTooltip
		{
			name = "Depth Reprojection",
			tooltip = "Toggle Depth Reprojection in the pipeline asset."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OccludeeUseAABB = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occludee Use AABB",
			tooltip = "Toggle bounding sphere vs. AABB usage for occludees."
		};

		public static readonly DebugUI.Widget.NameAndTooltip VisibilityMaskReadbackMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Visibility Mask Readback Mode",
			tooltip = "Select which passes the visibility mask info is used to cull draw calls."
		};

		public static readonly DebugUI.Widget.NameAndTooltip UseForIndirectRenderingSystem = new DebugUI.Widget.NameAndTooltip
		{
			name = "Use for Indirect Rendering System",
			tooltip = ""
		};

		public static readonly DebugUI.Widget.NameAndTooltip ShowOcclusionTest = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Occlusion Test",
			tooltip = "Show the Occlusion Culling Test overlay."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OcclusionTestCountRange = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occlusion Test Count Range",
			tooltip = "The Occlusion Culling Test visualized range's limit."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OcclusionTestOpacity = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occlusion Test Opacity",
			tooltip = "The opacity of the Occlusion Culling Test overlay."
		};
	}

	private static class WidgetFactory
	{
		private const float kRefreshRate = 0.2f;

		private const string kFormatString = "{0}";

		internal static DebugUI.Widget CreateEnabled(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.Enabled,
				getter = () => data.Enabled,
				setter = delegate(bool value)
				{
					data.Enabled = value;
				}
			};
		}

		internal static DebugUI.Widget CreateMaterialCount(DebugDisplaySettingsGPUDrivenBRG data)
		{
			DebugUI.ValueTuple valueTuple = new DebugUI.ValueTuple();
			valueTuple.displayName = "Material Count";
			valueTuple.values = new DebugUI.Value[1]
			{
				new DebugUI.Value
				{
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => data.MaterialCount
				}
			};
			return valueTuple;
		}

		internal static DebugUI.Widget CreateMeshCount(DebugDisplaySettingsGPUDrivenBRG data)
		{
			DebugUI.ValueTuple valueTuple = new DebugUI.ValueTuple();
			valueTuple.displayName = "Mesh Count";
			valueTuple.values = new DebugUI.Value[1]
			{
				new DebugUI.Value
				{
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => data.MeshCount
				}
			};
			return valueTuple;
		}

		internal static DebugUI.Widget CreateLogResources(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.Button
			{
				nameAndTooltip = Strings.LogResources,
				action = data.LogResources
			};
		}

		internal static DebugUI.Widget CreateOverrideCameraToMain(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.OverrideCameraToMain,
				getter = () => data.DebugOverrideCameraToMain,
				setter = delegate(bool value)
				{
					data.DebugOverrideCameraToMain = value;
				}
			};
		}

		internal static DebugUI.Widget CreateOpaqueSortingCPU(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.OpaqueSortingCPU,
				getter = () => data.OpaqueSortingCPU,
				setter = delegate(bool value)
				{
					data.OpaqueSortingCPU = value;
				}
			};
		}

		internal static DebugUI.Widget CreateOpaqueSortingGPU(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.OpaqueSortingGPU,
				getter = () => data.OpaqueSortingGPU,
				setter = delegate(bool value)
				{
					data.OpaqueSortingGPU = value;
				}
			};
		}

		internal static DebugUI.Widget CreateCullingStats(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.CullingStats,
				getter = () => data.DebugCullingStats,
				setter = delegate(bool value)
				{
					data.DebugCullingStats = value;
				}
			};
		}

		internal static DebugUI.Widget CreateCullingStatsTable(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.Table
			{
				displayName = "View",
				isHiddenCallback = () => !data.DebugCullingStats,
				children = 
				{
					(DebugUI.Widget)new DebugUI.Table.Row
					{
						nameAndTooltip = Strings.CullingStatsMainPassRow,
						children = 
						{
							(DebugUI.Widget)CreateCell(() => data.GPUCullingStatsData.MainVisible, "Visible"),
							(DebugUI.Widget)CreateCell(() => data.CPUCullingStatsData.MainFrustumCulled[0], "Frustum Culled (CPU)"),
							(DebugUI.Widget)CreateCell(() => data.GPUCullingStatsData.MainFrustumCulled, "Frustum Culled (GPU)"),
							(DebugUI.Widget)CreateCell(() => data.GPUCullingStatsData.MainOcclusionCulled, "Occlusion Culled")
						}
					},
					(DebugUI.Widget)new DebugUI.Table.Row
					{
						nameAndTooltip = Strings.CullingStatsShadowsPassRow,
						children = 
						{
							(DebugUI.Widget)CreateCell(() => data.GPUCullingStatsData.ShadowVisible, "Visible"),
							(DebugUI.Widget)CreateCell(() => data.CPUCullingStatsData.ShadowFrustumCulled[0], "Frustum Culled (CPU)"),
							(DebugUI.Widget)CreateCell(() => data.GPUCullingStatsData.ShadowFrustumCulled, "Frustum Culled (GPU)"),
							(DebugUI.Widget)CreateCell(() => data.GPUCullingStatsData.ShadowOcclusionCulled, "Occlusion Culled")
						}
					}
				}
			};
			static DebugUI.Value CreateCell(Func<object> getter, string displayName = "")
			{
				return new DebugUI.Value
				{
					displayName = displayName,
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = getter
				};
			}
		}

		internal static DebugUI.Widget CreateSkipRendering(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.SkipRendering,
				getter = () => data.DebugSkipRendering,
				setter = delegate(bool value)
				{
					data.DebugSkipRendering = value;
				}
			};
		}

		internal static DebugUI.Widget CreateSkipSubmittingDrawCommands(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.SkipSubmittingDrawCommands,
				getter = () => data.DebugSkipSubmittingDrawCommands,
				setter = delegate(bool value)
				{
					data.DebugSkipSubmittingDrawCommands = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDrawOneByOne(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawOneByOne,
				getter = () => data.DebugDrawOneByOne,
				setter = delegate(bool value)
				{
					data.DebugDrawOneByOne = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDecalSorting(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DecalSorting,
				getter = () => data.DebugDecalSorting,
				setter = delegate(bool value)
				{
					data.DebugDecalSorting = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDecalBatching(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DecalBatching,
				getter = () => data.DebugDecalBatching,
				setter = delegate(bool value)
				{
					data.DebugDecalBatching = value;
				},
				isHiddenCallback = () => !data.DebugDecalSorting
			};
		}

		internal static DebugUI.Widget CreateDisableGroupSorting(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DisableGroupSorting,
				getter = () => data.DebugDisableGroupSorting,
				setter = delegate(bool value)
				{
					data.DebugDisableGroupSorting = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDisableGroupMerging(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DisableGroupMerging,
				getter = () => data.DebugDisableGroupMerging,
				setter = delegate(bool value)
				{
					data.DebugDisableGroupMerging = value;
				}
			};
		}

		internal static DebugUI.Widget CreateForceBuildRenderGroupsEachFrame(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ForceBuildRenderGroupsEachFrame,
				getter = () => data.DebugForceBuildRenderGroupsEachFrame,
				setter = delegate(bool value)
				{
					data.DebugForceBuildRenderGroupsEachFrame = value;
				}
			};
		}

		internal static DebugUI.Widget CreateViewTypeFilter(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.ViewTypeFilter,
				autoEnum = typeof(BatchCullingViewType),
				getter = () => (int)data.DebugViewTypeFilter,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugViewTypeFilter,
				setIndex = delegate(int value)
				{
					data.DebugViewTypeFilter = (BatchCullingViewType)value;
				}
			};
		}

		internal static DebugUI.Widget CreateUploadAllDataToGPU(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.GPUDataUploadDebugMode,
				autoEnum = typeof(GPUDrivenBRGDebug.DataUploadDebugMode),
				getter = () => (int)data.DebugGPUDataUploadDebugMode,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugGPUDataUploadDebugMode,
				setIndex = delegate(int value)
				{
					data.DebugGPUDataUploadDebugMode = (GPUDrivenBRGDebug.DataUploadDebugMode)value;
				}
			};
		}

		public static DebugUI.Widget CreateLogRendererGroups(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.Button
			{
				nameAndTooltip = Strings.LogRendererGroups,
				action = data.LogRendererGroups
			};
		}

		internal static DebugUI.Widget CreateOnModifiedRendererGroups(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.Button
			{
				nameAndTooltip = Strings.OnModifiedRendererGroups,
				action = data.OnModifiedRendererGroups
			};
		}

		internal static DebugUI.Widget CreateOcclusionCulling(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.OcclusionCulling,
				getter = () => data.OcclusionCulling,
				setter = delegate(bool value)
				{
					data.OcclusionCulling = value;
				}
			};
		}

		internal static DebugUI.Widget CreateOccludeeUseAABB(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.OccludeeUseAABB,
				getter = () => data.OccludeeUseAABB,
				setter = delegate(bool value)
				{
					data.OccludeeUseAABB = value;
				},
				isHiddenCallback = () => !data.OcclusionCulling
			};
		}

		internal static DebugUI.Widget CreateDepthReprojection(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DepthReprojection,
				getter = () => data.DepthReprojection,
				setter = delegate(bool value)
				{
					data.DepthReprojection = value;
				}
			};
		}

		internal static DebugUI.Widget CreateVisibilityMaskReadbackMode(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.VisibilityMaskReadbackMode,
				autoEnum = typeof(GPUDrivenVisibilityMaskReadbackMode),
				getter = () => (int)data.VisibilityMaskReadbackMode,
				setter = delegate
				{
				},
				getIndex = () => (int)data.VisibilityMaskReadbackMode,
				setIndex = delegate(int value)
				{
					data.VisibilityMaskReadbackMode = (GPUDrivenVisibilityMaskReadbackMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateUseForIndirectRenderingSystem(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.UseForIndirectRenderingSystem,
				getter = () => data.UseForIndirectRenderingSystem,
				setter = delegate(bool value)
				{
					data.UseForIndirectRenderingSystem = value;
				}
			};
		}

		internal static DebugUI.Widget CreateShowOcclusionTest(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ShowOcclusionTest,
				getter = () => data.DebugShowOcclusionTest,
				setter = delegate(bool value)
				{
					data.DebugShowOcclusionTest = value;
				},
				isHiddenCallback = () => !data.OcclusionCulling
			};
		}

		internal static DebugUI.Widget CreateOcclusionTestCountRange(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.IntField
			{
				nameAndTooltip = Strings.OcclusionTestCountRange,
				getter = () => data.DebugOcclusionTestCountRange,
				setter = delegate(int value)
				{
					data.DebugOcclusionTestCountRange = value;
				},
				isHiddenCallback = () => !data.OcclusionCulling || !data.DebugShowOcclusionTest
			};
		}

		internal static DebugUI.Widget CreateOcclusionTestOpacity(DebugDisplaySettingsGPUDrivenBRG data)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.OcclusionTestOpacity,
				getter = () => data.DebugOcclusionTestOpacity,
				setter = delegate(float value)
				{
					data.DebugOcclusionTestOpacity = value;
				},
				isHiddenCallback = () => !data.OcclusionCulling || !data.DebugShowOcclusionTest,
				min = () => 0f,
				max = () => 1f
			};
		}
	}

	private class SettingsPanel : DebugDisplaySettingsPanel
	{
		public override string PanelName => "GPU Driven BRG";

		public SettingsPanel(DebugDisplaySettingsGPUDrivenBRG data)
		{
			AddWidget(WidgetFactory.CreateEnabled(data));
			AddWidget(new DebugUI.Container
			{
				children = 
				{
					(DebugUI.Widget)new DebugUI.Foldout
					{
						displayName = "Resources",
						isHeader = true,
						opened = true,
						children = 
						{
							WidgetFactory.CreateMaterialCount(data),
							WidgetFactory.CreateMeshCount(data),
							WidgetFactory.CreateLogResources(data)
						},
						isHiddenCallback = () => !data.Enabled
					},
					(DebugUI.Widget)new DebugUI.Foldout
					{
						displayName = "Culling",
						isHeader = true,
						opened = true,
						children = 
						{
							WidgetFactory.CreateOverrideCameraToMain(data),
							WidgetFactory.CreateOpaqueSortingCPU(data),
							WidgetFactory.CreateOpaqueSortingGPU(data),
							WidgetFactory.CreateSkipRendering(data),
							WidgetFactory.CreateSkipSubmittingDrawCommands(data),
							WidgetFactory.CreateDrawOneByOne(data),
							WidgetFactory.CreateDecalSorting(data),
							WidgetFactory.CreateDecalBatching(data),
							WidgetFactory.CreateDisableGroupSorting(data),
							WidgetFactory.CreateDisableGroupMerging(data),
							WidgetFactory.CreateForceBuildRenderGroupsEachFrame(data),
							WidgetFactory.CreateViewTypeFilter(data),
							WidgetFactory.CreateUploadAllDataToGPU(data),
							(DebugUI.Widget)new DebugUI.Foldout
							{
								displayName = "Occlusion Culling",
								isHeader = true,
								opened = true,
								children = 
								{
									WidgetFactory.CreateOcclusionCulling(data),
									WidgetFactory.CreateOccludeeUseAABB(data),
									WidgetFactory.CreateDepthReprojection(data),
									WidgetFactory.CreateVisibilityMaskReadbackMode(data),
									WidgetFactory.CreateUseForIndirectRenderingSystem(data),
									WidgetFactory.CreateShowOcclusionTest(data),
									WidgetFactory.CreateOcclusionTestCountRange(data),
									WidgetFactory.CreateOcclusionTestOpacity(data)
								}
							},
							(DebugUI.Widget)new DebugUI.Foldout
							{
								displayName = "Culling Stats",
								children = 
								{
									WidgetFactory.CreateCullingStats(data),
									WidgetFactory.CreateCullingStatsTable(data)
								}
							}
						}
					},
					(DebugUI.Widget)new DebugUI.Foldout
					{
						displayName = "RendererGroups",
						isHeader = true,
						opened = true,
						children = 
						{
							WidgetFactory.CreateLogRendererGroups(data),
							WidgetFactory.CreateOnModifiedRendererGroups(data)
						}
					}
				}
			});
		}
	}

	private readonly WaaaghDebugData m_DebugData;

	public int MaterialCount
	{
		get
		{
			if (!TryGetBRG(out var brg))
			{
				return 0;
			}
			return brg.ResourceRegistry.MaterialPool.Count;
		}
	}

	public int MeshCount
	{
		get
		{
			if (!TryGetBRG(out var brg))
			{
				return 0;
			}
			return brg.ResourceRegistry.MeshPool.Count;
		}
	}

	private bool Enabled
	{
		get
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				return pipelineAsset.GPUDrivenBRGSettings.IsEnabledAndSupported;
			}
			return false;
		}
		set
		{
			if (TryGetPipelineAsset(out var pipelineAsset) && pipelineAsset.GPUDrivenBRGSettings.SetEnabled(value))
			{
				pipelineAsset.Invalidate();
			}
		}
	}

	private bool DebugOverrideCameraToMain
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.OverrideCameraToMain;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.OverrideCameraToMain = value;
		}
	}

	private bool OpaqueSortingCPU
	{
		get
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				return pipelineAsset.GPUDrivenBRGSettings.OpaqueSortingCPU;
			}
			return false;
		}
		set
		{
			if (TryGetPipelineAsset(out var pipelineAsset) && pipelineAsset.GPUDrivenBRGSettings.OpaqueSortingCPU != value)
			{
				pipelineAsset.GPUDrivenBRGSettings.OpaqueSortingCPU = value;
				pipelineAsset.Invalidate();
			}
		}
	}

	private bool OpaqueSortingGPU
	{
		get
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				return pipelineAsset.GPUDrivenBRGSettings.OpaqueSortingGPU;
			}
			return false;
		}
		set
		{
			if (TryGetPipelineAsset(out var pipelineAsset) && pipelineAsset.GPUDrivenBRGSettings.OpaqueSortingGPU != value)
			{
				pipelineAsset.GPUDrivenBRGSettings.OpaqueSortingGPU = value;
				pipelineAsset.Invalidate();
			}
		}
	}

	private GPUDrivenCullingPassSharedData.CPUCullingStatsData CPUCullingStatsData
	{
		get
		{
			if (!TryGetBRG(out var brg))
			{
				return default(GPUDrivenCullingPassSharedData.CPUCullingStatsData);
			}
			return brg.SharedPassData.CPUCullingStats;
		}
	}

	private GPUDrivenCullingPassSharedData.GPUCullingStatsData GPUCullingStatsData
	{
		get
		{
			if (!TryGetBRG(out var brg))
			{
				return default(GPUDrivenCullingPassSharedData.GPUCullingStatsData);
			}
			return brg.SharedPassData.GPUCullingStats;
		}
	}

	private bool DebugSkipRendering
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.SkipRendering;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.SkipRendering = value;
		}
	}

	private bool DebugSkipSubmittingDrawCommands
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.SkipSubmittingDrawCommands;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.SkipSubmittingDrawCommands = value;
		}
	}

	private bool DebugDrawOneByOne
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.DrawOneByOne;
		}
		set
		{
			if (m_DebugData.GPUDrivenBRGDebug.DrawOneByOne != value)
			{
				m_DebugData.GPUDrivenBRGDebug.DrawOneByOne = value;
				if (TryGetBRG(out var brg))
				{
					brg.RendererGroupPool.OnModifiedCPUData();
				}
			}
		}
	}

	private bool DebugDecalSorting
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.DecalSorting;
		}
		set
		{
			if (m_DebugData.GPUDrivenBRGDebug.DecalSorting != value)
			{
				m_DebugData.GPUDrivenBRGDebug.DecalSorting = value;
				if (TryGetBRG(out var brg))
				{
					brg.RendererGroupPool.OnModifiedCPUData();
				}
			}
		}
	}

	private bool DebugDecalBatching
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.DecalBatching;
		}
		set
		{
			if (m_DebugData.GPUDrivenBRGDebug.DecalBatching != value)
			{
				m_DebugData.GPUDrivenBRGDebug.DecalBatching = value;
				if (TryGetBRG(out var brg))
				{
					brg.RendererGroupPool.OnModifiedCPUData();
				}
			}
		}
	}

	private bool DebugDisableGroupSorting
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.DisableGroupSorting;
		}
		set
		{
			if (m_DebugData.GPUDrivenBRGDebug.DisableGroupSorting != value)
			{
				if (TryGetBRG(out var brg))
				{
					brg.RendererGroupPool.OnModifiedCPUData();
				}
				m_DebugData.GPUDrivenBRGDebug.DisableGroupSorting = value;
			}
		}
	}

	private bool DebugDisableGroupMerging
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.DisableGroupMerging;
		}
		set
		{
			if (m_DebugData.GPUDrivenBRGDebug.DisableGroupMerging != value)
			{
				if (TryGetBRG(out var brg))
				{
					brg.RendererGroupPool.OnModifiedCPUData();
				}
				m_DebugData.GPUDrivenBRGDebug.DisableGroupMerging = value;
			}
		}
	}

	private bool DebugForceBuildRenderGroupsEachFrame
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.ForceBuildRenderGroupsEachFrame;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.ForceBuildRenderGroupsEachFrame = value;
		}
	}

	private BatchCullingViewType DebugViewTypeFilter
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.ViewTypeFilter;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.ViewTypeFilter = value;
		}
	}

	private GPUDrivenBRGDebug.DataUploadDebugMode DebugGPUDataUploadDebugMode
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.GPUDataUploadDebugMode;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.GPUDataUploadDebugMode = value;
		}
	}

	private bool OcclusionCulling
	{
		get
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				return pipelineAsset.GPUDrivenBRGSettings.OcclusionCulling;
			}
			return false;
		}
		set
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				pipelineAsset.GPUDrivenBRGSettings.OcclusionCulling = value;
				pipelineAsset.Invalidate();
			}
		}
	}

	private bool OccludeeUseAABB
	{
		get
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				return pipelineAsset.GPUDrivenBRGSettings.OccludeeUseAABB;
			}
			return false;
		}
		set
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				pipelineAsset.GPUDrivenBRGSettings.OccludeeUseAABB = value;
			}
		}
	}

	private bool DepthReprojection
	{
		get
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				return pipelineAsset.GPUDrivenBRGSettings.DepthReprojection;
			}
			return false;
		}
		set
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				pipelineAsset.GPUDrivenBRGSettings.DepthReprojection = value;
				pipelineAsset.Invalidate();
			}
		}
	}

	private GPUDrivenVisibilityMaskReadbackMode VisibilityMaskReadbackMode
	{
		get
		{
			if (!TryGetPipelineAsset(out var pipelineAsset))
			{
				return GPUDrivenVisibilityMaskReadbackMode.Off;
			}
			return pipelineAsset.GPUDrivenBRGSettings.VisibilityMaskReadbackMode;
		}
		set
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				pipelineAsset.GPUDrivenBRGSettings.VisibilityMaskReadbackMode = value;
				pipelineAsset.Invalidate();
			}
		}
	}

	private bool UseForIndirectRenderingSystem
	{
		get
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				return pipelineAsset.GPUDrivenBRGSettings.UseForIndirectRenderingSystem;
			}
			return false;
		}
		set
		{
			if (TryGetPipelineAsset(out var pipelineAsset))
			{
				pipelineAsset.GPUDrivenBRGSettings.UseForIndirectRenderingSystem = value;
				pipelineAsset.Invalidate();
			}
		}
	}

	private bool DebugShowOcclusionTest
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.ShowOcclusionTest;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.ShowOcclusionTest = value;
		}
	}

	private int DebugOcclusionTestCountRange
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.OcclusionTestCountRange;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.OcclusionTestCountRange = math.clamp(value, 1, 1024);
		}
	}

	private float DebugOcclusionTestOpacity
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.OcclusionTestOpacity;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.OcclusionTestOpacity = math.saturate(value);
		}
	}

	private bool DebugCullingStats
	{
		get
		{
			return m_DebugData.GPUDrivenBRGDebug.CullingStats;
		}
		set
		{
			m_DebugData.GPUDrivenBRGDebug.CullingStats = value;
		}
	}

	public bool AreAnySettingsActive
	{
		get
		{
			if (!DebugOverrideCameraToMain && !DebugSkipRendering && !DebugSkipSubmittingDrawCommands && !DebugDrawOneByOne && !DebugDecalSorting && !DebugDecalBatching && !DebugDisableGroupSorting && !DebugDisableGroupMerging && !DebugForceBuildRenderGroupsEachFrame && DebugViewTypeFilter == BatchCullingViewType.Unknown && DebugGPUDataUploadDebugMode == GPUDrivenBRGDebug.DataUploadDebugMode.None && !DebugShowOcclusionTest)
			{
				return DebugCullingStats;
			}
			return true;
		}
	}

	public DebugDisplaySettingsGPUDrivenBRG(WaaaghDebugData debugData)
	{
		m_DebugData = debugData;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}

	private static bool TryGetPipelineAsset(out WaaaghPipelineAsset pipelineAsset)
	{
		pipelineAsset = WaaaghPipeline.Asset;
		return pipelineAsset != null;
	}

	private bool TryGetBRG(out GPUDrivenBatchRendererGroup brg, bool emitWarningOnNotFound = false)
	{
		brg = m_DebugData.Pipeline.GPUDrivenBatchRendererGroup;
		GPUDrivenBatchRendererGroup gPUDrivenBatchRendererGroup = brg;
		bool num = gPUDrivenBatchRendererGroup != null && gPUDrivenBatchRendererGroup.IsEnabledAndInitialized && gPUDrivenBatchRendererGroup.ResourceRegistry != null;
		if (!num && emitWarningOnNotFound)
		{
			Debug.LogWarning("GPU Driven BRG is disabled.");
		}
		return num;
	}

	private void LogResources()
	{
		if (!TryGetBRG(out var brg, emitWarningOnNotFound: true))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Clear();
		stringBuilder.AppendLine("BRG Materials:");
		foreach (GPUDrivenRegisteredResource<GPUDrivenResourceRegistry.MaterialKey> item in brg.ResourceRegistry.MaterialPool)
		{
			ref GPUDrivenResourceRegistry.ManagedMaterialInfo managedMaterialInfo = ref brg.ResourceRegistry.GetManagedMaterialInfo(item.IndexAllocation);
			ref GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = ref brg.ResourceRegistry.GetUnmanagedMaterialInfo(item.IndexAllocation);
			stringBuilder.AppendLine($"{managedMaterialInfo.OriginalMaterial.name}: RefCount={unmanagedMaterialInfo.ReferenceCount}; PropertyOverrideMask={unmanagedMaterialInfo.PropertyOverrideMask}");
		}
		Debug.Log(stringBuilder.ToString());
		stringBuilder.Clear();
		stringBuilder.AppendLine("BRG Meshes:");
		foreach (GPUDrivenRegisteredResource<GPUDrivenResourceRegistry.MeshKey> item2 in brg.ResourceRegistry.MeshPool)
		{
			ref GPUDrivenResourceRegistry.ManagedMeshInfo managedMeshInfo = ref brg.ResourceRegistry.GetManagedMeshInfo(item2.IndexAllocation);
			ref GPUDrivenResourceRegistry.UnmanagedMeshInfo unmanagedMeshInfo = ref brg.ResourceRegistry.GetUnmanagedMeshInfo(item2.IndexAllocation);
			stringBuilder.AppendLine($"{managedMeshInfo.Mesh.name}: Submesh={unmanagedMeshInfo.SubmeshIndex}/{unmanagedMeshInfo.MaxSubmeshCount}; RefCount={unmanagedMeshInfo.ReferenceCount}");
		}
		Debug.Log(stringBuilder.ToString());
	}

	private void LogRendererGroups()
	{
		if (!TryGetBRG(out var brg2, emitWarningOnNotFound: true))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Renderer groups ({brg2.ActiveRendererGroups.Count} total):");
		int num = 0;
		int num2 = 0;
		foreach (KVPair<GPUDrivenRendererGroupPool.RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation> activeRendererGroup in brg2.ActiveRendererGroups)
		{
			GPUDrivenIndexAllocator.IndexAllocation value = activeRendererGroup.Value;
			int length = brg2.ReadRendererGroupIndices(value).Length;
			stringBuilder.Append($"(Alloc={{{value.Index}:{value.Generation}}}): {length} indices. ");
			num += length;
			if (length == 0)
			{
				num2++;
			}
		}
		stringBuilder.AppendLine();
		Debug.Log($"Renderer groups store {num} indices in total ({(float)num / (float)brg2.ActiveRendererGroups.Count:F2} on average)).");
		Debug.Log($"Detected {num2} empty renderer groups.");
		GPUDrivenRendererGroupPool.ViewTypeInfo[] viewTypeInfos = brg2.RendererGroupPool.ViewTypeInfos;
		for (int i = 0; i < viewTypeInfos.Length; i++)
		{
			GPUDrivenRendererGroupPool.ViewTypeInfo viewTypeInfo = viewTypeInfos[i];
			LogViewGroupSlices(brg2, viewTypeInfo.ViewType);
		}
		Debug.Log(stringBuilder.ToString());
		static void LogViewGroupSlices(GPUDrivenBatchRendererGroup brg, GPUDrivenRendererGroupPool.ViewType viewType)
		{
			NativeArray<GPUDrivenRendererGroupPool.RendererGroupSlice>.ReadOnly allRendererGroupSlicesReadonly = brg.RendererGroupPool.GetAllRendererGroupSlicesReadonly(viewType);
			int num3 = 0;
			foreach (GPUDrivenRendererGroupPool.RendererGroupSlice item in allRendererGroupSlicesReadonly)
			{
				num3 += item.IndexCount;
			}
			Debug.Log($"{viewType} view has {num3} indices and {allRendererGroupSlicesReadonly.Length} renderer group slices ({(float)num3 / (float)allRendererGroupSlicesReadonly.Length} on average).");
		}
	}

	private void OnModifiedRendererGroups()
	{
		if (TryGetBRG(out var brg, emitWarningOnNotFound: true))
		{
			brg.RendererGroupPool.OnModifiedCPUData();
		}
	}
}
