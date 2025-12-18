using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[AllowMultipleComponents]
[TypeId("f2dd09e9b92aa574097a32a41b7e140e")]
public class StarshipDamageModifier : BlueprintComponent
{
	private enum TriggerType
	{
		AsInitiator,
		AsTarget
	}

	[SerializeField]
	private TriggerType triggerType;

	public bool CheckWeaponType;

	[ShowIf("CheckWeaponType")]
	public StarshipWeaponType WeaponType;

	[SerializeField]
	private int m_BonusDamage;

	[SerializeField]
	private float m_ExtraDamageMod;

	[SerializeField]
	private bool m_MultiplyByBuffRank;

	[SerializeField]
	[ShowIf("m_MultiplyByBuffRank")]
	private BlueprintBuffReference m_StackingBuff;

	public BlueprintBuff StackingBuff => m_StackingBuff?.Get();
}
