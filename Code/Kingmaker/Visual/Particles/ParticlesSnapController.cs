using System.Collections.Generic;
using Kingmaker.Visual.Particles.SnapController;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

[RequireComponent(typeof(ParticleSystem))]
public class ParticlesSnapController : SnapControllerBase
{
	[HideInInspector]
	[SerializeField]
	private bool m_useRandomBones;

	[HideInInspector]
	[SerializeField]
	private float m_randomBonesPercent = 1f;

	internal override BoneCollector.Result GetBones(SnapMapBase snapMap, HashSet<FxBone> fxBones)
	{
		BoneCollector boneCollector = new BoneCollector(snapMap, base.LocatorGroups, SnapType == ParticleSnapType.Transforms && !m_IsRotatableCopy, SnapType == ParticleSnapType.Transforms && m_IsRotatableCopy, Offset.WorldRotationBone, SnapType == ParticleSnapType.Transforms && m_useRandomBones, m_randomBonesPercent);
		return boneCollector.Collect(fxBones);
	}
}
