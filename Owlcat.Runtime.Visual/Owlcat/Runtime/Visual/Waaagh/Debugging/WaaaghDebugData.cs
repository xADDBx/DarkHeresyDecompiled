using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Debugging.DisplayStats;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

public class WaaaghDebugData : ScriptableObject, IDebugData
{
	[SerializeField]
	private LightingDebug m_LightingDebug = new LightingDebug();

	[SerializeField]
	private RenderingDebug m_RenderingDebug = new RenderingDebug();

	[SerializeField]
	private StencilDebug m_StencilDebug = new StencilDebug();

	[SerializeField]
	private ShadowsDebug m_ShadowsDebug = new ShadowsDebug();

	[SerializeField]
	private VirtualTextureDebug m_VirtualTextureDebug = new VirtualTextureDebug();

	[SerializeField]
	private GPUDrivenBRGDebug m_GPUDrivenBRGDebug = new GPUDrivenBRGDebug();

	[SerializeField]
	private TerrainDebug m_TerrainDebug = new TerrainDebug();

	[SerializeField]
	private XPBDDebug m_XPBDDebug = new XPBDDebug();

	private ExtendedDebugDisplaySettingsStats m_DebugStats;

	private DebugDisplaySettingsGPUDrivenBRG m_DebugGPUDrivenBRG;

	private DebugDisplaySettingsLighting m_DebugLighting;

	private DebugDisplaySettingsRendering m_DebugRendering;

	private DebugDisplaySettingsShadows m_DebugShadows;

	private DebugDisplaySettingsVirtualTexture m_DebugVirtualTexture;

	private DebugDisplaySettingsTerrain m_DebugDisplayTerrain;

	private DebugDisplaySettingsXPBD m_DebugDisplayXPBD;

	private DebugDisplaySettingsVolume m_DebugDisplayVolume;

	private IEnumerable<IDebugDisplaySettingsPanelDisposable> m_DisposablePanels;

	public DebugResources Resources { get; private set; }

	public LightingDebug LightingDebug => m_LightingDebug;

	public RenderingDebug RenderingDebug => m_RenderingDebug;

	public StencilDebug StencilDebug => m_StencilDebug;

	public ShadowsDebug ShadowsDebug => m_ShadowsDebug;

	public VirtualTextureDebug VirtualTextureDebug => m_VirtualTextureDebug;

	public GPUDrivenBRGDebug GPUDrivenBRGDebug => m_GPUDrivenBRGDebug;

	internal TerrainDebug TerrainDebug => m_TerrainDebug;

	public XPBDDebug XPBDDebug => m_XPBDDebug;

	public WaaaghPipeline Pipeline { get; private set; }

	private void Reset()
	{
		m_LightingDebug = new LightingDebug();
		m_RenderingDebug = new RenderingDebug();
		m_StencilDebug = new StencilDebug();
		m_ShadowsDebug = new ShadowsDebug();
		m_VirtualTextureDebug = new VirtualTextureDebug();
		m_TerrainDebug = new TerrainDebug();
		m_XPBDDebug = new XPBDDebug();
	}

	private void OnEnable()
	{
		Resources = GraphicsSettings.GetRenderPipelineSettings<DebugResources>();
	}

	public Action GetReset()
	{
		return Reset;
	}

	public void RegisterDebug(WaaaghPipeline pipeline)
	{
		Pipeline = pipeline;
		Init();
		DebugManager instance = DebugManager.instance;
		instance.RegisterData(this);
		List<IDebugDisplaySettingsPanelDisposable> list = new List<IDebugDisplaySettingsPanelDisposable>();
		CreateAndRegisterPanel(m_DebugStats, instance, list);
		CreateAndRegisterPanel(m_DebugRendering, instance, list, RegisterRenderGraph);
		CreateAndRegisterPanel(m_DebugLighting, instance, list);
		CreateAndRegisterPanel(m_DebugShadows, instance, list);
		CreateAndRegisterPanel(m_DebugVirtualTexture, instance, list);
		CreateAndRegisterPanel(m_DebugGPUDrivenBRG, instance, list);
		CreateAndRegisterPanel(m_DebugDisplayTerrain, instance, list);
		CreateAndRegisterPanel(m_DebugDisplayXPBD, instance, list);
		CreateAndRegisterPanel(m_DebugDisplayVolume, instance, list);
		m_DisposablePanels = list;
		static void CreateAndRegisterPanel(IDebugDisplaySettingsData debugDisplay, DebugManager debugManager, IList<IDebugDisplaySettingsPanelDisposable> panels, Action<DebugUI.Panel> withPanel = null)
		{
			IDebugDisplaySettingsPanelDisposable debugDisplaySettingsPanelDisposable = debugDisplay.CreatePanel();
			DebugUI.Widget[] widgets = debugDisplaySettingsPanelDisposable.Widgets;
			DebugUI.Panel panel = debugManager.GetPanel(debugDisplaySettingsPanelDisposable.PanelName, createIfNull: true);
			panel.children.Add(widgets);
			withPanel?.Invoke(panel);
			panels.Add(debugDisplaySettingsPanelDisposable);
		}
	}

	private void Init()
	{
		if (m_DebugStats == null)
		{
			m_DebugStats = new ExtendedDebugDisplaySettingsStats(new WaaaghDebugDisplayStats());
		}
		if (m_DebugRendering == null)
		{
			m_DebugRendering = new DebugDisplaySettingsRendering(this);
		}
		if (m_DebugLighting == null)
		{
			m_DebugLighting = new DebugDisplaySettingsLighting(this);
		}
		if (m_DebugShadows == null)
		{
			m_DebugShadows = new DebugDisplaySettingsShadows(this);
		}
		if (m_DebugVirtualTexture == null)
		{
			m_DebugVirtualTexture = new DebugDisplaySettingsVirtualTexture(this);
		}
		if (m_DebugGPUDrivenBRG == null)
		{
			m_DebugGPUDrivenBRG = new DebugDisplaySettingsGPUDrivenBRG(this);
		}
		if (m_DebugDisplayTerrain == null)
		{
			m_DebugDisplayTerrain = new DebugDisplaySettingsTerrain(this);
		}
		if (m_DebugDisplayXPBD == null)
		{
			m_DebugDisplayXPBD = new DebugDisplaySettingsXPBD(this);
		}
		if (m_DebugDisplayVolume == null)
		{
			m_DebugDisplayVolume = new DebugDisplaySettingsVolume(new WaaaghVolumeDebugSettings());
		}
	}

	private static void RegisterRenderGraph(DebugUI.Panel panel)
	{
		if (!(panel.displayName == "Rendering"))
		{
			return;
		}
		foreach (RenderGraph registeredRenderGraph in RenderGraph.GetRegisteredRenderGraphs())
		{
			registeredRenderGraph.RegisterDebug(panel);
		}
	}

	public void UnregisterDebug()
	{
		DebugManager instance = DebugManager.instance;
		foreach (IDebugDisplaySettingsPanelDisposable disposablePanel in m_DisposablePanels)
		{
			DebugUI.Widget[] widgets = disposablePanel.Widgets;
			string panelName = disposablePanel.PanelName;
			DebugUI.Panel panel = instance.GetPanel(panelName, createIfNull: true);
			UnregisterRenderGraph(panel);
			ObservableList<DebugUI.Widget> children = panel.children;
			disposablePanel.Dispose();
			children.Remove(widgets);
		}
		m_DisposablePanels = null;
		instance.UnregisterData(this);
	}

	private static void UnregisterRenderGraph(DebugUI.Panel panel)
	{
		if (!(panel.displayName == "Rendering"))
		{
			return;
		}
		foreach (RenderGraph registeredRenderGraph in RenderGraph.GetRegisteredRenderGraphs())
		{
			registeredRenderGraph.UnRegisterDebug();
			for (int num = panel.children.Count - 1; num >= 0; num--)
			{
				if (panel.children[num] is DebugUI.Foldout foldout && foldout.displayName == registeredRenderGraph.name)
				{
					panel.children.RemoveAt(num);
				}
			}
		}
	}

	internal void UpdateDisplayStats()
	{
		m_DebugStats?.debugDisplayStats.Update();
	}

	internal bool DebugNeedsDebugDisplayKeyword()
	{
		if (m_DebugLighting.DebugLightingMode == DebugLightingMode.None && m_DebugRendering.OverdrawMode == DebugOverdrawMode.None && !m_DebugRendering.DebugMipMap && m_DebugRendering.DebugMaterialMode == DebugMaterialMode.None && m_DebugVirtualTexture.DebugTilesMode == DebugTilesMode.None)
		{
			return m_DebugDisplayTerrain.AreAnySettingsActive;
		}
		return true;
	}
}
