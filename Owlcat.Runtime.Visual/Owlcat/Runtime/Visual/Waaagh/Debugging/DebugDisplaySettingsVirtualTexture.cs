using System;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.VirtualTexture.Atlas;
using Owlcat.Runtime.Visual.VirtualTexture.Streaming;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class DebugDisplaySettingsVirtualTexture : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	private static class Strings
	{
		public static DebugUI.Widget.NameAndTooltip ShowFeedback = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show",
			tooltip = "Shows debug layer for VT feedback buffer."
		};

		public static DebugUI.Widget.NameAndTooltip FeedbackScale = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Scale",
			tooltip = "Configure screen size of VT feedback buffer."
		};

		public static DebugUI.Widget.NameAndTooltip FeedbackMipBias = new DebugUI.Widget.NameAndTooltip
		{
			name = "Mip Bias",
			tooltip = "Mip bias added to feedback to prevent oversubscription."
		};

		public static DebugUI.Widget.NameAndTooltip FeedbackConsumption = new DebugUI.Widget.NameAndTooltip
		{
			name = "Consumption (read-only)",
			tooltip = "Current consumption level detected by the feedback."
		};

		public static DebugUI.Widget.NameAndTooltip DontLoadFeedback = new DebugUI.Widget.NameAndTooltip
		{
			name = "Don't Load Feedback",
			tooltip = "Stop loading feedback."
		};

		public static DebugUI.Widget.NameAndTooltip ShowPhysicalAtlas = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show",
			tooltip = "Shows debug layer with VT physical atlas."
		};

		public static DebugUI.Widget.NameAndTooltip ShowPhysicalAtlasSliceGrid = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Slice Grid",
			tooltip = "Shows VT physical slice grid."
		};

		public static DebugUI.Widget.NameAndTooltip PhysBufferScale = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Scale",
			tooltip = "Configure screen size of VT physical atlas."
		};

		public static DebugUI.Widget.NameAndTooltip PhysicalAtlasMaxSliceResolution = new DebugUI.Widget.NameAndTooltip
		{
			name = "Max Slice Resolution",
			tooltip = "Configure the upper limit override of the physical atlas slice resolution."
		};

		public static DebugUI.Widget.NameAndTooltip PhysicalAtlasMemoryOverhead = new DebugUI.Widget.NameAndTooltip
		{
			name = "Memory Overhead (%, read-only)",
			tooltip = "Displays the amount of redundant memory in the physical atlas caused by per-tile and per-slice alignment."
		};

		public static DebugUI.Widget.NameAndTooltip ShowIndirectionTexture = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show",
			tooltip = "Shows Indirection texture debug layer"
		};

		public static DebugUI.Widget.NameAndTooltip IndirectTextureScale = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Scale",
			tooltip = "Configure screen size of VT indirect texture"
		};

		public static DebugUI.Widget.NameAndTooltip ShowBatchedCopyRt = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show",
			tooltip = "Shows debug layer with VT batched copy RT"
		};

		public static DebugUI.Widget.NameAndTooltip BatchedCopyRtScale = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Scale",
			tooltip = "Configure screen size of VT batched copy RT"
		};

		public static DebugUI.Widget.NameAndTooltip DebugTilesMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug Tiles Mode",
			tooltip = "Shows debug layer with VT tile borders. Color specifies either mip level or slice index, depending on the selected mode."
		};

		public static DebugUI.Widget.NameAndTooltip ShowVirtualAtlas = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show",
			tooltip = "Shows debug layer with VT virtual atlas."
		};

		public static DebugUI.Widget.NameAndTooltip VirtualAtlasScale = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Scale",
			tooltip = "Configure screen size of VT virtual atlas."
		};

		public static DebugUI.Widget.NameAndTooltip VirtualAtlasOccupancy = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occupancy (read-only)",
			tooltip = "Current normalized virtual atlas occupancy."
		};

		public static DebugUI.Widget.NameAndTooltip VirtualAtlasResolution = new DebugUI.Widget.NameAndTooltip
		{
			name = "Resolution (read-only)",
			tooltip = "Current virtual atlas resolution in tiles."
		};

		public static DebugUI.Widget.NameAndTooltip MaterialUpdateMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Material Update Mode",
			tooltip = "Override when to write VT material data."
		};

		public static DebugUI.Widget.NameAndTooltip TextureStacksInAtlasCount = new DebugUI.Widget.NameAndTooltip
		{
			name = "Texture Stacks in Atlas",
			tooltip = "How many texture stacks are registered in the Virtual Atlas."
		};

		public static DebugUI.Widget.NameAndTooltip DumpStreamingStats = new DebugUI.Widget.NameAndTooltip
		{
			name = "Dump streaming stats",
			tooltip = "Dumps streaming statistics to a temp file."
		};

		public static DebugUI.Widget.NameAndTooltip ResetVT = new DebugUI.Widget.NameAndTooltip
		{
			name = "Reset Virtual Atlas",
			tooltip = "Resets Virtual Atlas to default size."
		};

		public static DebugUI.Widget.NameAndTooltip CalulateTextureMemory = new DebugUI.Widget.NameAndTooltip
		{
			name = "Calculate Texture Memory",
			tooltip = "Calculate amount of memory needed to allocate all textures in materials which replaced by VT."
		};
	}

	private static class WidgetFactory
	{
		internal static DebugUI.Widget CreateDebugShowFeedback(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ShowFeedback,
				getter = () => data.DebugShowFeedback,
				setter = delegate(bool value)
				{
					data.DebugShowFeedback = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugFeedbackScale(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.Container
			{
				children = { (DebugUI.Widget)new DebugUI.FloatField
				{
					nameAndTooltip = Strings.FeedbackScale,
					getter = () => data.DebugFeedbackScale,
					setter = delegate(float value)
					{
						data.DebugFeedbackScale = value;
					},
					min = () => 0.1f,
					max = () => 20f,
					isHiddenCallback = () => !data.DebugShowFeedback
				} }
			};
		}

		internal static DebugUI.Widget CreateDebugFeedbackMipBias(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.FeedbackMipBias,
				getter = () => data.FeedbackMipBias,
				setter = delegate(float value)
				{
					data.FeedbackMipBias = value;
				},
				min = () => 0f,
				max = () => 10f,
				flags = DebugUI.Flags.EditorForceUpdate
			};
		}

		internal static DebugUI.Widget CreateDebugFeedbackConsumption(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.FeedbackConsumption,
				getter = () => data.FeedbackConsumption,
				setter = delegate
				{
				},
				min = () => 0f,
				max = () => 1f,
				flags = DebugUI.Flags.EditorForceUpdate
			};
		}

		internal static DebugUI.Widget CreateDontLoadFeedback(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DontLoadFeedback,
				getter = () => data.DontLoadFeedback,
				setter = delegate(bool value)
				{
					data.DontLoadFeedback = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugShowPhysicalAtlas(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ShowPhysicalAtlas,
				getter = () => data.ShowPhysicalAtlas,
				setter = delegate(bool value)
				{
					data.ShowPhysicalAtlas = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugShowPhysicalAtlasSliceGrid(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ShowPhysicalAtlasSliceGrid,
				getter = () => data.ShowPhysicalAtlasSliceGrid,
				setter = delegate(bool value)
				{
					data.ShowPhysicalAtlasSliceGrid = value;
				},
				isHiddenCallback = () => !data.ShowPhysicalAtlas
			};
		}

		internal static DebugUI.Widget CreateDebugPhysicalAtlasScale(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.Container
			{
				children = { (DebugUI.Widget)new DebugUI.FloatField
				{
					nameAndTooltip = Strings.PhysBufferScale,
					getter = () => data.PhysicalAtlasScale,
					setter = delegate(float value)
					{
						data.PhysicalAtlasScale = value;
					},
					min = () => 0.1f,
					max = () => 20f,
					isHiddenCallback = () => !data.ShowPhysicalAtlas
				} }
			};
		}

		internal static DebugUI.Widget CreateDebugPhysicalAtlasMaxSliceResolution(DebugDisplaySettingsVirtualTexture data)
		{
			SliceResolution[] sliceResolutions = (SliceResolution[])Enum.GetValues(typeof(SliceResolution));
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.PhysicalAtlasMaxSliceResolution,
				getter = () => (int)data.PhysicalAtlasMaxSliceResolution,
				setter = delegate
				{
				},
				autoEnum = typeof(SliceResolution),
				getIndex = () => Array.IndexOf(sliceResolutions, data.PhysicalAtlasMaxSliceResolution),
				setIndex = delegate(int index)
				{
					data.PhysicalAtlasMaxSliceResolution = sliceResolutions[index];
				}
			};
		}

		internal static DebugUI.Widget CreateDebugPhysicalAtlasMemoryOverhead(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.PhysicalAtlasMemoryOverhead,
				getter = () => data.PhysicalAtlasMemoryOverheadPercentage,
				setter = delegate
				{
				}
			};
		}

		internal static DebugUI.Widget CreateDebugShowIndirectionTexture(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ShowIndirectionTexture,
				getter = () => data.ShowIndirectionTexture,
				setter = delegate(bool value)
				{
					data.ShowIndirectionTexture = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugIndirectTextureScale(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.Container
			{
				children = { (DebugUI.Widget)new DebugUI.FloatField
				{
					nameAndTooltip = Strings.IndirectTextureScale,
					getter = () => data.IndirectTextureScale,
					setter = delegate(float value)
					{
						data.IndirectTextureScale = value;
					},
					min = () => 0.1f,
					max = () => 20f,
					isHiddenCallback = () => !data.ShowIndirectionTexture
				} }
			};
		}

		internal static DebugUI.Widget CreateDebugTilesMode(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.DebugTilesMode,
				autoEnum = typeof(DebugTilesMode),
				getter = () => (int)data.DebugTilesMode,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugTilesMode,
				setIndex = delegate(int value)
				{
					data.DebugTilesMode = (DebugTilesMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugShowBatchedCopyRt(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ShowBatchedCopyRt,
				getter = () => data.ShowBatchedCopyRt,
				setter = delegate(bool value)
				{
					data.ShowBatchedCopyRt = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugBatchedCopyRtScale(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.Container
			{
				children = { (DebugUI.Widget)new DebugUI.FloatField
				{
					nameAndTooltip = Strings.BatchedCopyRtScale,
					getter = () => data.BatchedCopyRtScale,
					setter = delegate(float value)
					{
						data.BatchedCopyRtScale = value;
					},
					min = () => 0.1f,
					max = () => 20f,
					isHiddenCallback = () => !data.ShowBatchedCopyRt
				} }
			};
		}

		internal static DebugUI.Widget CreateDebugShowVirtualAtlas(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ShowVirtualAtlas,
				getter = () => data.ShowVirtualAtlas,
				setter = delegate(bool value)
				{
					data.ShowVirtualAtlas = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugVirtualAtlasScale(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.Container
			{
				children = { (DebugUI.Widget)new DebugUI.FloatField
				{
					nameAndTooltip = Strings.VirtualAtlasScale,
					getter = () => data.VirtualAtlasScale,
					setter = delegate(float value)
					{
						data.VirtualAtlasScale = value;
					},
					min = () => 0.1f,
					max = () => 20f,
					isHiddenCallback = () => !data.ShowVirtualAtlas
				} }
			};
		}

		internal static DebugUI.Widget CreateDebugVirtualAtlasOccupancy(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.VirtualAtlasOccupancy,
				getter = () => data.VirtualAtlasOccupancy,
				setter = delegate
				{
				}
			};
		}

		internal static DebugUI.Widget CreateDebugVirtualAtlasResolution(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.Value
			{
				nameAndTooltip = Strings.VirtualAtlasResolution,
				getter = () => data.VirtualAtlasResolution
			};
		}

		internal static DebugUI.Widget CreateMaterialUpdateMode(DebugDisplaySettingsVirtualTexture data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.MaterialUpdateMode,
				autoEnum = typeof(VirtualTextureMaterialUpdateMode),
				getter = () => (int)data.MaterialUpdateMode,
				setter = delegate
				{
				},
				getIndex = () => (int)data.MaterialUpdateMode,
				setIndex = delegate(int value)
				{
					data.MaterialUpdateMode = (VirtualTextureMaterialUpdateMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateMaterialsInAtlas(DebugDisplaySettingsVirtualTexture data)
		{
			DebugUI.ValueTuple valueTuple = new DebugUI.ValueTuple();
			valueTuple.nameAndTooltip = Strings.TextureStacksInAtlasCount;
			valueTuple.values = new DebugUI.Value[1]
			{
				new DebugUI.Value
				{
					formatString = "{0}",
					getter = () => data.TextureStacksInAtlasCount
				}
			};
			return valueTuple;
		}
	}

	private class SettingsPanel : DebugDisplaySettingsPanel
	{
		public override string PanelName => "Virtual Texture";

		public SettingsPanel(DebugDisplaySettingsVirtualTexture data)
		{
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Feedback",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateDebugShowFeedback(data),
					WidgetFactory.CreateDebugFeedbackScale(data),
					WidgetFactory.CreateDebugFeedbackMipBias(data),
					WidgetFactory.CreateDebugFeedbackConsumption(data),
					WidgetFactory.CreateDontLoadFeedback(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Physical Atlas",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateDebugShowPhysicalAtlas(data),
					WidgetFactory.CreateDebugShowPhysicalAtlasSliceGrid(data),
					WidgetFactory.CreateDebugPhysicalAtlasScale(data),
					WidgetFactory.CreateDebugPhysicalAtlasMaxSliceResolution(data),
					WidgetFactory.CreateDebugPhysicalAtlasMemoryOverhead(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Batched Copy RT",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateDebugShowBatchedCopyRt(data),
					WidgetFactory.CreateDebugBatchedCopyRtScale(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Indirection Texture",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateDebugShowIndirectionTexture(data),
					WidgetFactory.CreateDebugIndirectTextureScale(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Virtual Atlas",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateDebugShowVirtualAtlas(data),
					WidgetFactory.CreateDebugVirtualAtlasScale(data),
					WidgetFactory.CreateDebugVirtualAtlasOccupancy(data),
					WidgetFactory.CreateDebugVirtualAtlasResolution(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Materials",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateMaterialUpdateMode(data),
					WidgetFactory.CreateMaterialsInAtlas(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Misc",
				isHeader = true,
				opened = true,
				children = { WidgetFactory.CreateDebugTilesMode(data) }
			});
		}
	}

	private WaaaghDebugData m_DebugData;

	public bool AreAnySettingsActive
	{
		get
		{
			if (!DebugShowFeedback && !(DontLoadFeedback | ShowPhysicalAtlas) && !ShowPhysicalAtlasSliceGrid && PhysicalAtlasMaxSliceResolution == SliceResolution.MaximumAvailable && !ShowIndirectionTexture && DebugTilesMode == DebugTilesMode.None && !ShowBatchedCopyRt && !ShowVirtualAtlas)
			{
				return MaterialUpdateMode != VirtualTextureMaterialUpdateMode.Default;
			}
			return true;
		}
	}

	public bool DebugShowFeedback
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.ShowFeedback;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.ShowFeedback = value;
		}
	}

	public float DebugFeedbackScale
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.FeedbackScale;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.FeedbackScale = value;
		}
	}

	public bool DontLoadFeedback
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.DontLoadFeedback;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.DontLoadFeedback = value;
		}
	}

	public bool ShowPhysicalAtlas
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.ShowPhysicalAtlas;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.ShowPhysicalAtlas = value;
		}
	}

	public bool ShowPhysicalAtlasSliceGrid
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.ShowPhysicalAtlasSliceGrid;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.ShowPhysicalAtlasSliceGrid = value;
		}
	}

	public float PhysicalAtlasScale
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.PhyscalAtlasScale;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.PhyscalAtlasScale = value;
		}
	}

	public SliceResolution PhysicalAtlasMaxSliceResolution
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.PhysicalAtlasMaxSliceResolution;
		}
		set
		{
			if (m_DebugData.VirtualTextureDebug.PhysicalAtlasMaxSliceResolution != value)
			{
				m_DebugData.VirtualTextureDebug.PhysicalAtlasMaxSliceResolution = value;
				InvalidateRenderPipeline();
			}
		}
	}

	public float PhysicalAtlasMemoryOverheadPercentage
	{
		get
		{
			VirtualTextureManager virtualTextureManager = m_DebugData.Pipeline.VirtualTextureManager;
			if (virtualTextureManager == null)
			{
				return 0f;
			}
			int num = virtualTextureManager.PhysicalAtlasRequestedSizeInMegaBytes * 1024 * 1024;
			int num2 = virtualTextureManager.PhysicalAtlasResolution.TotalSizeInBytes() - num;
			return 100f * ((float)num2 / (float)num);
		}
	}

	public bool ShowIndirectionTexture
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.ShowIndirectionTexture;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.ShowIndirectionTexture = value;
		}
	}

	public float IndirectTextureScale
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.IndirectTextureScale;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.IndirectTextureScale = value;
		}
	}

	public DebugTilesMode DebugTilesMode
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.DebugTilesMode;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.DebugTilesMode = value;
		}
	}

	public bool ShowBatchedCopyRt
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.ShowBatchedCopyRt;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.ShowBatchedCopyRt = value;
		}
	}

	public float BatchedCopyRtScale
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.BatchedCopyRtScale;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.BatchedCopyRtScale = value;
		}
	}

	public float FeedbackMipBias
	{
		get
		{
			return m_DebugData.Pipeline.VirtualTextureManager?.FeedbackConsumptionTracker.MipBias ?? 0f;
		}
		set
		{
			if (m_DebugData.Pipeline.VirtualTextureManager != null)
			{
				m_DebugData.Pipeline.VirtualTextureManager.FeedbackConsumptionTracker.MipBias = value;
			}
		}
	}

	public float FeedbackConsumption => m_DebugData.Pipeline.VirtualTextureManager?.FeedbackConsumptionTracker.FindMaxOfConsumptionMemory() ?? 0f;

	public bool ShowVirtualAtlas
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.ShowVirtualAtlas;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.ShowVirtualAtlas = value;
		}
	}

	public float VirtualAtlasScale
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.VirtualAtlasScale;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.VirtualAtlasScale = value;
		}
	}

	public float VirtualAtlasOccupancy => m_DebugData.Pipeline.VirtualTextureManager?.VirtualAtlasOccupancy ?? 0f;

	public int2 VirtualAtlasResolution => m_DebugData.Pipeline.VirtualTextureManager?.VirtualAtlasResolutionInTiles ?? ((int2)0);

	public VirtualTextureMaterialUpdateMode MaterialUpdateMode
	{
		get
		{
			return m_DebugData.VirtualTextureDebug.MaterialUpdateMode;
		}
		set
		{
			m_DebugData.VirtualTextureDebug.MaterialUpdateMode = value;
		}
	}

	public int TextureStacksInAtlasCount => m_DebugData.Pipeline.VirtualTextureManager?.TextureStacksInAtlasCount ?? 0;

	public TileUploader TileUploader => m_DebugData.Pipeline.VirtualTextureManager?.TileUploader;

	private void ResetVT()
	{
		m_DebugData.Pipeline.VirtualTextureManager?.Reset();
	}

	private void CalculateTextureMemory()
	{
		m_DebugData.Pipeline.VirtualTextureManager?.CalculateAndLogTextureMemory();
	}

	public DebugDisplaySettingsVirtualTexture(WaaaghDebugData debugData)
	{
		m_DebugData = debugData;
	}

	private static void InvalidateRenderPipeline()
	{
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if (asset != null)
		{
			asset.Invalidate();
		}
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
