using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.History;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[ImageEffectAllowedInSceneView]
public class WaaaghAdditionalCameraData : MonoBehaviour, IAdditionalData
{
	private const string k_GizmoPath = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/";

	private const string k_BaseCameraGizmoPath = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/Camera_Base.png";

	private const string k_OverlayCameraGizmoPath = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/Camera_Base.png";

	private const string k_PostProcessingGizmoPath = "Packages/com.unity.render-pipelines.universal/Editor/Gizmos/Camera_PostProcessing.png";

	[SerializeField]
	private bool m_IsLightingEnabled = true;

	[SerializeField]
	private bool m_RenderShadows = true;

	[SerializeField]
	private CameraOverrideOption m_RequiresDepthTextureOption = CameraOverrideOption.UsePipelineSettings;

	[SerializeField]
	private CameraOverrideOption m_RequiresOpaqueTextureOption = CameraOverrideOption.UsePipelineSettings;

	[SerializeField]
	private CameraRenderType m_CameraType;

	[SerializeField]
	private List<Camera> m_Cameras = new List<Camera>();

	[SerializeField]
	private int m_RendererIndex = -1;

	[SerializeField]
	private LayerMask m_VolumeLayerMask = 1;

	[SerializeField]
	private Transform m_VolumeTrigger;

	[SerializeField]
	private VolumeFrameworkUpdateMode m_VolumeFrameworkUpdateModeOption = VolumeFrameworkUpdateMode.UsePipelineSettings;

	[SerializeField]
	private bool m_RenderPostProcessing;

	[SerializeField]
	private AntialiasingMode m_Antialiasing;

	[SerializeField]
	private AntialiasingQuality m_AntialiasingQuality = AntialiasingQuality.High;

	[SerializeField]
	private bool m_StopNaN;

	[SerializeField]
	private bool m_Dithering;

	[SerializeField]
	private bool m_ClearDepth = true;

	[SerializeField]
	private bool m_AllowIndirectRendering = true;

	[SerializeField]
	private bool m_AllowRenderScaling;

	[SerializeField]
	private bool m_AllowHDROutput = true;

	[SerializeField]
	internal TemporalAA.Settings m_TaaSettings = TemporalAA.Settings.Create();

	[NonSerialized]
	private MotionVectorsPersistentData m_MotionVectorsPersistentData = new MotionVectorsPersistentData();

	private VolumeStack m_VolumeStack;

	[NonSerialized]
	private Camera m_Camera;

	private static WaaaghAdditionalCameraData s_DefaultAdditionalCameraData;

	private static List<VolumeStack> s_CachedVolumeStacks;

	internal static WaaaghAdditionalCameraData DefaultAdditionalCameraData
	{
		get
		{
			if (s_DefaultAdditionalCameraData == null)
			{
				s_DefaultAdditionalCameraData = new WaaaghAdditionalCameraData();
			}
			return s_DefaultAdditionalCameraData;
		}
	}

	internal Camera camera
	{
		get
		{
			if (!m_Camera)
			{
				base.gameObject.TryGetComponent<Camera>(out m_Camera);
			}
			return m_Camera;
		}
	}

	public bool IsLightingEnabled
	{
		get
		{
			return m_IsLightingEnabled;
		}
		set
		{
			m_IsLightingEnabled = value;
		}
	}

	public bool RenderShadows
	{
		get
		{
			return m_RenderShadows;
		}
		set
		{
			m_RenderShadows = value;
		}
	}

	public CameraOverrideOption RequiresDepthOption
	{
		get
		{
			return m_RequiresDepthTextureOption;
		}
		set
		{
			m_RequiresDepthTextureOption = value;
		}
	}

	public CameraOverrideOption RequiresColorOption
	{
		get
		{
			return m_RequiresOpaqueTextureOption;
		}
		set
		{
			m_RequiresOpaqueTextureOption = value;
		}
	}

	public CameraRenderType RenderType
	{
		get
		{
			return m_CameraType;
		}
		set
		{
			m_CameraType = value;
		}
	}

	public List<Camera> CameraStack
	{
		get
		{
			if (RenderType != 0)
			{
				Camera component = base.gameObject.GetComponent<Camera>();
				Debug.LogWarning($"{component.name}: This camera is of {RenderType} type. Only Base cameras can have a camera stack.");
				return null;
			}
			return m_Cameras;
		}
	}

	public bool ClearDepth => m_ClearDepth;

	public bool AllowIndirectRendering
	{
		get
		{
			return m_AllowIndirectRendering;
		}
		set
		{
			m_AllowIndirectRendering = value;
		}
	}

	public bool AllowRenderScaling
	{
		get
		{
			return m_AllowRenderScaling;
		}
		set
		{
			m_AllowRenderScaling = value;
		}
	}

	public bool RequiresDepthTexture
	{
		get
		{
			if (m_RequiresDepthTextureOption == CameraOverrideOption.UsePipelineSettings)
			{
				return WaaaghPipeline.Asset.SupportsCameraDepthTexture;
			}
			return m_RequiresDepthTextureOption == CameraOverrideOption.On;
		}
		set
		{
			m_RequiresDepthTextureOption = (value ? CameraOverrideOption.On : CameraOverrideOption.Off);
		}
	}

	public bool RequiresColorTexture
	{
		get
		{
			if (m_RequiresOpaqueTextureOption == CameraOverrideOption.UsePipelineSettings)
			{
				return WaaaghPipeline.Asset.SupportsCameraOpaqueTexture;
			}
			return m_RequiresOpaqueTextureOption == CameraOverrideOption.On;
		}
		set
		{
			m_RequiresOpaqueTextureOption = (value ? CameraOverrideOption.On : CameraOverrideOption.Off);
		}
	}

	public IPipelineRenderer ScriptableRenderer
	{
		get
		{
			if ((object)WaaaghPipeline.Asset == null)
			{
				return null;
			}
			if (!WaaaghPipeline.Asset.ValidateRendererData(m_RendererIndex))
			{
				int defaultRendererIndex = WaaaghPipeline.Asset.m_DefaultRendererIndex;
				Debug.LogWarning("Renderer at <b>index " + m_RendererIndex + "</b> is missing for camera <b>" + camera.name + "</b>, falling back to Default Renderer. <b>" + WaaaghPipeline.Asset.m_RendererDataList[defaultRendererIndex].name + "</b>", WaaaghPipeline.Asset);
				return WaaaghPipeline.Asset.GetRenderer(defaultRendererIndex);
			}
			return WaaaghPipeline.Asset.GetRenderer(m_RendererIndex);
		}
	}

	public LayerMask VolumeLayerMask
	{
		get
		{
			return m_VolumeLayerMask;
		}
		set
		{
			m_VolumeLayerMask = value;
		}
	}

	public Transform VolumeTrigger
	{
		get
		{
			return m_VolumeTrigger;
		}
		set
		{
			m_VolumeTrigger = value;
		}
	}

	internal VolumeFrameworkUpdateMode VolumeFrameworkUpdateMode
	{
		get
		{
			return m_VolumeFrameworkUpdateModeOption;
		}
		set
		{
			m_VolumeFrameworkUpdateModeOption = value;
		}
	}

	public bool RequiresVolumeFrameworkUpdate
	{
		get
		{
			if (m_VolumeFrameworkUpdateModeOption == VolumeFrameworkUpdateMode.UsePipelineSettings)
			{
				return WaaaghPipeline.Asset.VolumeFrameworkUpdateMode != VolumeFrameworkUpdateMode.ViaScripting;
			}
			return m_VolumeFrameworkUpdateModeOption == VolumeFrameworkUpdateMode.EveryFrame;
		}
	}

	public VolumeStack VolumeStack
	{
		get
		{
			return m_VolumeStack;
		}
		set
		{
			if (value == null && m_VolumeStack != null && m_VolumeStack.isValid)
			{
				if (s_CachedVolumeStacks == null)
				{
					s_CachedVolumeStacks = new List<VolumeStack>(4);
				}
				s_CachedVolumeStacks.Add(m_VolumeStack);
			}
			m_VolumeStack = value;
		}
	}

	public bool RenderPostProcessing
	{
		get
		{
			return m_RenderPostProcessing;
		}
		set
		{
			m_RenderPostProcessing = value;
		}
	}

	public AntialiasingMode Antialiasing
	{
		get
		{
			return m_Antialiasing;
		}
		set
		{
			m_Antialiasing = value;
		}
	}

	public AntialiasingQuality AntialiasingQuality
	{
		get
		{
			return m_AntialiasingQuality;
		}
		set
		{
			m_AntialiasingQuality = value;
		}
	}

	public bool StopNaN
	{
		get
		{
			return m_StopNaN;
		}
		set
		{
			m_StopNaN = value;
		}
	}

	public bool Dithering
	{
		get
		{
			return m_Dithering;
		}
		set
		{
			m_Dithering = value;
		}
	}

	public ICameraHistoryReadAccess History => WaaaghCameraHistoryManager.EnsureCamera(camera);

	internal WaaaghCameraHistory HistoryManager => WaaaghCameraHistoryManager.EnsureCamera(camera);

	public bool AllowHDROutput
	{
		get
		{
			return m_AllowHDROutput;
		}
		set
		{
			m_AllowHDROutput = value;
		}
	}

	public ref TemporalAA.Settings TaaSettings => ref m_TaaSettings;

	internal MotionVectorsPersistentData MotionVectorsPersistentData => m_MotionVectorsPersistentData;

	public void UpdateCameraStack()
	{
		int num = m_Cameras.RemoveAll((Camera cam) => cam == null);
		if (num != 0)
		{
			Debug.LogWarning(base.name + ": " + num + " camera overlay" + ((num > 1) ? "s" : "") + " no longer exists and will be removed from the camera stack.");
		}
	}

	internal void GetOrCreateVolumeStack()
	{
		if (s_CachedVolumeStacks != null && s_CachedVolumeStacks.Count > 0)
		{
			int index = s_CachedVolumeStacks.Count - 1;
			VolumeStack volumeStack = s_CachedVolumeStacks[index];
			s_CachedVolumeStacks.RemoveAt(index);
			if (volumeStack.isValid)
			{
				VolumeStack = volumeStack;
			}
		}
		if (VolumeStack == null)
		{
			VolumeStack = VolumeManager.instance.CreateStack();
		}
	}

	private void OnDestroy()
	{
		m_Camera.DestroyVolumeStack(this);
		WaaaghCameraHistoryManager.OnAdditionalCameraDataDestroy(this);
	}
}
