using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Animation;

[CreateAssetMenu(fileName = "RandomWrapperWithWeightSwitcher", menuName = "Animation/Wrapper Switcher/Random Wrapper With Weight Switcher")]
public class RandomWrapperWithWeightSwitcher : AnimationClipWrapperSwitcher
{
	private static Dictionary<GameObject, int> m_LastUsed = new Dictionary<GameObject, int>();

	[SerializeField]
	private List<WeightedAnimationVariant> m_Variants = new List<WeightedAnimationVariant>();

	[SerializeField]
	private bool m_TryToNotRepeat;

	public override AnimationClipWrapper GetWrapper(IAnimationManager animationManager)
	{
		if (m_Variants == null || m_Variants.Count == 0)
		{
			return null;
		}
		GameObject gameObject = animationManager.GameObject;
		if (m_Variants.Count == 1)
		{
			return m_Variants[0].Switcher.GetWrapper(animationManager);
		}
		float num = 0f;
		foreach (WeightedAnimationVariant variant in m_Variants)
		{
			num += variant.Weight;
		}
		float num2 = animationManager.StatefulRandom.Range(0f, num);
		float num3 = 0f;
		if (m_TryToNotRepeat && gameObject != null)
		{
			int valueOrDefault = m_LastUsed.GetValueOrDefault(gameObject, -1);
			for (int i = 0; i < m_Variants.Count; i++)
			{
				num3 += m_Variants[i].Weight;
				if (valueOrDefault != i && num2 <= num3)
				{
					m_LastUsed[gameObject] = i;
					return m_Variants[i].Switcher.GetWrapper(animationManager);
				}
			}
			m_LastUsed[gameObject] = 0;
			return m_Variants[0].Switcher.GetWrapper(animationManager);
		}
		for (int j = 0; j < m_Variants.Count; j++)
		{
			num3 += m_Variants[j].Weight;
			if (num2 <= num3)
			{
				return m_Variants[j].Switcher.GetWrapper(animationManager);
			}
		}
		return m_Variants[0].Switcher.GetWrapper(animationManager);
	}

	public override IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		foreach (WeightedAnimationVariant variant in m_Variants)
		{
			IEnumerable<AnimationClipWrapper> enumerable = variant.Switcher.EnumerateClips();
			foreach (AnimationClipWrapper item in enumerable)
			{
				yield return item;
			}
		}
	}
}
