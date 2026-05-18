using System;
using Kingmaker.ResourceLinks;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[Serializable]
public struct WeightedAnimationVariant
{
	[SerializeField]
	[Range(0f, 10f)]
	private float m_Weight;

	[SerializeField]
	private AnimationClipWrapperSwitcherLink m_Switcher;

	public float Weight => m_Weight;

	public AnimationClipWrapperSwitcher Switcher => m_Switcher?.Load();
}
