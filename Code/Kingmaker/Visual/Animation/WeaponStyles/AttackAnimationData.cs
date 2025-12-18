using System;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class AttackAnimationData
{
	[Flags]
	public enum AttackAnimationTag
	{
		WithPreparation = 1,
		ComboSequence = 2
	}

	[SerializeField]
	[EnumFlagsAsButtons]
	private AttackAnimationTag m_Flags;

	[ShowIf("IsWithPreparation")]
	public AnimationClipWrapper In;

	[ShowIf("IsWithPreparation")]
	public AnimationClipWrapper Out;

	public List<AnimationClipWrapper> Clips;

	public bool IsWithPreparation => (m_Flags & AttackAnimationTag.WithPreparation) != 0;

	public bool IsComboSequence => (m_Flags & AttackAnimationTag.ComboSequence) != 0;

	public void SetFlags(AttackAnimationTag flags)
	{
		m_Flags = flags;
	}
}
