using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.HitSystem;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class SoundSurfaceObject : MonoBehaviour
{
	public SurfaceType Switch;

	public bool UseAllColliders;

	[Space]
	public bool UseBoxColliderInsteadOfMesh;

	[ShowIf("UseBoxColliderInsteadOfMesh")]
	public Vector3 ExtendBoxCollider = new Vector3(0.7f, 0f, 0f);
}
