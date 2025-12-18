using Unity.Burst;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.OcclusionClipping;

[BurstCompile]
internal struct ReplayActionsJob : IJob
{
	public State State;

	public ActionBuffer ActionBuffer;

	public void Execute()
	{
		ActionBuffer.Replay(ref State);
	}
}
