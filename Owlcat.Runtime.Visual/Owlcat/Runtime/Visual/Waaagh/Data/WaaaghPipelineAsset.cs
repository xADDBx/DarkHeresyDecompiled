using System;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.Lighting;
using Owlcat.Runtime.Visual.Terrain;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using Owlcat.Runtime.Visual.Waaagh.Shadows;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Data;

public class WaaaghPipelineAsset : RenderPipelineAsset<WaaaghPipeline>, IProbeVolumeEnabledRenderPipeline
{
	internal const int k_ShadowCascadeMinCount = 1;

	internal const int k_ShadowCascadeMaxCount = 4;

	public const int kUnhandledExceptionLockThreshold = 3;

	[NonSerialized]
	private int m_UnhandledExceptionCounter;

	private PipelineRuntimeResources m_RuntimeResources;

	private Shader m_DefaultShader;

	private IPipelineRenderer[] m_Renderers = new IPipelineRenderer[1];

	[Header("Renderer")]
	[SerializeField]
	internal ScriptableRendererData[] m_RendererDataList = new ScriptableRendererData[1];

	[SerializeField]
	internal int m_DefaultRendererIndex;

	[Header("General")]
	[SerializeField]
	private bool m_SupportsDepthTexture;

	[SerializeField]
	private bool m_SupportsOpaqueTexture;

	[SerializeField]
	private bool m_SupportsTerrainHoles = true;

	[Header("Quality")]
	[SerializeField]
	private bool m_SupportsHDR = true;

	[SerializeField]
	private HDRColorBufferPrecision m_HdrColorBufferPrecision = HDRColorBufferPrecision._64Bits;

	[SerializeField]
	[Range(0.1f, 1f)]
	private float m_RenderScale = 1f;

	[SerializeField]
	private UpscalingFilterSelection m_UpscalingFilter;

	[SerializeField]
	private bool m_FsrOverrideSharpness;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_FsrSharpness = 0.92f;

	[SerializeField]
	private DitheringSettings m_DitheringSettings = new DitheringSettings();

	[Header("Shadows")]
	[SerializeField]
	private ShadowSettings m_ShadowSettings = new ShadowSettings();

	[Header("Post Process")]
	[SerializeField]
	private PostProcessSettings m_PostProcessSettings = new PostProcessSettings();

	[Header("Lighting")]
	[SerializeField]
	private LightLayerEnum m_DefaultLightLayerMask = LightLayerEnum.LightLayerDefault | LightLayerEnum.LightLayer1;

	[SerializeField]
	private LightProbeSystem m_LightProbeSystem;

	[SerializeField]
	private ProbeVolumesSettings m_ProbeVolumesSettings;

	[Header("Light Cookies")]
	[SerializeField]
	private LightCookieSettings m_LightCookieSettings = new LightCookieSettings();

	[Header("Local Volumetric Fog")]
	[SerializeField]
	private LocalVolumetricFogSettings m_LocalVolumetricFogSettings = new LocalVolumetricFogSettings();

	[Header("Terrain")]
	[SerializeField]
	private TerrainSettings m_TerrainSettings = new TerrainSettings();

	[Header("Advanced")]
	[SerializeField]
	private bool m_UseSRPBatcher = true;

	[SerializeField]
	private bool m_SupportsDynamicBatching;

	[SerializeField]
	private bool m_SupportsLightLayers;

	[SerializeField]
	private VolumeFrameworkUpdateMode m_VolumeFrameworkUpdateMode;

	[SerializeField]
	private GPUDrivenBRGSettings m_GPUDrivenBRGSettings = new GPUDrivenBRGSettings();

	[SerializeField]
	private VirtualTextureSettings m_VirtualTextureSettings = new VirtualTextureSettings();

	[Header("Debug")]
	[SerializeField]
	private WaaaghDebugData m_DebugData;

	public override string renderPipelineShaderTag => "OwlcatPipeline";

	public bool SupportsCameraDepthTexture
	{
		get
		{
			return m_SupportsDepthTexture;
		}
		set
		{
			m_SupportsDepthTexture = value;
		}
	}

	public bool SupportsCameraOpaqueTexture
	{
		get
		{
			return m_SupportsOpaqueTexture;
		}
		set
		{
			m_SupportsOpaqueTexture = value;
		}
	}

	public bool SupportsTerrainHoles => m_SupportsTerrainHoles;

	public bool SupportsHDR
	{
		get
		{
			return m_SupportsHDR;
		}
		set
		{
			m_SupportsHDR = value;
		}
	}

	public HDRColorBufferPrecision HDRColorBufferPrecision
	{
		get
		{
			return m_HdrColorBufferPrecision;
		}
		set
		{
			m_HdrColorBufferPrecision = value;
		}
	}

	public float RenderScale
	{
		get
		{
			return m_RenderScale;
		}
		set
		{
			m_RenderScale = ValidateRenderScale(value);
		}
	}

	public UpscalingFilterSelection UpscalingFilter
	{
		get
		{
			return m_UpscalingFilter;
		}
		set
		{
			m_UpscalingFilter = value;
		}
	}

	public bool FsrOverrideSharpness
	{
		get
		{
			return m_FsrOverrideSharpness;
		}
		set
		{
			m_FsrOverrideSharpness = value;
		}
	}

	public float FsrSharpness
	{
		get
		{
			return m_FsrSharpness;
		}
		set
		{
			m_FsrSharpness = value;
		}
	}

	public DitheringSettings DitheringSettings => m_DitheringSettings;

	public ShadowSettings ShadowSettings => m_ShadowSettings;

	public PostProcessSettings PostProcessSettings => m_PostProcessSettings;

	public LightLayerEnum DefaultLightLayerMask => m_DefaultLightLayerMask;

	public LightProbeSystem LightProbeSystem
	{
		get
		{
			return m_LightProbeSystem;
		}
		set
		{
			m_LightProbeSystem = value;
		}
	}

	public bool SupportProbeVolumes => LightProbeSystem == LightProbeSystem.ProbeVolumes;

	public ProbeVolumesSettings ProbeVolumesSettings
	{
		get
		{
			return m_ProbeVolumesSettings;
		}
		set
		{
			m_ProbeVolumesSettings = value;
		}
	}

	public LightCookieSettings LightCookieSettings => m_LightCookieSettings;

	public LocalVolumetricFogSettings LocalVolumetricFogSettings => m_LocalVolumetricFogSettings;

	public TerrainSettings TerrainSettings => m_TerrainSettings;

	public bool UseSRPBatcher => m_UseSRPBatcher;

	public bool SupportsDynamicBatching => m_SupportsDynamicBatching;

	public VolumeFrameworkUpdateMode VolumeFrameworkUpdateMode => m_VolumeFrameworkUpdateMode;

	public GPUDrivenBRGSettings GPUDrivenBRGSettings => m_GPUDrivenBRGSettings;

	public VirtualTextureSettings VirtualTextureSettings => m_VirtualTextureSettings;

	public ScriptableRendererData[] RendererDataList => m_RendererDataList;

	public bool SupportsLightLayers => m_SupportsLightLayers;

	public WaaaghDebugData DebugData => m_DebugData;

	public PipelineRuntimeResources RuntimeResources
	{
		get
		{
			if (m_RuntimeResources == null || m_RuntimeResources.DefaultUIMaterial == null)
			{
				m_RuntimeResources = GraphicsSettings.GetRenderPipelineSettings<PipelineRuntimeResources>();
			}
			return m_RuntimeResources;
		}
	}

	public IPipelineRenderer ScriptableRenderer
	{
		get
		{
			if (m_RendererDataList?.Length > m_DefaultRendererIndex && m_RendererDataList[m_DefaultRendererIndex] == null)
			{
				Debug.LogError("Default renderer is missing from the current Pipeline Asset.", this);
				return null;
			}
			if (ScriptableRendererData.IsInvalidated || m_Renderers[m_DefaultRendererIndex] == null)
			{
				DestroyRenderer(ref m_Renderers[m_DefaultRendererIndex]);
				m_Renderers[m_DefaultRendererIndex] = ScriptableRendererData.InternalCreateRenderer();
			}
			return m_Renderers[m_DefaultRendererIndex];
		}
	}

	internal ScriptableRendererData ScriptableRendererData
	{
		get
		{
			if (m_RendererDataList[m_DefaultRendererIndex] == null)
			{
				CreatePipeline();
			}
			return m_RendererDataList[m_DefaultRendererIndex];
		}
	}

	public int[] RendererIndexList
	{
		get
		{
			int[] array = new int[m_RendererDataList.Length + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = i - 1;
			}
			return array;
		}
	}

	public override Shader defaultShader
	{
		get
		{
			if (m_DefaultShader == null)
			{
				m_DefaultShader = Shader.Find("Owlcat/Lit");
			}
			return m_DefaultShader;
		}
	}

	public override Material defaultMaterial => GetDefaultMaterial(DefaultMaterialType.Standard);

	public override Material defaultParticleMaterial => GetDefaultMaterial(DefaultMaterialType.Particle);

	public override Material defaultTerrainMaterial => GetDefaultMaterial(DefaultMaterialType.Terrain);

	public override Material defaultUIMaterial => GetDefaultMaterial(DefaultMaterialType.UI);

	public bool supportProbeVolume => LightProbeSystem == LightProbeSystem.ProbeVolumes;

	public ProbeVolumeSHBands maxSHBands
	{
		get
		{
			if (LightProbeSystem != LightProbeSystem.ProbeVolumes)
			{
				return ProbeVolumeSHBands.SphericalHarmonicsL1;
			}
			return ProbeVolumesSettings.SHBands;
		}
	}

	[Obsolete("This property is no longer necessary.")]
	public ProbeVolumeSceneData probeVolumeSceneData => null;

	public bool UnhandledExceptionLockActive()
	{
		return m_UnhandledExceptionCounter >= 3;
	}

	public void ResetUnhandledExceptionCounter()
	{
		m_UnhandledExceptionCounter = 0;
	}

	public void IncrementUnhandledExceptionCounter()
	{
		m_UnhandledExceptionCounter++;
	}

	public IPipelineRenderer GetRenderer(int index)
	{
		if (index == -1)
		{
			index = m_DefaultRendererIndex;
		}
		if (index >= m_RendererDataList.Length || index < 0 || m_RendererDataList[index] == null)
		{
			Debug.LogWarning("Renderer at index " + index + " is missing, falling back to Default Renderer " + m_RendererDataList[m_DefaultRendererIndex].name, this);
			index = m_DefaultRendererIndex;
		}
		if (m_Renderers == null || m_Renderers.Length < m_RendererDataList.Length)
		{
			CreateRenderers();
		}
		if (m_RendererDataList[index].IsInvalidated || m_Renderers[index] == null)
		{
			DestroyRenderer(ref m_Renderers[index]);
			m_Renderers[index] = m_RendererDataList[index].InternalCreateRenderer();
		}
		return m_Renderers[index];
	}

	protected override UnityEngine.Rendering.RenderPipeline CreatePipeline()
	{
		m_RuntimeResources = GraphicsSettings.GetRenderPipelineSettings<PipelineRuntimeResources>();
		if (m_RendererDataList == null)
		{
			m_RendererDataList = new ScriptableRendererData[1];
		}
		if (m_RendererDataList[m_DefaultRendererIndex] == null)
		{
			Debug.LogError("Default Renderer is missing, make sure there is a Renderer assigned as the default on the current Waaagh RP asset:" + WaaaghPipeline.Asset.name, this);
			return null;
		}
		CreateRenderers();
		ScriptableRendererData[] rendererDataList = m_RendererDataList;
		for (int i = 0; i < rendererDataList.Length; i++)
		{
			if (rendererDataList[i] is WaaaghRendererData waaaghRendererData)
			{
				Blitter.Cleanup();
				Blitter.Initialize(waaaghRendererData.Shaders.CoreBlitPS, waaaghRendererData.Shaders.CoreBlitColorAndDepthPS);
				break;
			}
		}
		return new WaaaghPipeline(this);
	}

	protected override void EnsureGlobalSettings()
	{
		base.EnsureGlobalSettings();
	}

	private void CreateRenderers()
	{
		DestroyRenderers();
		if (m_Renderers == null || m_Renderers.Length != m_RendererDataList.Length)
		{
			m_Renderers = new IPipelineRenderer[m_RendererDataList.Length];
		}
		for (int i = 0; i < m_RendererDataList.Length; i++)
		{
			if (m_RendererDataList[i] != null)
			{
				m_Renderers[i] = m_RendererDataList[i].InternalCreateRenderer();
			}
		}
	}

	private void DestroyRenderers()
	{
		if (m_Renderers != null)
		{
			for (int i = 0; i < m_Renderers.Length; i++)
			{
				DestroyRenderer(ref m_Renderers[i]);
			}
		}
	}

	private void DestroyRenderer(ref IPipelineRenderer renderer)
	{
		if (renderer != null)
		{
			renderer.Dispose();
			renderer = null;
		}
	}

	public bool ValidateRendererData(int index)
	{
		if (index == -1)
		{
			index = m_DefaultRendererIndex;
		}
		if (index >= m_RendererDataList.Length)
		{
			return false;
		}
		return m_RendererDataList[index] != null;
	}

	public void Invalidate()
	{
		GPUDrivenBRGSettings.ResetSupport();
		OnValidate();
	}

	protected override void OnValidate()
	{
		DestroyRenderers();
		base.OnValidate();
	}

	private void OnEnable()
	{
		ReloadAllNullProperties();
	}

	private void ReloadAllNullProperties()
	{
	}

	protected override void OnDisable()
	{
		DestroyRenderers();
		base.OnDisable();
	}

	private float ValidateRenderScale(float value)
	{
		return Mathf.Max(WaaaghPipeline.MinRenderScale, Mathf.Min(value, WaaaghPipeline.MaxRenderScale));
	}

	private float ValidateShadowBias(float value)
	{
		return Mathf.Max(0f, Mathf.Min(value, WaaaghPipeline.MaxShadowBias));
	}

	public bool ValidateRendererDataList(bool partial = false)
	{
		int num = 0;
		for (int i = 0; i < m_RendererDataList.Length; i++)
		{
			num += ((!ValidateRendererData(i)) ? 1 : 0);
		}
		if (partial)
		{
			return num == 0;
		}
		return num != m_RendererDataList.Length;
	}

	private Material GetDefaultMaterial(DefaultMaterialType materialType)
	{
		if (materialType == DefaultMaterialType.UI)
		{
			return RuntimeResources.DefaultUIMaterial;
		}
		return null;
	}

	public void GetDefaultPreviewReflectionProbe(out Texture cubemap, out Vector4 hdrDecodeValues)
	{
		cubemap = null;
		hdrDecodeValues = Vector4.zero;
	}
}
