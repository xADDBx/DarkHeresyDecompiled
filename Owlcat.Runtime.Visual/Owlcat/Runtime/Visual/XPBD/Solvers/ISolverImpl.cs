using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.Debug;
using Owlcat.Runtime.Visual.XPBD.Stats;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Solvers;

public interface ISolverImpl
{
	Solver Solver { get; }

	IGizmosImpl GizmosImpl { get; }

	void Dispose();

	void EnsureRenderBuffersInitialized(ScriptableRenderContext context);

	void BeginStep(in UpdateContext context);

	void Step(in UpdateContext context);

	void EndStep(in UpdateContext updateContext);

	void UpdateSkin(SkinnedMeshBody body, GraphicsBuffer vertexBuffer, bool recalculateNormals);

	void UpdateBoneTranforms(SkeletonBody body, TransformAccessArray bones);

	void UpdateConstraintSettings(AuthoringBase authoring);

	void UpdateMeshBasePositions(MeshBody body);

	void GetBodyAabb(AuthoringBase body, out Aabb bodyAabb);

	MemoryStat GetMemoryStat();

	void UpdateBodySimulationParameters(AuthoringBase authoring);

	void UpdateLayer(AuthoringBase authoring);
}
