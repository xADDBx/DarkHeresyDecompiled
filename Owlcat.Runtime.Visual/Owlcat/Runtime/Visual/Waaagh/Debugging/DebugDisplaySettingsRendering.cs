using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

internal class DebugDisplaySettingsRendering : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	private static class Strings
	{
		public static readonly DebugUI.Widget.NameAndTooltip MapOverlay = new DebugUI.Widget.NameAndTooltip
		{
			name = "Map Overlays",
			tooltip = "Overlays render pipeline textures to validate the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MapSize = new DebugUI.Widget.NameAndTooltip
		{
			name = "Map Size",
			tooltip = "Set the size of the render pipeline texture in the scene"
		};

		public static readonly DebugUI.Widget.NameAndTooltip MaterialDebugMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Material Debug Mode",
			tooltip = "Use the drop-down to select which material information to overlay on the screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OverdrawMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Overdraw Mode",
			tooltip = "Debug anywhere pixels are overdrawn on top of each other."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DebugMipMap = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug MipMap",
			tooltip = "MipMap ratio rendering."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DebugStencilMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug Stencil Mode",
			tooltip = "Visualize Stencil Reference Value"
		};

		public static readonly DebugUI.Widget.NameAndTooltip DebugStencilFlags = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug Stencil Flags",
			tooltip = "Predefined Stencil Reference Values"
		};

		public static readonly DebugUI.Widget.NameAndTooltip DebugStencilRef = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug Stencil Ref",
			tooltip = "Stencil Ref"
		};

		public static readonly DebugUI.Widget.NameAndTooltip InvalidatePipeline = new DebugUI.Widget.NameAndTooltip
		{
			name = "Invalidate Pipeline"
		};
	}

	public static class WidgetFactory
	{
		internal static DebugUI.Widget CreateMapOverlays(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.MapOverlay,
				autoEnum = typeof(DebugMapOverlay),
				getter = () => (int)data.DebugMapOverlay,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugMapOverlay,
				setIndex = delegate(int value)
				{
					data.DebugMapOverlay = (DebugMapOverlay)value;
				}
			};
		}

		internal static DebugUI.Widget CreateMapSize(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.Container
			{
				children = { (DebugUI.Widget)new DebugUI.FloatField
				{
					nameAndTooltip = Strings.MapSize,
					getter = () => data.MapSize,
					setter = delegate(float value)
					{
						data.MapSize = value;
					},
					min = () => 0.01f,
					max = () => 1f,
					isHiddenCallback = () => data.DebugMapOverlay == DebugMapOverlay.None
				} }
			};
		}

		internal static DebugUI.Widget CreateBuffersDebugMode(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.MaterialDebugMode,
				autoEnum = typeof(DebugMaterialMode),
				getter = () => (int)data.DebugMaterialMode,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugMaterialMode,
				setIndex = delegate(int value)
				{
					data.DebugMaterialMode = (DebugMaterialMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateOverdrawMode(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.VBox
			{
				children = 
				{
					(DebugUI.Widget)new DebugUI.EnumField
					{
						nameAndTooltip = Strings.OverdrawMode,
						autoEnum = typeof(DebugOverdrawMode),
						getter = () => (int)data.OverdrawMode,
						setter = delegate
						{
						},
						getIndex = () => (int)data.OverdrawMode,
						setIndex = delegate(int value)
						{
							if (data.OverdrawMode != (DebugOverdrawMode)value)
							{
								data.OverdrawMode = (DebugOverdrawMode)value;
								if (GPUDrivenBatchRendererGroup.TryGetInstance(out var instance))
								{
									instance.RendererGroupPool.OnModifiedCPUData();
								}
							}
						}
					},
					(DebugUI.Widget)new DebugUI.Container
					{
						isHiddenCallback = () => data.OverdrawMode != DebugOverdrawMode.QuadOverdraw,
						children = 
						{
							(DebugUI.Widget)new DebugUI.IntField
							{
								displayName = "Max Quad Cost",
								tooltip = null,
								getter = () => data.QuadOverdrawSettings.MaxQuadCost,
								setter = delegate(int value)
								{
									data.QuadOverdrawSettings.MaxQuadCost = value;
								},
								min = () => 0,
								max = () => 100
							},
							(DebugUI.Widget)new DebugUI.EnumField
							{
								displayName = "Object Filter",
								autoEnum = typeof(QuadOverdrawObjectFilter),
								getter = () => (int)data.QuadOverdrawSettings.ObjectFilter,
								setter = delegate
								{
								},
								getIndex = () => (int)data.QuadOverdrawSettings.ObjectFilter,
								setIndex = delegate(int value)
								{
									data.QuadOverdrawSettings.ObjectFilter = (QuadOverdrawObjectFilter)value;
								}
							}
						}
					}
				}
			};
		}

		internal static DebugUI.Widget CreateDebugMipMap(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DebugMipMap,
				getter = () => data.DebugMipMap,
				setter = delegate(bool value)
				{
					data.DebugMipMap = value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugStencilMode(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.DebugStencilMode,
				autoEnum = typeof(StencilDebugType),
				getter = () => (int)data.DebugStencilType,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DebugStencilType,
				setIndex = delegate(int value)
				{
					data.DebugStencilType = (StencilDebugType)value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugStencilFlags(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.BitField
			{
				nameAndTooltip = Strings.DebugStencilFlags,
				enumType = typeof(StencilRef),
				getter = () => data.DebugStencilFlags,
				setter = delegate(Enum value)
				{
					data.DebugStencilFlags = (StencilRef)(object)value;
				}
			};
		}

		internal static DebugUI.Widget CreateDebugStencilRef(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.IntField
			{
				nameAndTooltip = Strings.DebugStencilRef,
				getter = () => data.DebugStencilRef,
				setter = delegate(int value)
				{
					data.DebugStencilRef = value;
				},
				min = () => 0,
				max = () => 255
			};
		}

		internal static DebugUI.Widget CreateInvalidatePipeline(DebugDisplaySettingsRendering data)
		{
			return new DebugUI.Button
			{
				nameAndTooltip = Strings.InvalidatePipeline,
				action = delegate
				{
					WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
					if (asset != null)
					{
						asset.Invalidate();
					}
				}
			};
		}
	}

	private class SettingsPanel : DebugDisplaySettingsPanel
	{
		public override string PanelName => "Rendering";

		public SettingsPanel(DebugDisplaySettingsRendering data)
		{
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Buffers Debug Modes",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateMapOverlays(data),
					WidgetFactory.CreateMapSize(data),
					WidgetFactory.CreateBuffersDebugMode(data),
					WidgetFactory.CreateOverdrawMode(data),
					WidgetFactory.CreateDebugMipMap(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Stencil Debug",
				isHeader = true,
				opened = true,
				children = 
				{
					WidgetFactory.CreateDebugStencilMode(data),
					WidgetFactory.CreateDebugStencilFlags(data),
					WidgetFactory.CreateDebugStencilRef(data)
				}
			});
			AddWidget(WidgetFactory.CreateInvalidatePipeline(data));
		}
	}

	private readonly WaaaghDebugData m_DebugData;

	public bool AreAnySettingsActive
	{
		get
		{
			if (DebugMapOverlay == DebugMapOverlay.None && DebugMaterialMode == DebugMaterialMode.None && OverdrawMode == DebugOverdrawMode.None && !DebugMipMap)
			{
				return DebugStencilType != StencilDebugType.None;
			}
			return true;
		}
	}

	public DebugMapOverlay DebugMapOverlay
	{
		get
		{
			return m_DebugData.RenderingDebug.DebugMapOverlay;
		}
		internal set
		{
			m_DebugData.RenderingDebug.DebugMapOverlay = value;
		}
	}

	public float MapSize
	{
		get
		{
			return m_DebugData.RenderingDebug.MapSize;
		}
		set
		{
			m_DebugData.RenderingDebug.MapSize = value;
		}
	}

	public DebugMaterialMode DebugMaterialMode
	{
		get
		{
			return m_DebugData.RenderingDebug.DebugMaterialMode;
		}
		internal set
		{
			m_DebugData.RenderingDebug.DebugMaterialMode = value;
		}
	}

	public DebugOverdrawMode OverdrawMode
	{
		get
		{
			return m_DebugData.RenderingDebug.OverdrawMode;
		}
		internal set
		{
			m_DebugData.RenderingDebug.OverdrawMode = value;
		}
	}

	public QuadOverdrawSettings QuadOverdrawSettings
	{
		get
		{
			return m_DebugData.RenderingDebug.QuadOverdrawSettings;
		}
		internal set
		{
			m_DebugData.RenderingDebug.QuadOverdrawSettings = value;
		}
	}

	public bool DebugMipMap
	{
		get
		{
			return m_DebugData.RenderingDebug.DebugMipMap;
		}
		internal set
		{
			m_DebugData.RenderingDebug.DebugMipMap = value;
		}
	}

	public StencilDebugType DebugStencilType
	{
		get
		{
			return m_DebugData.StencilDebug.StencilDebugType;
		}
		internal set
		{
			m_DebugData.StencilDebug.StencilDebugType = value;
		}
	}

	public StencilRef DebugStencilFlags
	{
		get
		{
			return m_DebugData.StencilDebug.Flags;
		}
		internal set
		{
			m_DebugData.StencilDebug.Flags = value;
		}
	}

	public int DebugStencilRef
	{
		get
		{
			return m_DebugData.StencilDebug.Ref;
		}
		internal set
		{
			m_DebugData.StencilDebug.Ref = value;
		}
	}

	public DebugDisplaySettingsRendering(WaaaghDebugData data)
	{
		m_DebugData = data;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
