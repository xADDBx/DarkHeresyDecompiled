using JetBrains.Annotations;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public sealed class GPUDrivenDepthReprojectionUtils
{
	private static class Profiling
	{
		public static readonly ProfilingSampler Reproject = new ProfilingSampler("Reproject");

		public static readonly ProfilingSampler UnpackReprojectedDepth = new ProfilingSampler("UnpackReprojectedDepth");
	}

	public struct ReprojectionParameters
	{
		public bool Cull;

		public float DepthReprojectionBias;

		public Vector4 ReprojectedResolution;

		public Matrix4x4 SourceNdcToDestinationNdc;

		public Vector4 SourceResolution;

		public Vector4 ReprojectionDepthScaleOffset;

		public Vector4 CopyDepthScaleOffset;

		public TextureDesc PackedReprojectedDepthDesc;

		public ComputeShader DepthReprojectionCS;

		public int HistoryMipLevel;

		public Material CopyDepthMaterial;
	}

	private static class ShaderIDs
	{
		public static class DepthReprojection
		{
			public static readonly int _SourceResolution = Shader.PropertyToID("_SourceResolution");

			public static readonly int _DestinationResolution = Shader.PropertyToID("_DestinationResolution");

			public static readonly int _SourceNdcToDestinationNdc = Shader.PropertyToID("_SourceNdcToDestinationNdc");

			public static readonly int _Source = Shader.PropertyToID("_Source");

			public static readonly int _Destination = Shader.PropertyToID("_Destination");

			public static readonly int _DepthReprojectionBias = Shader.PropertyToID("_DepthReprojectionBias");

			public static readonly int _DestinationDepthScaleOffset = Shader.PropertyToID("_DestinationDepthScaleOffset");
		}

		public static class CopyDepth
		{
			public static readonly int _InputDepthTex = Shader.PropertyToID("_InputDepthTex");

			public static readonly int _BlitScaleBias = Shader.PropertyToID("_BlitScaleBias");

			public static readonly int _DepthScaleOffset = Shader.PropertyToID("_DepthScaleOffset");
		}
	}

	private const int kCopyDepthPassIndex = 4;

	private readonly Material m_CopyDepthMaterial;

	private static readonly MaterialPropertyBlock m_PropertyBlock = new MaterialPropertyBlock();

	public GPUDrivenDepthReprojectionUtils(Material copyDepthMaterial)
	{
		m_CopyDepthMaterial = copyDepthMaterial;
	}

	[MustUseReturnValue]
	public ReprojectionParameters Setup(RenderGraph renderGraph, WaaaghCameraData cameraData, GPUDrivenBatchRendererGroup brg, Matrix4x4 gpuViewProjection, out TextureHandle source)
	{
		CullingDepthHistory cullingDepthHistory = cameraData.CullingDepthHistory;
		if (cullingDepthHistory == null || !cullingDepthHistory.IsUsable())
		{
			source = TextureHandle.nullHandle;
			ReprojectionParameters result = default(ReprojectionParameters);
			result.Cull = true;
			return result;
		}
		ReprojectionParameters result2 = default(ReprojectionParameters);
		result2.Cull = false;
		result2.HistoryMipLevel = cullingDepthHistory.MipLevel;
		float2 @float = cullingDepthHistory.Resolution;
		result2.SourceResolution = math.float4(@float, 1f / @float);
		source = renderGraph.ImportBackbuffer(cullingDepthHistory.GetTexture());
		if (cameraData.camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component) && !FrameDebugger.enabled)
		{
			MotionVectorsPersistentData motionVectorsPersistentData = component.MotionVectorsPersistentData;
			result2.SourceNdcToDestinationNdc = gpuViewProjection * motionVectorsPersistentData.previousViewProjection.inverse;
		}
		else
		{
			result2.SourceNdcToDestinationNdc = Matrix4x4.identity;
		}
		int2 resolution = cullingDepthHistory.Resolution;
		result2.ReprojectedResolution = math.float4(resolution, 1f / (float2)resolution);
		float num;
		if (SystemInfo.usesReversedZBuffer)
		{
			num = math.asuint(4.2949673E+09f);
			result2.ReprojectionDepthScaleOffset = new Vector4(1f, 0f);
			result2.CopyDepthScaleOffset = new Vector4(1f, 0f);
		}
		else
		{
			num = math.asuint(0f);
			result2.ReprojectionDepthScaleOffset = new Vector4(0.9995117f, 0.00048828125f);
			result2.CopyDepthScaleOffset = new Vector4(1.0004885f, -0.0004885198f);
		}
		result2.PackedReprojectedDepthDesc = new TextureDesc(new RenderTextureDescriptor(resolution.x, resolution.y, GraphicsFormat.R32_UInt, GraphicsFormat.None))
		{
			name = "TempReprojectedDepth",
			enableRandomWrite = true,
			clearBuffer = true,
			clearColor = new Color(num, num, num, num)
		};
		result2.DepthReprojectionBias = brg.Settings.DepthReprojectionBias * (SystemInfo.usesReversedZBuffer ? (-1f) : 1f);
		result2.DepthReprojectionCS = brg.Resources.DepthReprojectionCS;
		result2.CopyDepthMaterial = m_CopyDepthMaterial;
		return result2;
	}

	public static void Reproject(in ReprojectionParameters reprojectionParameters, UnsafeCommandBuffer cmd, TextureHandle source, TextureHandle packedReprojectedDepth)
	{
		if (reprojectionParameters.Cull)
		{
			return;
		}
		using (new ProfilingScope(cmd, Profiling.Reproject))
		{
			ComputeShader depthReprojectionCS = reprojectionParameters.DepthReprojectionCS;
			int threadGroupsX = Alignment.AlignUp((int)reprojectionParameters.ReprojectedResolution.x, 32) / 32;
			int threadGroupsY = Alignment.AlignUp((int)reprojectionParameters.ReprojectedResolution.y, 32) / 32;
			cmd.SetComputeMatrixParam(depthReprojectionCS, ShaderIDs.DepthReprojection._SourceNdcToDestinationNdc, reprojectionParameters.SourceNdcToDestinationNdc);
			cmd.SetComputeVectorParam(depthReprojectionCS, ShaderIDs.DepthReprojection._SourceResolution, reprojectionParameters.SourceResolution);
			cmd.SetComputeVectorParam(depthReprojectionCS, ShaderIDs.DepthReprojection._DestinationResolution, reprojectionParameters.ReprojectedResolution);
			cmd.SetComputeVectorParam(depthReprojectionCS, ShaderIDs.DepthReprojection._DestinationDepthScaleOffset, reprojectionParameters.ReprojectionDepthScaleOffset);
			cmd.SetComputeFloatParam(depthReprojectionCS, ShaderIDs.DepthReprojection._DepthReprojectionBias, reprojectionParameters.DepthReprojectionBias);
			cmd.SetComputeTextureParam(depthReprojectionCS, 0, ShaderIDs.DepthReprojection._Source, source);
			cmd.SetComputeTextureParam(depthReprojectionCS, 0, ShaderIDs.DepthReprojection._Destination, packedReprojectedDepth);
			cmd.DispatchCompute(depthReprojectionCS, 0, threadGroupsX, threadGroupsY, 1);
		}
	}

	public static void UnpackReprojectedDepth(in ReprojectionParameters reprojectionParameters, UnsafeCommandBuffer cmd, TextureHandle packedReprojectedDepth, TextureHandle destination, Rect destinationViewport)
	{
		using (new ProfilingScope(cmd, Profiling.UnpackReprojectedDepth))
		{
			cmd.SetRenderTarget(destination);
			cmd.SetViewport(destinationViewport);
			m_PropertyBlock.Clear();
			m_PropertyBlock.SetTexture(ShaderIDs.CopyDepth._InputDepthTex, packedReprojectedDepth);
			m_PropertyBlock.SetVector(ShaderIDs.CopyDepth._BlitScaleBias, new Vector4(1f, 1f, 0f, 0f));
			m_PropertyBlock.SetVector(ShaderIDs.CopyDepth._DepthScaleOffset, reprojectionParameters.CopyDepthScaleOffset);
			cmd.DrawProcedural(Matrix4x4.identity, reprojectionParameters.CopyDepthMaterial, 4, MeshTopology.Triangles, 3, 1, m_PropertyBlock);
		}
	}
}
