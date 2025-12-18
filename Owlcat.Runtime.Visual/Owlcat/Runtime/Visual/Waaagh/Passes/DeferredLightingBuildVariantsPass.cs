using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

internal sealed class DeferredLightingBuildVariantsPass : ScriptableRenderPass<DeferredLightingBuildVariantsPass.PassData>
{
	public sealed class PassData : PassDataBase
	{
		public BufferHandle FeatureTilesBuffer;

		public BufferHandle FeatureTilesListsBuffer;

		public BufferHandle IndirectArgsBuffer;

		public int TilesCountX;

		public int TilesCountY;
	}

	private static readonly int s_FeatureTilesCount = Shader.PropertyToID("_FeatureTilesCount");

	private static readonly int s_FeatureTilesCountX = Shader.PropertyToID("_FeatureTilesCountX");

	private static readonly int s_FeatureTiles = Shader.PropertyToID("_FeatureTiles");

	private static readonly int s_FeatureTilesLists = Shader.PropertyToID("_FeatureTilesLists");

	private static readonly int s_DispatchIndirectArgs = Shader.PropertyToID("_DispatchIndirectArgs");

	private readonly ComputeShader m_BuildFeatureTilesShader;

	private readonly ComputeShader m_ClearIndirectArgsShader;

	private readonly ComputeShader m_BuildFeatureVariantsShader;

	private readonly int m_BuildFeatureTilesKernel;

	private readonly int m_ClearIndirectArgsKernel;

	private readonly int m_BuildFeatureVariantsKernel;

	public override string Name => "DeferredLightingBuildVariantsPass";

	private protected override WaaaghProfileId? ProfileId => WaaaghProfileId.DeferredLightingBuildVariantsPass;

	public DeferredLightingBuildVariantsPass(RenderPassEvent evt, [NotNull] ComputeShader buildFeatureTilesShader, [NotNull] ComputeShader buildFeatureVariantsShader)
		: base(evt)
	{
		m_BuildFeatureTilesShader = buildFeatureTilesShader;
		m_ClearIndirectArgsShader = buildFeatureVariantsShader;
		m_BuildFeatureVariantsShader = buildFeatureVariantsShader;
		m_BuildFeatureTilesKernel = buildFeatureTilesShader.FindKernel("BuildFeatureTiles");
		m_ClearIndirectArgsKernel = buildFeatureVariantsShader.FindKernel("ClearIndirectArgs");
		m_BuildFeatureVariantsKernel = buildFeatureVariantsShader.FindKernel("BuildFeatureVariants");
	}

	protected override void Setup(RenderGraphBuilder builder, PassData passData, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		passData.FeatureTilesBuffer = builder.WriteBuffer(in waaaghResourceData.DeferredLightingFeatureTilesBuffer);
		passData.FeatureTilesListsBuffer = builder.WriteBuffer(in waaaghResourceData.DeferredLightingFeatureTilesListsBuffer);
		passData.IndirectArgsBuffer = builder.WriteBuffer(in waaaghResourceData.DeferredLightingIndirectArgsBuffer);
		passData.TilesCountX = Mathf.CeilToInt((float)waaaghCameraData.cameraTargetDescriptor.width / 16f);
		passData.TilesCountY = Mathf.CeilToInt((float)waaaghCameraData.cameraTargetDescriptor.height / 16f);
	}

	protected override void Render(PassData passData, RenderGraphContext context)
	{
		BuildFeatureTiles(passData, context);
		ClearIndirectArgs(passData, context);
		BuildFeatureVariants(passData, context);
	}

	private void BuildFeatureTiles(PassData passData, RenderGraphContext context)
	{
		ComputeShader buildFeatureTilesShader = m_BuildFeatureTilesShader;
		int buildFeatureTilesKernel = m_BuildFeatureTilesKernel;
		context.cmd.SetComputeBufferParam(buildFeatureTilesShader, buildFeatureTilesKernel, s_FeatureTiles, passData.FeatureTilesBuffer);
		context.cmd.DispatchCompute(buildFeatureTilesShader, buildFeatureTilesKernel, passData.TilesCountX, passData.TilesCountY, 1);
	}

	private void ClearIndirectArgs(PassData passData, RenderGraphContext context)
	{
		ComputeShader clearIndirectArgsShader = m_ClearIndirectArgsShader;
		int clearIndirectArgsKernel = m_ClearIndirectArgsKernel;
		context.cmd.SetComputeBufferParam(clearIndirectArgsShader, clearIndirectArgsKernel, s_DispatchIndirectArgs, passData.IndirectArgsBuffer);
		context.cmd.DispatchCompute(clearIndirectArgsShader, clearIndirectArgsKernel, RenderingUtils.DivRoundUp(8, 32), 1, 1);
	}

	private void BuildFeatureVariants(PassData passData, RenderGraphContext context)
	{
		ComputeShader buildFeatureVariantsShader = m_BuildFeatureVariantsShader;
		int buildFeatureVariantsKernel = m_BuildFeatureVariantsKernel;
		int num = passData.TilesCountX * passData.TilesCountY;
		context.cmd.SetComputeIntParam(buildFeatureVariantsShader, s_FeatureTilesCount, num);
		context.cmd.SetComputeIntParam(buildFeatureVariantsShader, s_FeatureTilesCountX, passData.TilesCountX);
		context.cmd.SetComputeBufferParam(buildFeatureVariantsShader, buildFeatureVariantsKernel, s_FeatureTiles, passData.FeatureTilesBuffer);
		context.cmd.SetComputeBufferParam(buildFeatureVariantsShader, buildFeatureVariantsKernel, s_DispatchIndirectArgs, passData.IndirectArgsBuffer);
		context.cmd.SetComputeBufferParam(buildFeatureVariantsShader, buildFeatureVariantsKernel, s_FeatureTilesLists, passData.FeatureTilesListsBuffer);
		context.cmd.DispatchCompute(buildFeatureVariantsShader, buildFeatureVariantsKernel, RenderingUtils.DivRoundUp(num, 32), 1, 1);
	}
}
