using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.HBAO.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.HBAO;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/HBAO", fileName = "HBAOFeature")]
public class HBAOFeature : ScriptableRendererFeature
{
	internal static class MersenneTwister
	{
		public static float[] Numbers = new float[32]
		{
			0.556725f, 0.00552f, 0.708315f, 0.583199f, 0.236644f, 0.99238f, 0.981091f, 0.119804f, 0.510866f, 0.560499f,
			0.961497f, 0.557862f, 0.539955f, 0.332871f, 0.417807f, 0.920779f, 0.730747f, 0.07669f, 0.008562f, 0.660104f,
			0.428921f, 0.511342f, 0.587871f, 0.906406f, 0.43798f, 0.620309f, 0.062196f, 0.119485f, 0.235646f, 0.795892f,
			0.044437f, 0.617311f
		};
	}

	private HBAOFeatureResources m_HbaoResources;

	private Material m_HbaoMaterial;

	private HBAOPass m_HbaoPass;

	private Texture2D m_NoiseTexture;

	private HBAO.NoiseType? m_PreviousNoiseType;

	private Mesh m_FullscreenTriangle;

	private List<HBAOHistoryBuffer> m_CameraHistoryBuffers = new List<HBAOHistoryBuffer>();

	private bool m_PreviousColorBleedingEnabled;

	private HBAO.Resolution? m_PreviousResolution;

	private static readonly Vector2[] s_jitter = new Vector2[16];

	private static readonly float[] s_temporalRotations = new float[6] { 60f, 300f, 180f, 240f, 120f, 0f };

	private static readonly float[] s_temporalOffsets = new float[4] { 0f, 0.5f, 0.25f, 0.75f };

	internal Mesh fullscreenTriangle
	{
		get
		{
			if (m_FullscreenTriangle != null)
			{
				return m_FullscreenTriangle;
			}
			m_FullscreenTriangle = new Mesh
			{
				name = "Fullscreen Triangle"
			};
			m_FullscreenTriangle.SetVertices(new List<Vector3>
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 3f, 0f),
				new Vector3(3f, -1f, 0f)
			});
			m_FullscreenTriangle.SetIndices(new int[3] { 0, 1, 2 }, MeshTopology.Triangles, 0, calculateBounds: false);
			m_FullscreenTriangle.UploadMeshData(markNoLongerReadable: false);
			return m_FullscreenTriangle;
		}
	}

	internal Texture2D NoiseTexture => m_NoiseTexture;

	public override void Create()
	{
		m_HbaoResources = GraphicsSettings.GetRenderPipelineSettings<HBAOFeatureResources>();
		if (m_HbaoResources != null && m_HbaoResources.HbaoPS != null)
		{
			m_HbaoMaterial = CoreUtils.CreateEngineMaterial(m_HbaoResources.HbaoPS);
		}
		if (m_HbaoPass == null)
		{
			m_HbaoPass = new HBAOPass((RenderPassEvent)219, m_HbaoMaterial);
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		HBAO component = VolumeManager.instance.stack.GetComponent<HBAO>();
		CheckNoiseTexture(component);
		UpdateHistoryBuffers();
		m_HbaoPass.RenderPassEvent = GetPassEvent(component);
		if (m_HbaoPass.Setup(component, this))
		{
			renderer.EnqueuePass(m_HbaoPass);
		}
	}

	private RenderPassEvent GetPassEvent(HBAO hbaoSettings)
	{
		switch (hbaoSettings.mode.value)
		{
		case HBAO.Mode.Normal:
			return RenderPassEvent.AfterRenderingDeferredLights;
		case HBAO.Mode.LitAO:
		{
			HBAO.DebugMode value = hbaoSettings.debugMode.value;
			if (value != 0 && (uint)(value - 1) <= 5u)
			{
				return RenderPassEvent.AfterRenderingDeferredLights;
			}
			return (RenderPassEvent)219;
		}
		default:
			return (RenderPassEvent)219;
		}
	}

	private void UpdateHistoryBuffers()
	{
		for (int num = m_CameraHistoryBuffers.Count - 1; num >= 0; num--)
		{
			HBAOHistoryBuffer buffers = m_CameraHistoryBuffers[num];
			if (Time.frameCount - buffers.lastRenderedFrame > 1)
			{
				ReleaseCameraHistoryBuffers(ref buffers);
			}
		}
	}

	public HBAOHistoryBuffer GetCurrentCameraHistoryBuffersRG(WaaaghCameraData cameraData, HBAO settings)
	{
		HBAOHistoryBuffer buffers = null;
		if (settings.temporalFilterEnabled.value && cameraData.cameraType != CameraType.SceneView)
		{
			for (int i = 0; i < m_CameraHistoryBuffers.Count; i++)
			{
				if (m_CameraHistoryBuffers[i].camera == cameraData.camera)
				{
					buffers = m_CameraHistoryBuffers[i];
					break;
				}
			}
			if ((m_PreviousColorBleedingEnabled != settings.colorBleedingEnabled.value || m_PreviousResolution != settings.resolution.value) && buffers != null)
			{
				ReleaseCameraHistoryBuffers(ref buffers);
				m_PreviousColorBleedingEnabled = settings.colorBleedingEnabled.value;
				m_PreviousResolution = settings.resolution.value;
			}
			if (buffers == null)
			{
				AllocCameraHistoryBuffersRG(cameraData, ref buffers, settings);
			}
		}
		return buffers;
	}

	private void AllocCameraHistoryBuffersRG(WaaaghCameraData cameraData, ref HBAOHistoryBuffer buffers, HBAO settings)
	{
		buffers = new HBAOHistoryBuffer();
		buffers.camera = cameraData.camera;
		buffers.frameCount = 0;
		buffers.historyRTSystem = new BufferedRTHandleSystem();
		buffers.historyRTSystem.AllocBuffer(0, HistoryBufferAllocator, 2);
		if (settings.colorBleedingEnabled.value)
		{
			buffers.historyRTSystem.AllocBuffer(1, HistoryBufferAllocator, 2);
		}
		m_CameraHistoryBuffers.Add(buffers);
	}

	private RTHandle HistoryBufferAllocator(RTHandleSystem rtHandleSystem, int frameIndex)
	{
		TextureDimension textureDimension = TextureDimension.Tex2D;
		int slices = 1;
		Vector2 one = Vector2.one;
		GraphicsFormat graphicsColorFormat = m_HbaoPass.graphicsColorFormat;
		string text = "HBAO_HistoryBuffer_" + frameIndex;
		TextureDimension dimension = textureDimension;
		return rtHandleSystem.Alloc(one, slices, DepthBits.None, graphicsColorFormat, FilterMode.Point, TextureWrapMode.Repeat, dimension, enableRandomWrite: false, useMipMap: false, autoGenerateMips: true, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: true, useDynamicScaleExplicit: false, RenderTextureMemoryless.None, VRTextureUsage.None, text);
	}

	private void ReleaseCameraHistoryBuffers(ref HBAOHistoryBuffer buffers)
	{
		buffers.historyRTSystem.ReleaseAll();
		buffers.historyRTSystem.Dispose();
		m_CameraHistoryBuffers.Remove(buffers);
		buffers = null;
	}

	private void CheckNoiseTexture(HBAO hbaoSettings)
	{
		if (m_NoiseTexture == null || m_PreviousNoiseType != hbaoSettings.noiseType.value)
		{
			CoreUtils.Destroy(m_NoiseTexture);
			CreateNoiseTexture(hbaoSettings);
			m_PreviousNoiseType = hbaoSettings.noiseType.value;
		}
	}

	private void CreateNoiseTexture(HBAO settings)
	{
		m_NoiseTexture = new Texture2D(4, 4, SystemInfo.SupportsTextureFormat(TextureFormat.RGHalf) ? TextureFormat.RGHalf : TextureFormat.RGB24, mipChain: false, linear: true);
		m_NoiseTexture.filterMode = FilterMode.Point;
		m_NoiseTexture.wrapMode = TextureWrapMode.Repeat;
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				float r = ((settings.noiseType.value != 0) ? (0.25f * (0.0625f * (float)(((i + j) & 3) << 2) + (float)(i & 3))) : MersenneTwister.Numbers[num++]);
				float g = ((settings.noiseType.value != 0) ? (0.25f * (float)((j - i) & 3)) : MersenneTwister.Numbers[num++]);
				Color color = new Color(r, g, 0f);
				m_NoiseTexture.SetPixel(i, j, color);
			}
		}
		m_NoiseTexture.Apply();
		int k = 0;
		int num2 = 0;
		for (; k < s_jitter.Length; k++)
		{
			float x = MersenneTwister.Numbers[num2++];
			float y = MersenneTwister.Numbers[num2++];
			s_jitter[k] = new Vector2(x, y);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		for (int num = m_CameraHistoryBuffers.Count - 1; num >= 0; num--)
		{
			HBAOHistoryBuffer buffers = m_CameraHistoryBuffers[num];
			ReleaseCameraHistoryBuffers(ref buffers);
		}
		CoreUtils.Destroy(m_HbaoMaterial);
		CoreUtils.Destroy(m_NoiseTexture);
		CoreUtils.Destroy(m_FullscreenTriangle);
	}
}
