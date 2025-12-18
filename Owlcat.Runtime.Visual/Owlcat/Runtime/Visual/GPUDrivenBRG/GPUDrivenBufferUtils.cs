using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenBufferUtils
{
	private static class ShaderIDs
	{
		public static class RawBufferClear
		{
			public static readonly int _Buffer = Shader.PropertyToID("_Buffer");

			public static readonly int _ItemCount = Shader.PropertyToID("_ItemCount");

			public static readonly int _WriteOffset = Shader.PropertyToID("_WriteOffset");

			public static readonly int _ClearValue = Shader.PropertyToID("_ClearValue");
		}

		public static class RawBufferCopy
		{
			public static readonly int _SourceBuffer = Shader.PropertyToID("_SourceBuffer");

			public static readonly int _DestinationBuffer = Shader.PropertyToID("_DestinationBuffer");

			public static readonly int _Count = Shader.PropertyToID("_Count");
		}
	}

	private readonly ComputeShader m_RawBufferClearShader;

	private readonly ComputeShader m_RawBufferCopyShader;

	public GPUDrivenBufferUtils(PipelineRuntimeResources pipelineRuntimeResources)
	{
		m_RawBufferClearShader = pipelineRuntimeResources.RawBufferClearCS;
		m_RawBufferCopyShader = pipelineRuntimeResources.RawBufferCopyCS;
	}

	public void DispatchClearBuffer(CommandBuffer cmd, GraphicsBuffer buffer, int clearValue)
	{
		DispatchClearBuffer(cmd, buffer, clearValue, 0, buffer.count * buffer.stride);
	}

	public void DispatchClearBuffer(CommandBuffer cmd, GraphicsBuffer buffer, int clearValue, int writeOffsetInBytes, int sizeInBytes)
	{
		ComputeShader rawBufferClearShader = m_RawBufferClearShader;
		cmd.SetGlobalBuffer(ShaderIDs.RawBufferClear._Buffer, buffer);
		cmd.SetComputeIntParam(rawBufferClearShader, ShaderIDs.RawBufferClear._ItemCount, sizeInBytes / 4);
		cmd.SetComputeIntParam(rawBufferClearShader, ShaderIDs.RawBufferClear._WriteOffset, writeOffsetInBytes / 4);
		cmd.SetComputeIntParam(rawBufferClearShader, ShaderIDs.RawBufferClear._ClearValue, clearValue);
		int threadsCount = Alignment.AlignUp(sizeInBytes / 4, 4) / 4;
		cmd.DispatchCompute(rawBufferClearShader, 0, GPUDrivenComputeShaders.ComputeGroupCount(threadsCount, 32), 1, 1);
	}

	public void DispatchCopyBuffer(CommandBuffer cmd, GraphicsBuffer source, GraphicsBuffer destination, int byteCount)
	{
		ComputeShader rawBufferCopyShader = m_RawBufferCopyShader;
		cmd.SetGlobalBuffer(ShaderIDs.RawBufferCopy._SourceBuffer, source);
		cmd.SetGlobalBuffer(ShaderIDs.RawBufferCopy._DestinationBuffer, destination);
		cmd.SetComputeIntParam(rawBufferCopyShader, ShaderIDs.RawBufferCopy._Count, byteCount / 4);
		int threadsCount = Alignment.AlignUp(byteCount, 4) / 4;
		cmd.DispatchCompute(rawBufferCopyShader, 0, GPUDrivenComputeShaders.ComputeGroupCount(threadsCount, 32), 1, 1);
	}
}
