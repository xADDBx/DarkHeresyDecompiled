using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
internal struct CommandRecord
{
	public CommandCode code;

	public int dataIndex;
}
