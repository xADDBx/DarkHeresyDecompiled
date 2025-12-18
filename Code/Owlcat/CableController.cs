using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

namespace Owlcat;

public class CableController : MonoBehaviour
{
	public SplinePointsHolder[] m_Holders;

	public SplinePointsBoneBinder[] m_BoneBinders;

	public void Attach(Transform skeletonRoot)
	{
		SplinePointsBoneBinder[] boneBinders = m_BoneBinders;
		for (int i = 0; i < boneBinders.Length; i++)
		{
			boneBinders[i].Attach(skeletonRoot);
		}
	}

	public void Attach(CharacterBonesList characterBonesList)
	{
		SplinePointsBoneBinder[] boneBinders = m_BoneBinders;
		foreach (SplinePointsBoneBinder splinePointsBoneBinder in boneBinders)
		{
			splinePointsBoneBinder.AttachToTransform(characterBonesList.GetByName(splinePointsBoneBinder.m_Bone.BoneName));
		}
	}
}
