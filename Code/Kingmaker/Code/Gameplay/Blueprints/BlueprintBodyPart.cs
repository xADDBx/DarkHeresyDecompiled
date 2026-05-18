using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Framework.Mechanics;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Particles.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Blueprints;

[Serializable]
[OwlPackable(OwlPackableMode.NoGenerate)]
[TypeId("f5e70809c0e24e9db97fc2b2cc97b5af")]
public class BlueprintBodyPart : BlueprintScriptableObject
{
	public RestrictionCalculator Restrictions;

	public bool ReplaceableByCover;

	public LocalizedString Name;

	public LocalizedString Description;

	public Sprite Icon;

	[Range(0f, 100f)]
	public int HitChance;

	[EnumFlagsAsButtons]
	public BodyPartTags Tags;

	[InfoBox("Default tag - precise attacks to Default body parts produces auto hit (By Design)")]
	public BpRef<BlueprintBuff> CriticalEffect;

	public BpRef<ContextActionsList> ActionsOnPreciseAttackHit;

	public bool IsVital;

	[ShowIf("IsVital")]
	public int VitalDamageIncrease;

	public bool IgnoreArmorDamageReduction;

	public bool AlwaysCriticalHit;

	public int MeleePreciseHitChanceModifier;

	public int RangedPreciseHitChanceModifier;

	public List<BpRef<BlueprintFxLocatorGroup>> FxLocatorGroups;

	public int CriticalEffectStagesCount => CriticalEffect.MaybeBlueprint?.MaxRank ?? 0;

	public bool CanBeHitRandomly => HitChance > 0;
}
