using Owlcat.Runtime.Visual.XPBD.Solvers;
using Owlcat.Runtime.Visual.XPBD.Stats;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions.Broadphase;

public interface IBroadphaseImpl
{
	Solver Solver { get; }

	void SetCmd(CommandBuffer cmd);

	void CollisionDetection(float dt);

	void Dispose();

	MemoryStat GetMemoryStat();

	BroadphaseStats GetStats();

	int GetContactsCount();
}
