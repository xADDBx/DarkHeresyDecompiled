using System;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;

[Serializable]
public class SlaveVertex
{
	public int SlaveIndex;

	public int MasterTriangleIndex;

	public BarycentricPoint Position;

	public BarycentricPoint Normal;

	public BarycentricPoint Tangent;

	public int3 MasterVertexIndices;

	public static SlaveVertex Empty => new SlaveVertex(-1, -1, -1, BarycentricPoint.Zero, BarycentricPoint.Zero, BarycentricPoint.Zero);

	public bool IsEmpty
	{
		get
		{
			if (SlaveIndex >= 0)
			{
				return MasterTriangleIndex < 0;
			}
			return true;
		}
	}

	public SlaveVertex(int slaveIndex, int masterTriangleIndex, int3 masterVertexIndices, BarycentricPoint position, BarycentricPoint normal, BarycentricPoint tangent)
	{
		SlaveIndex = slaveIndex;
		MasterTriangleIndex = masterTriangleIndex;
		MasterVertexIndices = masterVertexIndices;
		Position = position;
		Normal = normal;
		Tangent = tangent;
	}
}
