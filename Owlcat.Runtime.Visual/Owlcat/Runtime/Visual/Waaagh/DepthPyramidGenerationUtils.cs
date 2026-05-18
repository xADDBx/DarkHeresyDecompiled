using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Owlcat.ShaderLibrary.Visual.ThirdParty.ffx.spd;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh;

public sealed class DepthPyramidGenerationUtils
{
	private static class ShaderConstantsId
	{
		public static class FfxSpd
		{
			public static readonly int mips = Shader.PropertyToID("mips");

			public static readonly int numWorkGroups = Shader.PropertyToID("numWorkGroups");

			public static readonly int workGroupOffset = Shader.PropertyToID("workGroupOffset");

			public static readonly int padding = Shader.PropertyToID("padding");

			public static readonly int _MipRects = Shader.PropertyToID("_MipRects");

			public static readonly int _SourceMip = Shader.PropertyToID("_SourceMip");

			public static readonly int _MidMip = Shader.PropertyToID("_MidMip");

			public static readonly int _GlobalAtomicCounter = Shader.PropertyToID("_GlobalAtomicCounter");
		}

		public static readonly int _SrcOffsetAndLimit = Shader.PropertyToID("_SrcOffsetAndLimit");

		public static readonly int _DstOffset = Shader.PropertyToID("_DstOffset");

		public static readonly int _CameraDepthUAVSize = Shader.PropertyToID("_CameraDepthUAVSize");

		public static readonly int _CameraDepthUAV = Shader.PropertyToID("_CameraDepthUAV");
	}

	public struct PyramidParameters
	{
		public TextureDesc PyramidDesc;

		public BufferDesc GlobalAtomicCounterDesc;

		public DepthPyramidGenerationAlgorithm Algorithm;

		public DownsamplingShader AlgorithmShader;

		public bool UseMax;

		public int LodCount;

		public int PyramidLodCount;

		public Vector4 PyramidSamplingRatio;

		public int2 PyramidTextureSize;

		public Vector4[] PyramidMipRects;
	}

	public readonly struct DownsamplingShader
	{
		public readonly ComputeShader Shader;

		public readonly ComputeShaderKernelDescriptor Kernel;

		public readonly LocalKeyword READ_FROM_CAMERA_DEPTH;

		public readonly LocalKeyword USE_MAX;

		public DownsamplingShader(ComputeShader shader, string kernelName)
		{
			Shader = shader;
			Kernel = shader.GetKernelDescriptor(kernelName);
			READ_FROM_CAMERA_DEPTH = new LocalKeyword(shader, "READ_FROM_CAMERA_DEPTH");
			USE_MAX = new LocalKeyword(shader, "USE_MAX");
		}
	}

	private const int kMaxDepthPyramidLevelCount = 16;

	private static readonly int[] s_CounterClearValue = new int[1];

	private readonly WaaaghRendererData m_Settings;

	private readonly DownsamplingShader m_ReferenceShader;

	private readonly DownsamplingShader m_FfxSpdShader;

	private readonly bool m_FfxSpdShaderSupported;

	private static readonly int[] s_DstOffset = new int[4];

	private static readonly int[] s_SrcOffset = new int[4];

	private static readonly int[] s_WorkGroupOffset = new int[2];

	public DepthPyramidGenerationUtils(WaaaghRendererData settings)
	{
		m_Settings = settings;
		m_ReferenceShader = new DownsamplingShader(settings.Shaders.DepthPyramidCS, "DepthPyramid");
		m_FfxSpdShader = new DownsamplingShader(settings.Shaders.DepthPyramidFfxSpdCS, "DepthPyramid");
		m_FfxSpdShaderSupported = m_FfxSpdShader.Shader.IsSupported(m_FfxSpdShader.Kernel.Index);
	}

	public void PopulateGenerationData(ref PyramidParameters pyramidParameters, int2 viewportSize, bool useMax, int lodCount = -1)
	{
		ComputePackedMipChainInfo(viewportSize, out pyramidParameters.PyramidTextureSize, ref pyramidParameters.PyramidMipRects, out pyramidParameters.PyramidLodCount, out pyramidParameters.PyramidSamplingRatio);
		DepthPyramidGenerationAlgorithm depthPyramidGenerationAlgorithm = ((m_Settings.DepthPyramidGenerationAlgorithm == DepthPyramidGenerationAlgorithm.AMDFidelityFX) ? (m_FfxSpdShaderSupported ? DepthPyramidGenerationAlgorithm.AMDFidelityFX : DepthPyramidGenerationAlgorithm.Reference) : DepthPyramidGenerationAlgorithm.Reference);
		pyramidParameters.PyramidDesc = new TextureDesc(pyramidParameters.PyramidTextureSize.x, pyramidParameters.PyramidTextureSize.y)
		{
			colorFormat = GraphicsFormat.R32_SFloat,
			enableRandomWrite = true
		};
		pyramidParameters.UseMax = useMax;
		pyramidParameters.LodCount = lodCount;
		pyramidParameters.Algorithm = depthPyramidGenerationAlgorithm;
		pyramidParameters.AlgorithmShader = ((depthPyramidGenerationAlgorithm == DepthPyramidGenerationAlgorithm.AMDFidelityFX) ? m_FfxSpdShader : m_ReferenceShader);
		pyramidParameters.GlobalAtomicCounterDesc = ((depthPyramidGenerationAlgorithm == DepthPyramidGenerationAlgorithm.AMDFidelityFX) ? new BufferDesc(1, 4, GraphicsBuffer.Target.Raw)
		{
			name = "GlobalAtomicCounter"
		} : default(BufferDesc));
	}

	public static void Render(UnsafeGraphContext context, in PyramidParameters pyramidParameters, TextureHandle sourceDepth, TextureHandle pyramidUAV, BufferHandle globalAtomicCounter, int fromMip = 1, int toMipExclusive = int.MaxValue)
	{
		if (FrameDebugger.enabled)
		{
			context.cmd.SetRenderTarget(pyramidUAV);
		}
		int x = pyramidParameters.PyramidLodCount;
		if (pyramidParameters.LodCount > 0)
		{
			x = math.min(x, pyramidParameters.LodCount);
		}
		x = math.min(x, toMipExclusive);
		ref readonly DownsamplingShader algorithmShader = ref pyramidParameters.AlgorithmShader;
		switch (pyramidParameters.Algorithm)
		{
		case DepthPyramidGenerationAlgorithm.AMDFidelityFX:
		{
			int num = fromMip - 1;
			context.cmd.SetBufferData(globalAtomicCounter, s_CounterClearValue);
			context.cmd.SetKeyword(algorithmShader.Shader, in algorithmShader.USE_MAX, pyramidParameters.UseMax);
			context.cmd.SetKeyword(algorithmShader.Shader, in algorithmShader.READ_FROM_CAMERA_DEPTH, num == 0);
			uint2 dispatchThreadGroupCountXY = default(uint2);
			uint2 workGroupOffset = default(uint2);
			uint2 numWorkGroupsAndMips = default(uint2);
			uint4 rectInfo = new uint4(0u, 0u, (uint)pyramidParameters.PyramidMipRects[num].z, (uint)pyramidParameters.PyramidMipRects[num].w);
			ffx_spd_h.ffxSpdSetup(ref dispatchThreadGroupCountXY, ref workGroupOffset, ref numWorkGroupsAndMips, rectInfo, x - 1 - num);
			s_WorkGroupOffset[0] = (int)workGroupOffset.x;
			s_WorkGroupOffset[1] = (int)workGroupOffset.y;
			int4 int3 = (int4)pyramidParameters.PyramidMipRects[num];
			s_SrcOffset[0] = int3.x;
			s_SrcOffset[1] = int3.y;
			s_SrcOffset[2] = int3.x + int3.z - 1;
			s_SrcOffset[3] = int3.y + int3.w - 1;
			context.cmd.SetComputeIntParam(algorithmShader.Shader, ShaderConstantsId.FfxSpd.mips, (int)numWorkGroupsAndMips.y);
			context.cmd.SetComputeIntParam(algorithmShader.Shader, ShaderConstantsId.FfxSpd.numWorkGroups, (int)numWorkGroupsAndMips.x);
			context.cmd.SetComputeIntParams(algorithmShader.Shader, ShaderConstantsId.FfxSpd.workGroupOffset, s_WorkGroupOffset);
			context.cmd.SetComputeVectorParam(algorithmShader.Shader, ShaderConstantsId.FfxSpd.padding, Vector4.zero);
			int2 pyramidTextureSize2 = pyramidParameters.PyramidTextureSize;
			context.cmd.SetComputeVectorParam(algorithmShader.Shader, ShaderConstantsId._CameraDepthUAVSize, new Vector4(pyramidTextureSize2.x, pyramidTextureSize2.y));
			context.cmd.SetComputeIntParams(algorithmShader.Shader, ShaderConstantsId._SrcOffsetAndLimit, s_SrcOffset);
			context.cmd.SetComputeVectorArrayParam(algorithmShader.Shader, ShaderConstantsId.FfxSpd._MipRects, pyramidParameters.PyramidMipRects);
			context.cmd.SetComputeIntParam(algorithmShader.Shader, ShaderConstantsId.FfxSpd._SourceMip, num);
			context.cmd.SetComputeTextureParam(algorithmShader.Shader, algorithmShader.Kernel.Index, GlobalTextureShaderPropertyId._CameraDepthRT, sourceDepth);
			context.cmd.SetComputeTextureParam(algorithmShader.Shader, algorithmShader.Kernel.Index, ShaderConstantsId._CameraDepthUAV, pyramidUAV);
			context.cmd.SetComputeTextureParam(algorithmShader.Shader, algorithmShader.Kernel.Index, ShaderConstantsId.FfxSpd._MidMip, pyramidUAV);
			context.cmd.SetComputeBufferParam(algorithmShader.Shader, algorithmShader.Kernel.Index, ShaderConstantsId.FfxSpd._GlobalAtomicCounter, globalAtomicCounter);
			context.cmd.DispatchCompute(algorithmShader.Shader, algorithmShader.Kernel.Index, (int)dispatchThreadGroupCountXY.x, (int)dispatchThreadGroupCountXY.y, 1);
			break;
		}
		case DepthPyramidGenerationAlgorithm.Reference:
		{
			context.cmd.SetKeyword(algorithmShader.Shader, in algorithmShader.USE_MAX, pyramidParameters.UseMax);
			for (int i = fromMip; i < x; i++)
			{
				int4 @int = (int4)pyramidParameters.PyramidMipRects[i];
				int4 int2 = (int4)pyramidParameters.PyramidMipRects[i - 1];
				s_SrcOffset[0] = int2.x;
				s_SrcOffset[1] = int2.y;
				s_SrcOffset[2] = int2.x + int2.z - 1;
				s_SrcOffset[3] = int2.y + int2.w - 1;
				s_DstOffset[0] = @int.x;
				s_DstOffset[1] = @int.y;
				s_DstOffset[2] = 0;
				s_DstOffset[3] = 0;
				context.cmd.SetComputeIntParams(algorithmShader.Shader, ShaderConstantsId._SrcOffsetAndLimit, s_SrcOffset);
				context.cmd.SetComputeIntParams(algorithmShader.Shader, ShaderConstantsId._DstOffset, s_DstOffset);
				int2 pyramidTextureSize = pyramidParameters.PyramidTextureSize;
				context.cmd.SetComputeVectorParam(algorithmShader.Shader, ShaderConstantsId._CameraDepthUAVSize, new Vector4(pyramidTextureSize.x, pyramidTextureSize.y));
				context.cmd.SetComputeTextureParam(algorithmShader.Shader, algorithmShader.Kernel.Index, ShaderConstantsId._CameraDepthUAV, pyramidUAV);
				context.cmd.SetKeyword(algorithmShader.Shader, in algorithmShader.READ_FROM_CAMERA_DEPTH, i == 1);
				if (i == 1)
				{
					context.cmd.SetComputeTextureParam(algorithmShader.Shader, algorithmShader.Kernel.Index, GlobalTextureShaderPropertyId._CameraDepthRT, sourceDepth);
				}
				context.cmd.DispatchCompute(algorithmShader.Shader, algorithmShader.Kernel.Index, RenderingUtils.DivRoundUp(@int.z, (int)algorithmShader.Kernel.ThreadGroupSize.x), RenderingUtils.DivRoundUp(@int.w, (int)algorithmShader.Kernel.ThreadGroupSize.y), 1);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static void ComputePackedMipChainInfo(int2 viewportSize, out int2 pyramidTextureSize, ref Vector4[] pyramidMipRects, out int pyramidLodCount, out Vector4 pyramidSamplingRatio)
	{
		pyramidTextureSize = viewportSize >> 1;
		if (pyramidMipRects == null)
		{
			pyramidMipRects = new Vector4[16];
		}
		pyramidMipRects[0] = new Vector4(0f, 0f, viewportSize.x, viewportSize.y);
		int num = 0;
		int2 @int = viewportSize;
		do
		{
			num++;
			@int.x = Math.Max(1, @int.x + 1 >> 1);
			@int.y = Math.Max(1, @int.y + 1 >> 1);
			float4 @float = pyramidMipRects[num - 1];
			int2 int2 = (int2)@float.xy;
			int2 int3 = int2 + (int2)@float.zw;
			int2 int4 = 0;
			if (num > 1)
			{
				if (((uint)num & (true ? 1u : 0u)) != 0)
				{
					int4.x = int2.x;
					int4.y = int3.y;
				}
				else
				{
					int4.x = int3.x;
					int4.y = int2.y;
				}
			}
			pyramidMipRects[num] = new Vector4(int4.x, int4.y, @int.x, @int.y);
			pyramidTextureSize.x = Math.Max(pyramidTextureSize.x, int4.x + @int.x);
			pyramidTextureSize.y = Math.Max(pyramidTextureSize.y, int4.y + @int.y);
		}
		while (@int.x > 1 || @int.y > 1);
		pyramidLodCount = num + 1;
		pyramidSamplingRatio.x = (float)viewportSize.x / (float)pyramidTextureSize.x;
		pyramidSamplingRatio.y = (float)viewportSize.y / (float)pyramidTextureSize.y;
		pyramidSamplingRatio.z = 0f;
		pyramidSamplingRatio.w = 0f;
	}
}
