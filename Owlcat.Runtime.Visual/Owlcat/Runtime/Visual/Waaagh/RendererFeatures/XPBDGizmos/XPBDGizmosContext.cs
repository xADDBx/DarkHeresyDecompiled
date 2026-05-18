using Owlcat.Runtime.Visual.Waaagh.Debugging;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.XPBDGizmos;

internal struct XPBDGizmosContext
{
	public XPBDDebug XPBDDebug;

	public Material GizmosMaterial;

	public int ParticlesRadiusPass;

	public int ParticlesPass;

	public int ConstraintsPass;

	public int VelocitiesPass;

	public int InertialForcesPass;

	public int AabbPass;

	public int ColliderContactsPass;

	public int ContactNormalsPass;

	public int NormalsPass;

	public int DrawDeformedVerticesPass;

	public int DrawSimplexContactsPass;
}
