using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

internal sealed class DeferredLightingComputePass : ScriptableRenderPass<DeferredLightingComputePass.PassData>
{
	public sealed class PassData : PassDataBase
	{
		public ShadowQuality ShadowQuality;

		public Color GlossyEnvironmentColor;

		public TextureHandle CameraColorRT;

		public bool IsCameraColorSrgb;

		public int2 CameraColorSize;

		public int2 FeatureTilesDimensions;

		public BufferHandle FeatureTilesBuffer;

		public BufferHandle IndirectArgsBuffer;

		public BufferHandle FeatureTilesListsBuffer;
	}

	private static int s_Result = Shader.PropertyToID("_Result");

	private static int s_FeatureTiles = Shader.PropertyToID("_FeatureTiles");

	private static int s_FeatureTilesLists = Shader.PropertyToID("_FeatureTilesLists");

	private static int s_ResultSizeX = Shader.PropertyToID("_ResultSizeX");

	private static int s_ResultSizeY = Shader.PropertyToID("_ResultSizeY");

	private static int s_FeatureTilesCount = Shader.PropertyToID("_FeatureTilesCount");

	private static int s_FeatureTilesCountX = Shader.PropertyToID("_FeatureTilesCountX");

	private readonly ComputeShader m_Shader;

	private readonly int[] m_VariantKernels;

	private readonly LocalKeyword m_LinearToSrgbConversion;

	public override string Name => "DeferredLightingComputePass";

	private protected override WaaaghProfileId? ProfileId => WaaaghProfileId.DeferredLightingPass;

	public DeferredLightingComputePass(RenderPassEvent evt, [NotNull] ComputeShader shader)
		: base(evt)
	{
		m_Shader = shader;
		m_VariantKernels = new int[8];
		for (int i = 0; i < 8; i++)
		{
			m_VariantKernels[i] = shader.FindKernel($"Deferred_Indirect_Variant{i}");
		}
		m_LinearToSrgbConversion = new LocalKeyword(shader, "_LINEAR_TO_SRGB_CONVERSION");
	}

	protected override void Setup(RenderGraphBuilder builder, PassData passData, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghShadowData waaaghShadowData = frameData.Get<WaaaghShadowData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		passData.CameraColorRT = builder.WriteTexture(in input);
		builder.ReadTexture(in waaaghResourceData.CameraAlbedoRT);
		builder.ReadTexture(in waaaghResourceData.CameraBakedGIRT);
		input = waaaghResourceData.CameraDepthBuffer;
		builder.ReadTexture(in input);
		builder.ReadTexture(in waaaghResourceData.CameraNormalsRT);
		builder.ReadTexture(in waaaghResourceData.CameraShadowmaskRT);
		builder.ReadTexture(in waaaghResourceData.CameraSpecularRT);
		builder.ReadTexture(in waaaghResourceData.CameraTranslucencyRT);
		if (waaaghResourceData.Shadowmap.IsValid())
		{
			input = waaaghResourceData.Shadowmap;
			builder.ReadTexture(in input);
		}
		SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
		Color glossyEnvironmentColor = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * RenderSettings.reflectionIntensity);
		passData.GlossyEnvironmentColor = glossyEnvironmentColor;
		passData.ShadowQuality = waaaghShadowData.ShadowQuality;
		passData.IsCameraColorSrgb = GraphicsFormatUtility.IsSRGBFormat(waaaghCameraData.cameraTargetDescriptor.graphicsFormat);
		passData.CameraColorSize.x = waaaghCameraData.cameraTargetDescriptor.width;
		passData.CameraColorSize.y = waaaghCameraData.cameraTargetDescriptor.height;
		passData.FeatureTilesDimensions = new int2(Mathf.CeilToInt((float)waaaghCameraData.cameraTargetDescriptor.width / 16f), Mathf.CeilToInt((float)waaaghCameraData.cameraTargetDescriptor.height / 16f));
		passData.FeatureTilesBuffer = builder.ReadBuffer(in waaaghResourceData.DeferredLightingFeatureTilesBuffer);
		passData.IndirectArgsBuffer = builder.ReadBuffer(in waaaghResourceData.DeferredLightingIndirectArgsBuffer);
		passData.FeatureTilesListsBuffer = builder.ReadBuffer(in waaaghResourceData.DeferredLightingFeatureTilesListsBuffer);
	}

	protected override void Render(PassData passData, RenderGraphContext context)
	{
		if (FrameDebugger.enabled)
		{
			context.cmd.SetRenderTarget(passData.CameraColorRT);
		}
		CommandBuffer cmd = context.cmd;
		ComputeShader shader = m_Shader;
		cmd.SetKeyword(shader, in m_LinearToSrgbConversion, passData.IsCameraColorSrgb);
		cmd.SetComputeVectorParam(shader, ShaderPropertyId._GlossyEnvironmentColor, Color.clear);
		cmd.SetComputeIntParam(shader, s_ResultSizeX, passData.CameraColorSize.x);
		cmd.SetComputeIntParam(shader, s_ResultSizeY, passData.CameraColorSize.y);
		cmd.SetComputeIntParam(shader, s_FeatureTilesCount, passData.FeatureTilesDimensions.x * passData.FeatureTilesDimensions.y);
		cmd.SetComputeIntParam(shader, s_FeatureTilesCountX, passData.FeatureTilesDimensions.x);
		for (int i = 0; i < m_VariantKernels.Length; i++)
		{
			int kernelIndex = m_VariantKernels[i];
			cmd.SetComputeTextureParam(shader, kernelIndex, s_Result, passData.CameraColorRT);
			cmd.SetComputeBufferParam(shader, kernelIndex, s_FeatureTiles, passData.FeatureTilesBuffer);
			cmd.SetComputeBufferParam(shader, kernelIndex, s_FeatureTilesLists, passData.FeatureTilesListsBuffer);
			cmd.DispatchCompute(shader, kernelIndex, passData.IndirectArgsBuffer, (uint)(i * 12));
		}
		cmd.SetComputeVectorParam(shader, ShaderPropertyId._GlossyEnvironmentColor, passData.GlossyEnvironmentColor);
	}
}
