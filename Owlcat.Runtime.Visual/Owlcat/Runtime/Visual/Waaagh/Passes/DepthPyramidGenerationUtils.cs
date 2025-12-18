using System;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Owlcat.ShaderLibrary.Visual.ThirdParty.ffx.spd;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

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

	public struct Context
	{
		public Resources Resources;

		public TextureDesc PyramidDesc;

		public BufferDesc GlobalAtomicCounterDesc;

		public bool UseMax;

		public int LodCount;

		public DepthPyramidGenerationAlgorithm Algorithm;
	}

	public class Resources
	{
		public readonly int[] DstOffset = new int[4];

		public readonly Vector4[] PyramidMipRects = new Vector4[16];

		public readonly int[] SrcOffset = new int[4];

		public readonly int[] WorkGroupOffset = new int[2];

		public int PyramidLodCount;

		public Vector4 PyramidSamplingRatio;

		public int2 PyramidTextureSize;
	}

	private readonly struct DownsamplingShader
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

	private readonly DownsamplingShader m_FfxSpdShader;

	private readonly DownsamplingShader m_ReferenceShader;

	private readonly WaaaghRendererData m_Settings;

	public DepthPyramidGenerationUtils(WaaaghRendererData settings)
	{
		m_Settings = settings;
		m_ReferenceShader = new DownsamplingShader(settings.Shaders.DepthPyramidCS, "DepthPyramid");
		m_FfxSpdShader = new DownsamplingShader(settings.Shaders.DepthPyramidFfxSpdCS, "DepthPyramid");
	}

	[MustUseReturnValue]
	public Context Setup(int2 viewportSize, Resources sharedResources, bool useMax, int lodCount = -1)
	{
		ComputePackedMipChainInfo(viewportSize, sharedResources);
		DepthPyramidGenerationAlgorithm depthPyramidGenerationAlgorithm = m_Settings.DepthPyramidGenerationAlgorithm;
		ref readonly DownsamplingShader reference = ref ResolveAlgorithm(depthPyramidGenerationAlgorithm);
		if (!reference.Shader.IsSupported(reference.Kernel.Index))
		{
			depthPyramidGenerationAlgorithm = DepthPyramidGenerationAlgorithm.Reference;
		}
		Context result = default(Context);
		result.Resources = sharedResources;
		result.PyramidDesc = new TextureDesc(sharedResources.PyramidTextureSize.x, sharedResources.PyramidTextureSize.y)
		{
			colorFormat = GraphicsFormat.R32_SFloat,
			enableRandomWrite = true
		};
		result.UseMax = useMax;
		result.LodCount = lodCount;
		result.Algorithm = depthPyramidGenerationAlgorithm;
		result.GlobalAtomicCounterDesc = ((depthPyramidGenerationAlgorithm == DepthPyramidGenerationAlgorithm.AMDFidelityFX) ? new BufferDesc(1, 4, GraphicsBuffer.Target.Raw)
		{
			name = "GlobalAtomicCounter"
		} : default(BufferDesc));
		return result;
	}

	private ref readonly DownsamplingShader ResolveAlgorithm(DepthPyramidGenerationAlgorithm algorithm)
	{
		return algorithm switch
		{
			DepthPyramidGenerationAlgorithm.Reference => ref m_ReferenceShader, 
			DepthPyramidGenerationAlgorithm.AMDFidelityFX => ref m_FfxSpdShader, 
			_ => throw new ArgumentOutOfRangeException("algorithm", algorithm, null), 
		};
	}

	public void Render(RenderGraphContext rgContext, in Context pyramidContext, TextureHandle sourceDepth, TextureHandle pyramidUAV, BufferHandle globalAtomicCounter, int fromMip = 1, int toMipExclusive = int.MaxValue)
	{
		if (FrameDebugger.enabled)
		{
			rgContext.cmd.SetRenderTarget(pyramidUAV);
		}
		Resources resources = pyramidContext.Resources;
		int x = resources.PyramidLodCount;
		if (pyramidContext.LodCount > 0)
		{
			x = math.min(x, pyramidContext.LodCount);
		}
		x = math.min(x, toMipExclusive);
		ref readonly DownsamplingShader reference = ref ResolveAlgorithm(pyramidContext.Algorithm);
		switch (pyramidContext.Algorithm)
		{
		case DepthPyramidGenerationAlgorithm.AMDFidelityFX:
		{
			int num = fromMip - 1;
			rgContext.cmd.SetBufferData(globalAtomicCounter, s_CounterClearValue);
			rgContext.cmd.SetKeyword(reference.Shader, in reference.USE_MAX, pyramidContext.UseMax);
			rgContext.cmd.SetKeyword(reference.Shader, in reference.READ_FROM_CAMERA_DEPTH, num == 0);
			uint2 dispatchThreadGroupCountXY = default(uint2);
			uint2 workGroupOffset = default(uint2);
			uint2 numWorkGroupsAndMips = default(uint2);
			uint4 rectInfo = new uint4(0u, 0u, (uint)resources.PyramidMipRects[num].z, (uint)resources.PyramidMipRects[num].w);
			ffx_spd_h.ffxSpdSetup(ref dispatchThreadGroupCountXY, ref workGroupOffset, ref numWorkGroupsAndMips, rectInfo, x - 1 - num);
			resources.WorkGroupOffset[0] = (int)workGroupOffset.x;
			resources.WorkGroupOffset[1] = (int)workGroupOffset.y;
			int4 int3 = (int4)resources.PyramidMipRects[num];
			resources.SrcOffset[0] = int3.x;
			resources.SrcOffset[1] = int3.y;
			resources.SrcOffset[2] = int3.x + int3.z - 1;
			resources.SrcOffset[3] = int3.y + int3.w - 1;
			rgContext.cmd.SetComputeIntParam(reference.Shader, ShaderConstantsId.FfxSpd.mips, (int)numWorkGroupsAndMips.y);
			rgContext.cmd.SetComputeIntParam(reference.Shader, ShaderConstantsId.FfxSpd.numWorkGroups, (int)numWorkGroupsAndMips.x);
			rgContext.cmd.SetComputeIntParams(reference.Shader, ShaderConstantsId.FfxSpd.workGroupOffset, resources.WorkGroupOffset);
			rgContext.cmd.SetComputeVectorParam(reference.Shader, ShaderConstantsId.FfxSpd.padding, Vector4.zero);
			int2 pyramidTextureSize2 = resources.PyramidTextureSize;
			rgContext.cmd.SetComputeVectorParam(reference.Shader, ShaderConstantsId._CameraDepthUAVSize, new Vector4(pyramidTextureSize2.x, pyramidTextureSize2.y));
			rgContext.cmd.SetComputeIntParams(reference.Shader, ShaderConstantsId._SrcOffsetAndLimit, resources.SrcOffset);
			rgContext.cmd.SetComputeVectorArrayParam(reference.Shader, ShaderConstantsId.FfxSpd._MipRects, pyramidContext.Resources.PyramidMipRects);
			rgContext.cmd.SetComputeIntParam(reference.Shader, ShaderConstantsId.FfxSpd._SourceMip, num);
			rgContext.cmd.SetComputeTextureParam(reference.Shader, reference.Kernel.Index, ShaderPropertyId._CameraDepthRT, sourceDepth);
			rgContext.cmd.SetComputeTextureParam(reference.Shader, reference.Kernel.Index, ShaderConstantsId._CameraDepthUAV, pyramidUAV);
			rgContext.cmd.SetComputeTextureParam(reference.Shader, reference.Kernel.Index, ShaderConstantsId.FfxSpd._MidMip, pyramidUAV);
			rgContext.cmd.SetComputeBufferParam(reference.Shader, reference.Kernel.Index, ShaderConstantsId.FfxSpd._GlobalAtomicCounter, globalAtomicCounter);
			rgContext.cmd.DispatchCompute(reference.Shader, reference.Kernel.Index, (int)dispatchThreadGroupCountXY.x, (int)dispatchThreadGroupCountXY.y, 1);
			break;
		}
		case DepthPyramidGenerationAlgorithm.Reference:
		{
			rgContext.cmd.SetKeyword(reference.Shader, in reference.USE_MAX, pyramidContext.UseMax);
			for (int i = fromMip; i < x; i++)
			{
				int4 @int = (int4)resources.PyramidMipRects[i];
				int4 int2 = (int4)resources.PyramidMipRects[i - 1];
				resources.SrcOffset[0] = int2.x;
				resources.SrcOffset[1] = int2.y;
				resources.SrcOffset[2] = int2.x + int2.z - 1;
				resources.SrcOffset[3] = int2.y + int2.w - 1;
				resources.DstOffset[0] = @int.x;
				resources.DstOffset[1] = @int.y;
				resources.DstOffset[2] = 0;
				resources.DstOffset[3] = 0;
				rgContext.cmd.SetComputeIntParams(reference.Shader, ShaderConstantsId._SrcOffsetAndLimit, resources.SrcOffset);
				rgContext.cmd.SetComputeIntParams(reference.Shader, ShaderConstantsId._DstOffset, resources.DstOffset);
				int2 pyramidTextureSize = resources.PyramidTextureSize;
				rgContext.cmd.SetComputeVectorParam(reference.Shader, ShaderConstantsId._CameraDepthUAVSize, new Vector4(pyramidTextureSize.x, pyramidTextureSize.y));
				rgContext.cmd.SetComputeTextureParam(reference.Shader, reference.Kernel.Index, ShaderConstantsId._CameraDepthUAV, pyramidUAV);
				rgContext.cmd.SetKeyword(reference.Shader, in reference.READ_FROM_CAMERA_DEPTH, i == 1);
				if (i == 1)
				{
					rgContext.cmd.SetComputeTextureParam(reference.Shader, reference.Kernel.Index, ShaderPropertyId._CameraDepthRT, sourceDepth);
				}
				rgContext.cmd.DispatchCompute(reference.Shader, reference.Kernel.Index, RenderingUtils.DivRoundUp(@int.z, (int)reference.Kernel.ThreadGroupSize.x), RenderingUtils.DivRoundUp(@int.w, (int)reference.Kernel.ThreadGroupSize.y), 1);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static void ComputePackedMipChainInfo(int2 viewportSize, Resources resources)
	{
		ref int2 pyramidTextureSize = ref resources.PyramidTextureSize;
		pyramidTextureSize = viewportSize >> 1;
		resources.PyramidMipRects[0] = new Vector4(0f, 0f, viewportSize.x, viewportSize.y);
		int num = 0;
		int2 @int = viewportSize;
		do
		{
			num++;
			@int.x = Math.Max(1, @int.x + 1 >> 1);
			@int.y = Math.Max(1, @int.y + 1 >> 1);
			float4 @float = resources.PyramidMipRects[num - 1];
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
			resources.PyramidMipRects[num] = new Vector4(int4.x, int4.y, @int.x, @int.y);
			pyramidTextureSize.x = Math.Max(pyramidTextureSize.x, int4.x + @int.x);
			pyramidTextureSize.y = Math.Max(pyramidTextureSize.y, int4.y + @int.y);
		}
		while (@int.x > 1 || @int.y > 1);
		resources.PyramidLodCount = num + 1;
		resources.PyramidSamplingRatio.x = (float)viewportSize.x / (float)pyramidTextureSize.x;
		resources.PyramidSamplingRatio.y = (float)viewportSize.y / (float)pyramidTextureSize.y;
	}
}
