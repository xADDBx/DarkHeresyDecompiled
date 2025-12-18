using Owlcat.Runtime.Visual.XPBD.SoA;
using Owlcat.Runtime.Visual.XPBD.Stats;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Debug;

public interface IGizmosImpl
{
	bool IsValid { get; }

	GraphicsBufferWrapper BodiesMapBuffer { get; }

	GraphicsBufferWrapper BodyVisibilityBuffer { get; }

	GraphicsBufferWrapper BodyAabbBuffer { get; }

	GraphicsBufferWrapper ParticlesMapBuffer { get; }

	GraphicsBufferWrapper ParticlePositionBuffer { get; }

	GraphicsBufferWrapper ParticleVelocityBuffer { get; }

	GraphicsBufferWrapper ParticleRadiusBuffer { get; }

	GraphicsBufferWrapper DistanceConstraintsMapBuffer { get; }

	GraphicsBufferWrapper BendConstraintsMapBuffer { get; }

	GraphicsBufferWrapper AngularConstraintsMapBuffer { get; }

	GraphicsBufferWrapper ConstraintsIndicesBuffer { get; }

	GraphicsBufferWrapper ConstraintsParameters0Buffer { get; }

	GraphicsBufferWrapper ConstraintsParameters1Buffer { get; }

	GraphicsBufferWrapper MeshVerticesBuffer { get; }

	GraphicsBufferWrapper DeformableGizmosVerticesBuffer { get; }

	GraphicsBufferWrapper CollidersMapBuffer { get; }

	GraphicsBufferWrapper ColliderAabbBuffer { get; }

	GraphicsBufferWrapper SimplexMapBuffer { get; }

	GraphicsBufferWrapper ContactsBuffer { get; }

	GraphicsBufferWrapper ColliderContactsIndirectArgsBuffer { get; }

	GraphicsBufferWrapper SimplexContactsIndirectArgsBuffer { get; }

	void PushDataToGpu(CommandBuffer cmd);

	void Dispose();

	MemoryStat GetMemoryStat();
}
