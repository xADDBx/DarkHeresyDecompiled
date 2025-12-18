using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("896373aaa0365ae40ad5420e293fad33")]
public class IgnoreGroupCooldownByBuff : BlueprintComponent
{
	[SerializeField]
	public BlueprintAbilityGroupReference m_IgnoreGroup;

	[SerializeField]
	private BlueprintBuffReference m_IgnoreBuff;

	public BlueprintAbilityGroup IgnoreGroup => m_IgnoreGroup?.Get();

	public BlueprintBuff IgnoreBuff => m_IgnoreBuff?.Get();
}
