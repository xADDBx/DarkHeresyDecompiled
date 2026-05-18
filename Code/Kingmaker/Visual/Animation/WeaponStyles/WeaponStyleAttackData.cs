using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Visual.Animation.WeaponStyles;

[Serializable]
public class WeaponStyleAttackData : IWeaponStyleAnimationClipsProvider
{
	[Serializable]
	public class AttackDataContainer
	{
		[Serializable]
		public class AttackAbilityTypeDataPair
		{
			public AttackAnimationType Type;

			public AttackAnimationData Data;
		}

		[SerializeField]
		[ProvideNameWithProperty("Type")]
		private List<AttackAbilityTypeDataPair> m_Attacks;

		public AttackAnimationData this[AttackAnimationType type] => m_Attacks.SingleOrDefault((AttackAbilityTypeDataPair x) => x.Type == type)?.Data;

		public IEnumerable<AnimationClipWrapper> CollectClipWrappers()
		{
			foreach (AttackAnimationData data in m_Attacks.Select((AttackAbilityTypeDataPair p) => p.Data))
			{
				yield return data.In;
				yield return data.Out;
				foreach (AnimationClipWrapper clip in data.Clips)
				{
					yield return clip;
				}
			}
		}

		public void InitializeAttacks()
		{
			if (m_Attacks == null)
			{
				m_Attacks = new List<AttackAbilityTypeDataPair>();
			}
		}

		public AttackAbilityTypeDataPair GetOrCreateAttackPair(AttackAnimationType type)
		{
			InitializeAttacks();
			AttackAbilityTypeDataPair attackAbilityTypeDataPair = m_Attacks.SingleOrDefault((AttackAbilityTypeDataPair p) => p.Type == type);
			if (attackAbilityTypeDataPair == null)
			{
				attackAbilityTypeDataPair = new AttackAbilityTypeDataPair
				{
					Type = type,
					Data = new AttackAnimationData
					{
						Clips = new List<AnimationClipWrapper>()
					}
				};
				m_Attacks.Add(attackAbilityTypeDataPair);
			}
			return attackAbilityTypeDataPair;
		}
	}

	[Serializable]
	public class WeaponTypeData : AttackDataContainer
	{
		public WeaponType WeaponType;
	}

	[Serializable]
	public class MechadendriteAttackData : AttackDataContainer
	{
		public MechadendriteAttackAnimationType MechadendriteAttackType;
	}

	[SerializeField]
	private WeaponAnimationStyle m_WeaponStyle;

	[SerializeField]
	[ProvideNameWithProperty("WeaponType")]
	private List<WeaponTypeData> m_Main;

	[SerializeField]
	[ProvideNameWithProperty("WeaponType")]
	[ShowIf("m_IsDualWielding")]
	private List<WeaponTypeData> m_Off;

	[SerializeField]
	private bool m_HasMechadendrite;

	[SerializeField]
	[ProvideNameWithProperty("MechadendriteAttackType")]
	[ShowIf("m_HasMechadendrite")]
	private List<MechadendriteAttackData> m_Mechadendrite;

	private bool m_IsDualWielding => m_WeaponStyle.IsDualWielding();

	public void SetWeaponStyle(WeaponAnimationStyle style)
	{
		m_WeaponStyle = style;
	}

	public WeaponTypeData GetOrCreateWeaponTypeData(WeaponType weaponType, bool isMainHand)
	{
		List<WeaponTypeData> list = (isMainHand ? m_Main : m_Off);
		if (list == null)
		{
			list = new List<WeaponTypeData>();
			if (isMainHand)
			{
				m_Main = list;
			}
			else
			{
				m_Off = list;
			}
		}
		WeaponTypeData weaponTypeData = list.SingleOrDefault((WeaponTypeData w) => w.WeaponType == weaponType);
		if (weaponTypeData == null)
		{
			weaponTypeData = new WeaponTypeData
			{
				WeaponType = weaponType
			};
			weaponTypeData.InitializeAttacks();
			list.Add(weaponTypeData);
		}
		return weaponTypeData;
	}

	public AttackAnimationData GetMainHandAttackData(WeaponType weaponType, AttackAnimationType attackType)
	{
		return (m_Main?.SingleOrDefault((WeaponTypeData x) => x.WeaponType == weaponType))?[attackType];
	}

	public AttackAnimationData GetOffHandAttackData(WeaponType weaponType, AttackAnimationType attackType)
	{
		return (m_Off?.SingleOrDefault((WeaponTypeData x) => x.WeaponType == weaponType))?[attackType];
	}

	public AttackAnimationData GetMechadendriteAttackData(WeaponType weaponType, AttackAnimationType attackType)
	{
		MechadendriteAttackAnimationType mechadendriteAttackType = weaponType.GetMechadendriteAttackAnimationType();
		return (m_Mechadendrite?.SingleOrDefault((MechadendriteAttackData x) => x.MechadendriteAttackType == mechadendriteAttackType))?[attackType];
	}

	public IEnumerable<AnimationClipWrapper> EnumerateClips()
	{
		List<AnimationClipWrapper> list = new List<AnimationClipWrapper>();
		if (m_Main != null)
		{
			list.AddRange(m_Main.SelectMany((WeaponTypeData data) => data.CollectClipWrappers()));
		}
		if (m_Off != null)
		{
			list.AddRange(m_Off.SelectMany((WeaponTypeData data) => data.CollectClipWrappers()));
		}
		if (m_HasMechadendrite && m_Mechadendrite != null)
		{
			list.AddRange(m_Mechadendrite.SelectMany((MechadendriteAttackData data) => data.CollectClipWrappers()));
		}
		return list;
	}
}
