using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
public class WaaaghPipelineGlobalSettings : RenderPipelineGlobalSettings
{
	[SerializeReference]
	private RenderPipelineGraphicsSettingsContainer m_Settings = new RenderPipelineGraphicsSettingsContainer();

	[SerializeField]
	private int k_AssetVersion = 1;

	[SerializeField]
	private int k_AssetPreviousVersion = 1;

	private static WaaaghPipelineGlobalSettings s_CachedInstance = null;

	public static readonly string DefaultAssetName = "WaaaghPipelineGlobalSettings";

	public bool SupportRuntimeDebugDisplay;

	protected override List<IRenderPipelineGraphicsSettings> settingsList => m_Settings.settingsList;

	public static WaaaghPipelineGlobalSettings Instance
	{
		get
		{
			if (s_CachedInstance == null)
			{
				s_CachedInstance = GraphicsSettings.GetSettingsForRenderPipeline<WaaaghPipeline>() as WaaaghPipelineGlobalSettings;
			}
			return s_CachedInstance;
		}
	}

	public void GetAllSettings(List<IRenderPipelineGraphicsSettings> settings)
	{
		settings.AddRange(settingsList);
	}
}
