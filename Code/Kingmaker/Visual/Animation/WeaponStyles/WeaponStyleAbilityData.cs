using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleAbilityData : IWeaponStyleAnimationClipsProvider
{
	public enum VariationType
	{
		Self
	}

	[Serializable]
	public class VariationSettings
	{
		public VariationType Type;

		public AnimationClipWrapper Clip;
	}

	[Serializable]
	public class WeaponStyleAbilityDataEntry
	{
		public AbilityAnimationStyle Style;

		public AnimationClipWrapper Clip;

		[CanBeNull]
		[SerializeField]
		private List<VariationSettings> m_Variations;

		public AnimationClipWrapper GetVariation(VariationType variationType)
		{
			return m_Variations?.SingleOrDefault((VariationSettings x) => x.Type == variationType)?.Clip.Or(null) ?? Clip;
		}

		public void SetVariation(VariationType variationType, AnimationClipWrapper clip)
		{
			if (m_Variations == null)
			{
				m_Variations = new List<VariationSettings>();
			}
			m_Variations.RemoveAll((VariationSettings v) => v == null || v.Clip == null);
			VariationSettings variationSettings = m_Variations.FirstOrDefault((VariationSettings v) => v.Type == variationType);
			if (variationSettings != null)
			{
				variationSettings.Clip = clip;
				return;
			}
			m_Variations.Add(new VariationSettings
			{
				Type = variationType,
				Clip = clip
			});
		}

		public IEnumerable<AnimationClipWrapper> EnumerateClips()
		{
			yield return Clip;
			if (m_Variations == null)
			{
				yield break;
			}
			foreach (VariationSettings variation in m_Variations)
			{
				yield return variation.Clip;
			}
		}
	}

	public List<WeaponStyleAbilityDataEntry> Clips;

	public AnimationClipWrapper this[AbilityAnimationStyle style] => Clips.SingleOrDefault((WeaponStyleAbilityDataEntry x) => x.Style == style)?.Clip;

	public AnimationClipWrapper this[AbilityAnimationStyle style, VariationType type] => Clips.SingleOrDefault((WeaponStyleAbilityDataEntry x) => x.Style == style)?.GetVariation(type);

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		return Clips.SelectMany((WeaponStyleAbilityDataEntry x) => x.EnumerateClips());
	}
}
