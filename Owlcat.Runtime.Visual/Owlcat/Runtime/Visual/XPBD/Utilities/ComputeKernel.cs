using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Utilities;

public class ComputeKernel
{
	private ComputeShader m_Shader;

	public string Name;

	public int Index;

	public int3 NumThreads;

	public ComputeKernel(ComputeShader shader, string kernalName)
	{
		Name = kernalName;
		m_Shader = shader;
		Index = shader.FindKernel(Name);
		uint3 @uint = default(uint3);
		shader.GetKernelThreadGroupSizes(Index, out @uint.x, out @uint.y, out @uint.z);
		NumThreads = (int3)@uint;
	}

	internal void Dispatch(CommandBuffer cmd, int numThreadsX, int numThreadsY, int numThreadsZ)
	{
		cmd.DispatchCompute(m_Shader, Index, numThreadsX, numThreadsY, numThreadsZ);
	}

	internal void DispatchIndirect(CommandBuffer cmd, GraphicsBuffer indirectArgsBuffer, uint argsOffset)
	{
		cmd.DispatchCompute(m_Shader, Index, indirectArgsBuffer, argsOffset);
	}
}
