using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.XPBD;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual;

public class DebugDisplaySettingsXPBD : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	private static class Strings
	{
		public static readonly DebugUI.Widget.NameAndTooltip GizmosEnabled = new DebugUI.Widget.NameAndTooltip
		{
			name = "Gizmos Enabled",
			tooltip = "Enables debug gizmos for XPBD simulation."
		};

		public static readonly DebugUI.Widget.NameAndTooltip UseDepthBuffer = new DebugUI.Widget.NameAndTooltip
		{
			name = "Use Depth Buffer",
			tooltip = "Use the depth buffer when rendering gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ShowStats = new DebugUI.Widget.NameAndTooltip
		{
			name = "Show Stats",
			tooltip = "Show XPBD simulation statistics."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawParticles = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Particles",
			tooltip = "Draws particle gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawVelocities = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Velocities",
			tooltip = "Draws particle velocity gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawIntertialForces = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Inertial Forces",
			tooltip = "Draws particle inertial forces gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawConstraints = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Constraints",
			tooltip = "Draws constraint gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawNormals = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Normals",
			tooltip = "Draws normal gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawRestNormals = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Rest Normals",
			tooltip = "Draws rest normal gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawDeformedVertices = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Deformed Vertices",
			tooltip = "Draws deformed vertices gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawColliderAabb = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Collider AABB",
			tooltip = "Draws collider AABB gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawSimplexAabb = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Simplex AABB",
			tooltip = "Draws simplex AABB gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawColliderContacts = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Collider Contacts",
			tooltip = "Draws collider contacts gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawSimplexContacts = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Simplex Contacts",
			tooltip = "Draws simplex contacts gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawContactNormals = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Contact Normals",
			tooltip = "Draws collider contact normals gizmos."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DrawVisibleBodyAabbs = new DebugUI.Widget.NameAndTooltip
		{
			name = "Draw Visible Body AABBs",
			tooltip = "Draws visible body AABBs."
		};

		public static readonly DebugUI.Widget.NameAndTooltip UseOnlyGameCameraForCulling = new DebugUI.Widget.NameAndTooltip
		{
			name = "Use Only Game Camera For Culling",
			tooltip = "Use only the game camera for culling."
		};
	}

	private static class WidgetFactory
	{
		public static DebugUI.Widget CreateGizmosEnabledWidget(DebugDisplaySettingsXPBD settings)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.GizmosEnabled,
				getter = () => settings.GizmosEnabled,
				setter = delegate(bool value)
				{
					settings.GizmosEnabled = value;
				}
			};
		}

		internal static DebugUI.Widget CreateGizmosUseDepthBuffer(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.UseDepthBuffer,
				getter = () => data.UseDepthBuffer,
				setter = delegate(bool value)
				{
					data.UseDepthBuffer = value;
				}
			};
		}

		internal static DebugUI.Widget CreateShowStats(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.ShowStats,
				getter = () => data.ShowStats,
				setter = delegate(bool value)
				{
					data.ShowStats = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawParticles(DebugDisplaySettingsXPBD settings)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.DrawParticles,
				autoEnum = typeof(XPBDGizmosParticlesMode),
				getter = () => (int)settings.DrawParticles,
				setter = delegate
				{
				},
				getIndex = () => (int)settings.DrawParticles,
				setIndex = delegate(int value)
				{
					settings.DrawParticles = (XPBDGizmosParticlesMode)value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawVelocities(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawVelocities,
				getter = () => data.DrawVelocities,
				setter = delegate(bool value)
				{
					data.DrawVelocities = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawInertialForces(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawIntertialForces,
				getter = () => data.DrawInertialForces,
				setter = delegate(bool value)
				{
					data.DrawInertialForces = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawConstraints(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.DrawConstraints,
				autoEnum = typeof(DrawConstraintType),
				getter = () => (int)data.DrawConstraints,
				setter = delegate
				{
				},
				getIndex = () => (int)data.DrawConstraints,
				setIndex = delegate(int value)
				{
					data.DrawConstraints = (DrawConstraintType)value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawNormals(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawNormals,
				getter = () => data.DrawNormals,
				setter = delegate(bool value)
				{
					data.DrawNormals = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawRestNormals(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawRestNormals,
				getter = () => data.DrawRestNormals,
				setter = delegate(bool value)
				{
					data.DrawRestNormals = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawDeformedVertices(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawDeformedVertices,
				getter = () => data.DrawDeformedVertices,
				setter = delegate(bool value)
				{
					data.DrawDeformedVertices = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawColliderAabb(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawColliderAabb,
				getter = () => data.DrawColliderAabb,
				setter = delegate(bool value)
				{
					data.DrawColliderAabb = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawSimplexAabb(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawSimplexAabb,
				getter = () => data.DrawSimplexAabb,
				setter = delegate(bool value)
				{
					data.DrawSimplexAabb = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawColliderContacts(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawColliderContacts,
				getter = () => data.DrawColliderContacts,
				setter = delegate(bool value)
				{
					data.DrawColliderContacts = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawSimplexContacts(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawSimplexContacts,
				getter = () => data.DrawSimplexContacts,
				setter = delegate(bool value)
				{
					data.DrawSimplexContacts = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawColliderContactNormals(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawContactNormals,
				getter = () => data.DrawContactNormals,
				setter = delegate(bool value)
				{
					data.DrawContactNormals = value;
				}
			};
		}

		public static DebugUI.Widget CreateDrawVisibleBodyAabbs(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.DrawVisibleBodyAabbs,
				getter = () => data.DrawVisibleBodyAabbs,
				setter = delegate(bool value)
				{
					data.DrawVisibleBodyAabbs = value;
				}
			};
		}

		public static DebugUI.Widget CreateUseOnlyGameCameraForCulling(DebugDisplaySettingsXPBD data)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.UseOnlyGameCameraForCulling,
				getter = () => data.UseOnlyGameCameraForCulling,
				setter = delegate(bool value)
				{
					data.UseOnlyGameCameraForCulling = value;
				}
			};
		}
	}

	private class SettingsPanel : Owlcat.Runtime.Visual.Waaagh.Debugging.DebugDisplaySettingsPanel
	{
		public override string PanelName => "XPBD";

		public SettingsPanel(DebugDisplaySettingsXPBD data)
		{
			AddWidget(WidgetFactory.CreateGizmosEnabledWidget(data));
			AddWidget(WidgetFactory.CreateGizmosUseDepthBuffer(data));
			AddWidget(WidgetFactory.CreateShowStats(data));
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Particles",
				isHeader = true,
				opened = true,
				isHiddenCallback = () => !data.GizmosEnabled,
				children = 
				{
					WidgetFactory.CreateDrawParticles(data),
					WidgetFactory.CreateDrawVelocities(data),
					WidgetFactory.CreateDrawInertialForces(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Constraints",
				isHeader = true,
				opened = true,
				isHiddenCallback = () => !data.GizmosEnabled,
				children = { WidgetFactory.CreateDrawConstraints(data) }
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Meshes",
				isHeader = true,
				opened = true,
				isHiddenCallback = () => !data.GizmosEnabled,
				children = 
				{
					WidgetFactory.CreateDrawNormals(data),
					WidgetFactory.CreateDrawRestNormals(data),
					WidgetFactory.CreateDrawDeformedVertices(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Collisions",
				isHeader = true,
				opened = true,
				isHiddenCallback = () => !data.GizmosEnabled,
				children = 
				{
					WidgetFactory.CreateDrawColliderAabb(data),
					WidgetFactory.CreateDrawSimplexAabb(data),
					WidgetFactory.CreateDrawColliderContacts(data),
					WidgetFactory.CreateDrawColliderContactNormals(data),
					WidgetFactory.CreateDrawSimplexContacts(data)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Culling",
				isHeader = true,
				opened = true,
				isHiddenCallback = () => !data.GizmosEnabled,
				children = 
				{
					WidgetFactory.CreateDrawVisibleBodyAabbs(data),
					WidgetFactory.CreateUseOnlyGameCameraForCulling(data)
				}
			});
		}
	}

	private WaaaghDebugData m_DebugData;

	public bool AreAnySettingsActive
	{
		get
		{
			if (!GizmosEnabled && DrawParticles == XPBDGizmosParticlesMode.None && !DrawVelocities && !DrawInertialForces && DrawConstraints == DrawConstraintType.None && !DrawNormals && !DrawRestNormals && !DrawDeformedVertices && !DrawColliderAabb && !DrawSimplexAabb && !DrawColliderContacts && !DrawSimplexContacts && !DrawContactNormals && !DrawVisibleBodyAabbs)
			{
				return UseOnlyGameCameraForCulling;
			}
			return true;
		}
	}

	public bool GizmosEnabled
	{
		get
		{
			return m_DebugData.XPBDDebug.GizmosEnabled;
		}
		set
		{
			m_DebugData.XPBDDebug.GizmosEnabled = value;
		}
	}

	public bool UseDepthBuffer
	{
		get
		{
			return m_DebugData.XPBDDebug.UseDepthBuffer;
		}
		set
		{
			m_DebugData.XPBDDebug.UseDepthBuffer = value;
		}
	}

	public bool ShowStats
	{
		get
		{
			return m_DebugData.XPBDDebug.ShowStats;
		}
		set
		{
			m_DebugData.XPBDDebug.ShowStats = value;
		}
	}

	public XPBDGizmosParticlesMode DrawParticles
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawParticles;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawParticles = value;
		}
	}

	public bool DrawVelocities
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawVelocities;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawVelocities = value;
		}
	}

	public bool DrawInertialForces
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawInertialForces;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawInertialForces = value;
		}
	}

	public DrawConstraintType DrawConstraints
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawConstraints;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawConstraints = value;
		}
	}

	public bool DrawNormals
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawNormals;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawNormals = value;
		}
	}

	public bool DrawRestNormals
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawRestNormals;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawRestNormals = value;
		}
	}

	public bool DrawDeformedVertices
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawDeformedVertices;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawDeformedVertices = value;
		}
	}

	public bool DrawColliderAabb
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawColliderAabb;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawColliderAabb = value;
		}
	}

	public bool DrawSimplexAabb
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawSimplexAabb;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawSimplexAabb = value;
		}
	}

	public bool DrawColliderContacts
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawColliderContacts;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawColliderContacts = value;
		}
	}

	public bool DrawSimplexContacts
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawSimplexContacts;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawSimplexContacts = value;
		}
	}

	public bool DrawContactNormals
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawContactNormals;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawContactNormals = value;
		}
	}

	public bool DrawVisibleBodyAabbs
	{
		get
		{
			return m_DebugData.XPBDDebug.DrawVisibleBodyAabbs;
		}
		set
		{
			m_DebugData.XPBDDebug.DrawVisibleBodyAabbs = value;
		}
	}

	public bool UseOnlyGameCameraForCulling
	{
		get
		{
			return m_DebugData.XPBDDebug.UseOnlyGameCameraForCulling;
		}
		set
		{
			m_DebugData.XPBDDebug.UseOnlyGameCameraForCulling = value;
		}
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}

	public DebugDisplaySettingsXPBD(WaaaghDebugData waaaghDebugData)
	{
		m_DebugData = waaaghDebugData;
	}
}
