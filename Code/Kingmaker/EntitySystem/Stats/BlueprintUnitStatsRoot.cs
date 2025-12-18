using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

[Serializable]
[TypeId("4cedbbc935e34cad942bcb2802af72f1")]
public sealed class BlueprintUnitStatsRoot : BlueprintScriptableObject
{
	[Serializable]
	public sealed class AttributeCategoryAdvance
	{
		[ArrayElementNameProvider]
		public AttributeType Attribute;

		public WeaponType RequiredWeapon;

		[Range(1f, 4f)]
		[SerializeField]
		private int _advance = 1;

		public int Advance => Math.Max(1, _advance);
	}

	[Serializable]
	public sealed class UnitDifficultyTypeAttributeCategoryAdvances
	{
		public static readonly UnitDifficultyTypeAttributeCategoryAdvances Empty = new UnitDifficultyTypeAttributeCategoryAdvances();

		[ArrayElementNameProvider]
		public UnitDifficultyType Difficulty;

		public AttributeCategoryAdvance[] Advances = new AttributeCategoryAdvance[0];
	}

	[Serializable]
	public sealed class UnitSubtypeAttributeCategoryAdvances
	{
		public static readonly UnitSubtypeAttributeCategoryAdvances Empty = new UnitSubtypeAttributeCategoryAdvances();

		[ArrayElementNameProvider]
		public UnitSubtype Subtype;

		public AttributeCategoryAdvance[] Advances = new AttributeCategoryAdvance[0];
	}

	public enum WeaponType
	{
		Any,
		Melee,
		Ranged
	}

	[Serializable]
	public sealed class UnitDifficultyTypeAttributeFactor
	{
		public UnitDifficultyType DifficultyType;

		public float Factor;
	}

	[Serializable]
	public sealed class UnitDifficultyTypeUnmodifiedWoundsBaseValue
	{
		public UnitDifficultyType DifficultyType;

		public int UnmodifiedBaseValue;
	}

	public int PrimaryAttributeBase = 45;

	public int DumpAttributeBase = 30;

	public int PrimaryPlusAttributeIncrease = 10;

	[InfoBox("Процентный модификатор для HP \"крепких\" юнитов (+/-X%)")]
	public int ToughUnitHitPointsModifier = 30;

	[InfoBox("Процентный модификатор для HP \"хрупких\" юнитов (+/-X%)")]
	public int FragileUnitHitPointsModifier = -30;

	[InfoBox("Процентный модификатор для брони \"танковых\" юнитов (+/-X%)")]
	public int TankUnitArmorFromHitPointsModifier = 20;

	[InfoBox("Процентный модификатор для брони \"хрупких\" юнитов (+/-X%)")]
	public int FragileUnitArmorFromHitPointsModifier = -58;

	[InfoBox("Процентный модификатор для брони \"обычных\" юнитов (+/-X%)")]
	public int DefaultUnitArmorFromHitPointsModifier = -40;

	public UnitSubtype[] ToughSubtypes = new UnitSubtype[2]
	{
		UnitSubtype.Defender,
		UnitSubtype.Persecutor
	};

	public UnitSubtype[] FragileSubtypes = new UnitSubtype[1] { UnitSubtype.Sniper };

	public UnitSubtype[] TankSubtypes = new UnitSubtype[1] { UnitSubtype.Defender };

	public UnitDifficultyTypeUnmodifiedWoundsBaseValue[] UnmodifiedWoundsBaseValues = new UnitDifficultyTypeUnmodifiedWoundsBaseValue[4]
	{
		new UnitDifficultyTypeUnmodifiedWoundsBaseValue
		{
			DifficultyType = UnitDifficultyType.Swarm,
			UnmodifiedBaseValue = 8
		},
		new UnitDifficultyTypeUnmodifiedWoundsBaseValue
		{
			DifficultyType = UnitDifficultyType.Common,
			UnmodifiedBaseValue = 33
		},
		new UnitDifficultyTypeUnmodifiedWoundsBaseValue
		{
			DifficultyType = UnitDifficultyType.Elite,
			UnmodifiedBaseValue = 56
		},
		new UnitDifficultyTypeUnmodifiedWoundsBaseValue
		{
			DifficultyType = UnitDifficultyType.Boss,
			UnmodifiedBaseValue = 130
		}
	};

	public AttributeCategoryAdvance[] DefaultAttributeAdvances = new AttributeCategoryAdvance[0];

	public UnitDifficultyTypeAttributeCategoryAdvances[] DifficultyTypeAttributeAdvances = new UnitDifficultyTypeAttributeCategoryAdvances[0];

	public UnitSubtypeAttributeCategoryAdvances[] SubtypeAttributeAdvances = new UnitSubtypeAttributeCategoryAdvances[0];

	public UnitDifficultyTypeAttributeFactor[] DifficultyTypeAttributeFactor = new UnitDifficultyTypeAttributeFactor[4]
	{
		new UnitDifficultyTypeAttributeFactor
		{
			DifficultyType = UnitDifficultyType.Swarm,
			Factor = 0.5f
		},
		new UnitDifficultyTypeAttributeFactor
		{
			DifficultyType = UnitDifficultyType.Common,
			Factor = 0.75f
		},
		new UnitDifficultyTypeAttributeFactor
		{
			DifficultyType = UnitDifficultyType.Elite,
			Factor = 1f
		},
		new UnitDifficultyTypeAttributeFactor
		{
			DifficultyType = UnitDifficultyType.Boss,
			Factor = 1.15f
		}
	};

	public UnitDifficultyTypeAttributeCategoryAdvances GetDifficultyTypeAdvances(UnitDifficultyType difficulty)
	{
		return DifficultyTypeAttributeAdvances.FirstItem((UnitDifficultyTypeAttributeCategoryAdvances a) => a.Difficulty == difficulty) ?? UnitDifficultyTypeAttributeCategoryAdvances.Empty;
	}

	public UnitSubtypeAttributeCategoryAdvances GetUnitSubtypeAdvances(UnitSubtype subtype)
	{
		return SubtypeAttributeAdvances.FirstItem((UnitSubtypeAttributeCategoryAdvances a) => a.Subtype == subtype) ?? UnitSubtypeAttributeCategoryAdvances.Empty;
	}

	public float GetDifficultyTypeAttributeFactor(UnitDifficultyType difficulty)
	{
		return DifficultyTypeAttributeFactor.FirstItem((UnitDifficultyTypeAttributeFactor a) => a.DifficultyType == difficulty)?.Factor ?? 1f;
	}

	public int GetUnmodifiedWoundsBaseValue(UnitDifficultyType difficulty)
	{
		return UnmodifiedWoundsBaseValues.FirstItem((UnitDifficultyTypeUnmodifiedWoundsBaseValue a) => a.DifficultyType == difficulty)?.UnmodifiedBaseValue ?? 1;
	}
}
