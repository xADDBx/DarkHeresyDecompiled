using System;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.HitSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker;

public class SurfaceHitObject : MonoBehaviour
{
	[Serializable]
	public class ColliderData
	{
		public Collider Collider;

		public string ColliderMeshName;
	}

	public const SurfaceType DefaultSoundSurfaceType = SurfaceType.MetalEnv;

	[FormerlySerializedAs("_SoundSurfaceType")]
	public SurfaceType m_SoundSurfaceType = SurfaceType.MetalEnv;

	[FormerlySerializedAs("_Colliders")]
	[InspectorReadOnly]
	[Obsolete]
	public Collider[] m_Colliders;

	public List<ColliderData> m_CollidersData = new List<ColliderData>();
}
