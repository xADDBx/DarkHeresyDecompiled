using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD;

[Serializable]
public class CollisionSettings
{
	[SerializeField]
	[HideInInspector]
	private bool m_ParticleCollisionsEnabled;

	public bool ColliderCollisionsEnabled;

	[Tooltip("Amount of iterations spent on convex optimization for surface collisions.")]
	[Range(1f, 32f)]
	public int OptimizationIterations = 8;

	[Tooltip("Error threshold at which to stop convex optimization for surface collisions.")]
	public float SurfaceCollisionTolerance = 0.005f;

	[Tooltip("Maximum depenetration velocity applied to particles that start a frame inside an object. Low values ensure no 'explosive' collision resolution. Should be > 0 unless looking for non-physical effects.")]
	public float MaxDepenetration = 10f;

	[Range(0f, 1f)]
	[Tooltip("Collider continuous collision detection. Set 0 for purely static collisions, set 1 for pure continuous collisions.")]
	public float ColliderCCD;

	[Range(0f, 1f)]
	[Tooltip("Particle continuous collision detection. Set 0 for purely static collisions, set 1 for pure continuous collisions.")]
	public float ParticleCCD;

	public float CollisionMargin = 0.02f;

	[Range(16f, 65536f)]
	public int MaxContactsCount = 1024;

	public bool ParticleCollisionsEnabled
	{
		get
		{
			return false;
		}
		set
		{
			m_ParticleCollisionsEnabled = value;
		}
	}
}
