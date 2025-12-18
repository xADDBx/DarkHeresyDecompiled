using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[ComponentName("Custom/AllyParry")]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("a789b4af17454621b49ff64b506401a2")]
public class AllyParry : BlueprintComponent
{
	[SerializeField]
	private bool m_ParryMelee;

	[SerializeField]
	private bool m_ParryRanged;

	public bool ParryMelee => m_ParryMelee;

	public bool ParryRanged => m_ParryRanged;
}
