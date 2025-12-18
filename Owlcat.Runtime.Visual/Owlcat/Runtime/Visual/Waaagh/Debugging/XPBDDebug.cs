using System;
using Owlcat.Runtime.Visual.XPBD;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class XPBDDebug
{
	public bool GizmosEnabled;

	public bool UseDepthBuffer;

	public XPBDGizmosParticlesMode DrawParticles;

	public DrawConstraintType DrawConstraints;

	public bool DrawVelocities;

	public bool DrawInertialForces;

	public bool DrawNormals;

	public bool DrawRestNormals;

	public bool DrawDeformedVertices;

	public bool DrawColliderAabb;

	public bool DrawSimplexAabb;

	public bool DrawColliderContacts;

	public bool DrawContactNormals;

	public bool DrawSimplexContacts;

	public bool DrawVisibleBodyAabbs;

	public bool UseOnlyGameCameraForCulling;

	public bool TickOnEveryFrame;

	public bool ShowStats;
}
