using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
internal struct ProcesuralSubMesh
{
	public byte materialId;

	public int indexStart;

	public int indexCount;
}
