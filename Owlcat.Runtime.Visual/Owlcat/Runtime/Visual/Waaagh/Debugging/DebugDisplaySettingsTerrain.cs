using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

internal sealed class DebugDisplaySettingsTerrain : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	private sealed class SettingsPanel : DebugDisplaySettingsPanel
	{
		private readonly DebugDisplaySettingsTerrain m_Settings;

		public override string PanelName => "Terrain";

		public SettingsPanel(DebugDisplaySettingsTerrain settings)
		{
			m_Settings = settings;
			AddWidget(CreateModeSection(settings));
			AddWidget(CreateSplatmapWeightModeSection(settings));
			AddWidget(CreatePvsHeatmapModeSection(settings));
			AddWidget(CreateSettingsSection(settings));
		}

		public override void Dispose()
		{
			base.Dispose();
		}

		private static DebugUI.Widget CreateModeSection(DebugDisplaySettingsTerrain data)
		{
			return new DebugUI.Container
			{
				children = 
				{
					(DebugUI.Widget)new DebugUI.BoolField
					{
						flags = DebugUI.Flags.None,
						displayName = "Enabled",
						getter = () => data.Enabled,
						setter = delegate(bool value)
						{
							data.Enabled = value;
						}
					},
					(DebugUI.Widget)new DebugUI.EnumField
					{
						flags = DebugUI.Flags.None,
						displayName = "Mode",
						getter = () => (int)data.Mode,
						setter = delegate
						{
						},
						getIndex = () => (int)data.Mode,
						setIndex = delegate(int value)
						{
							data.Mode = (TerrainDebugMode)value;
						},
						currentIndex = 0,
						autoEnum = typeof(TerrainDebugMode)
					},
					(DebugUI.Widget)new DebugUI.HBox()
				}
			};
		}

		private static DebugUI.Widget CreateSettingsSection(DebugDisplaySettingsTerrain data)
		{
			return new DebugUI.Container
			{
				displayName = "Common Settings",
				children = 
				{
					(DebugUI.Widget)new DebugUI.IntField
					{
						displayName = "Terrain Layer Count",
						getter = () => data.LayerCount,
						setter = delegate(int value)
						{
							data.LayerCount = value;
						},
						min = () => 1,
						max = () => 64
					},
					(DebugUI.Widget)new DebugUI.FloatField
					{
						displayName = "Opacity",
						getter = () => data.Opacity,
						setter = delegate(float value)
						{
							data.Opacity = value;
						},
						min = () => 0f,
						max = () => 1f
					},
					(DebugUI.Widget)new DebugUI.ColorField
					{
						displayName = "Gradient Min Color",
						getter = () => data.GradientMinColor,
						setter = delegate(Color value)
						{
							data.GradientMinColor = value;
						}
					},
					(DebugUI.Widget)new DebugUI.ColorField
					{
						displayName = "Gradient Max Color",
						getter = () => data.GradientMaxColor,
						setter = delegate(Color value)
						{
							data.GradientMaxColor = value;
						}
					},
					(DebugUI.Widget)new DebugUI.ColorField
					{
						displayName = "Back Color",
						getter = () => data.BackColor,
						setter = delegate(Color value)
						{
							data.BackColor = value;
						}
					}
				}
			};
		}

		private static DebugUI.Widget CreateSplatmapWeightModeSection(DebugDisplaySettingsTerrain data)
		{
			return new DebugUI.Container
			{
				displayName = "Mode Settings",
				isHiddenCallback = () => data.Mode != TerrainDebugMode.SplatMapWeight,
				children = 
				{
					(DebugUI.Widget)new DebugUI.IntField
					{
						displayName = "Layer",
						getter = () => data.LayerIndex,
						setter = delegate(int value)
						{
							data.LayerIndex = value;
						},
						min = () => -1,
						max = () => data.LayerCount - 1
					},
					(DebugUI.Widget)new DebugUI.FloatField
					{
						displayName = "Weight Min",
						getter = () => data.LayerWeightMin,
						setter = delegate(float value)
						{
							data.LayerWeightMin = value;
						},
						min = () => 0f,
						max = () => 1f
					},
					(DebugUI.Widget)new DebugUI.FloatField
					{
						displayName = "Weight Max",
						getter = () => data.LayerWeightMax,
						setter = delegate(float value)
						{
							data.LayerWeightMax = value;
						},
						min = () => 0f,
						max = () => 1f
					},
					CreateMaskSection(data),
					(DebugUI.Widget)new DebugUI.HBox()
				}
			};
		}

		private static DebugUI.Widget CreatePvsHeatmapModeSection(DebugDisplaySettingsTerrain data)
		{
			return new DebugUI.Container
			{
				displayName = "Mode Settings",
				isHiddenCallback = () => data.Mode != TerrainDebugMode.LayerBudgetOverrun,
				children = 
				{
					(DebugUI.Widget)new DebugUI.IntField
					{
						displayName = "Layer Count Budget",
						getter = () => data.PvsHeatmapCountThreshold,
						setter = delegate(int value)
						{
							data.PvsHeatmapCountThreshold = value;
						},
						min = () => 0,
						max = () => data.LayerCount
					},
					CreateMaskSection(data),
					(DebugUI.Widget)new DebugUI.HBox(),
					(DebugUI.Widget)new DebugUI.HBox(),
					(DebugUI.Widget)new DebugUI.HBox
					{
						children = 
						{
							(DebugUI.Widget)new DebugUI.HBox(),
							(DebugUI.Widget)new DebugUI.HBox(),
							(DebugUI.Widget)new DebugUI.Button
							{
								displayName = "Refresh Heatmap",
								action = data.RequestHeatmapRefresh
							}
						}
					},
					(DebugUI.Widget)new DebugUI.HBox()
				}
			};
		}

		private static DebugUI.Widget CreateMaskSection(DebugDisplaySettingsTerrain data)
		{
			return new DebugUI.VBox
			{
				children = 
				{
					(DebugUI.Widget)new DebugUI.EnumField
					{
						displayName = "Splatmap Mask",
						getter = () => (int)data.SplatMapMaskType,
						setter = delegate
						{
						},
						getIndex = () => (int)data.SplatMapMaskType,
						setIndex = delegate(int value)
						{
							data.SplatMapMaskType = (TerrainDebug.MaskType)value;
						},
						currentIndex = 0,
						autoEnum = typeof(TerrainDebug.MaskType)
					},
					(DebugUI.Widget)new DebugUI.EnumField
					{
						displayName = "Splatmap Mask Position",
						getter = () => (int)data.SplatMapMaskPositionType,
						setter = delegate
						{
						},
						getIndex = () => (int)data.SplatMapMaskPositionType,
						setIndex = delegate(int value)
						{
							data.SplatMapMaskPositionType = (TerrainDebug.MaskPositionType)value;
						},
						currentIndex = 0,
						autoEnum = typeof(TerrainDebug.MaskPositionType)
					},
					(DebugUI.Widget)new DebugUI.FloatField
					{
						displayName = "Splatmap Mask Radius",
						isHiddenCallback = () => data.SplatMapMaskType != TerrainDebug.MaskType.Custom,
						getter = () => data.SplatMapMaskCustomRadius,
						setter = delegate(float value)
						{
							data.SplatMapMaskCustomRadius = value;
						},
						min = () => 0f
					}
				}
			};
		}
	}

	private readonly WaaaghDebugData m_Data;

	public bool AreAnySettingsActive => Enabled;

	public bool Enabled
	{
		get
		{
			return m_Data.TerrainDebug.Enabled;
		}
		set
		{
			m_Data.TerrainDebug.Enabled = value;
		}
	}

	public TerrainDebugMode Mode
	{
		get
		{
			return m_Data.TerrainDebug.DebugMode;
		}
		set
		{
			m_Data.TerrainDebug.DebugMode = value;
		}
	}

	public Color GradientMinColor
	{
		get
		{
			return m_Data.TerrainDebug.GradientMinColor;
		}
		set
		{
			m_Data.TerrainDebug.GradientMinColor = value;
		}
	}

	public Color GradientMaxColor
	{
		get
		{
			return m_Data.TerrainDebug.GradientMaxColor;
		}
		set
		{
			m_Data.TerrainDebug.GradientMaxColor = value;
		}
	}

	public Color BackColor
	{
		get
		{
			return m_Data.TerrainDebug.BackColor;
		}
		set
		{
			m_Data.TerrainDebug.BackColor = value;
		}
	}

	public float Opacity
	{
		get
		{
			return m_Data.TerrainDebug.Opacity;
		}
		set
		{
			m_Data.TerrainDebug.Opacity = value;
		}
	}

	public int LayerCount
	{
		get
		{
			return m_Data.TerrainDebug.LayerCount;
		}
		set
		{
			m_Data.TerrainDebug.LayerCount = value;
		}
	}

	public int LayerIndex
	{
		get
		{
			return m_Data.TerrainDebug.LayerIndex;
		}
		set
		{
			m_Data.TerrainDebug.LayerIndex = value;
		}
	}

	public float LayerWeightMin
	{
		get
		{
			return m_Data.TerrainDebug.LayerWeightMin;
		}
		set
		{
			m_Data.TerrainDebug.LayerWeightMin = value;
		}
	}

	public float LayerWeightMax
	{
		get
		{
			return m_Data.TerrainDebug.LayerWeightMax;
		}
		set
		{
			m_Data.TerrainDebug.LayerWeightMax = value;
		}
	}

	public int PvsHeatmapCountThreshold
	{
		get
		{
			return m_Data.TerrainDebug.PvsHeatmapCountThreshold;
		}
		set
		{
			m_Data.TerrainDebug.PvsHeatmapCountThreshold = value;
		}
	}

	public TerrainDebug.MaskType SplatMapMaskType
	{
		get
		{
			return m_Data.TerrainDebug.SplatMapMaskType;
		}
		set
		{
			m_Data.TerrainDebug.SplatMapMaskType = value;
		}
	}

	public TerrainDebug.MaskPositionType SplatMapMaskPositionType
	{
		get
		{
			return m_Data.TerrainDebug.SplatMaskPositionType;
		}
		set
		{
			m_Data.TerrainDebug.SplatMaskPositionType = value;
		}
	}

	public Vector3 SplatMapMaskPosition
	{
		get
		{
			return m_Data.TerrainDebug.SplatMapMaskCustomPosition;
		}
		set
		{
			m_Data.TerrainDebug.SplatMapMaskCustomPosition = value;
		}
	}

	public float SplatMapMaskCustomRadius
	{
		get
		{
			return m_Data.TerrainDebug.SplatMapMaskCustomRadius;
		}
		set
		{
			m_Data.TerrainDebug.SplatMapMaskCustomRadius = value;
		}
	}

	public DebugDisplaySettingsTerrain(WaaaghDebugData data)
	{
		m_Data = data;
	}

	public void RequestHeatmapRefresh()
	{
		m_Data.TerrainDebug.HeatmapRefreshRequested = true;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
