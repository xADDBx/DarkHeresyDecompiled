namespace Owlcat.Runtime.Visual.VirtualTexture.Diagnostics;

public struct MaterialRecord
{
	public int InstanceId;

	public bool MaterialAlive;

	public int RegisteredCount;

	public int[] RegisteredIndices;

	public float[] ActualMatrix16;

	public int[] ShaderDeclaredLayerCounts;

	public int[] ShaderDeclaredLayerMasks;

	public string ShaderName;
}
