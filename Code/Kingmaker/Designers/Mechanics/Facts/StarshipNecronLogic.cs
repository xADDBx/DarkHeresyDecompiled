using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintStarship))]
[TypeId("73cdb42c2d277a446be7489e2676e7ad")]
public class StarshipNecronLogic : BlueprintComponent
{
	[SerializeField]
	private BlueprintBuffReference m_LordBuff;

	[SerializeField]
	private BlueprintBuffReference m_MarkBuff;

	[SerializeField]
	private int m_LordSpawnPrefferedDistance;

	[SerializeField]
	private int m_LordEscapeMaxDistance;

	[SerializeField]
	private int m_RegenPerTurnPct;

	[SerializeField]
	private BlueprintBuffReference m_UndeathBuff;

	[SerializeField]
	private int m_UndeathDamagePerTurnPct;

	[SerializeField]
	private ActionList ActionsOnUndeathEvent;

	public BlueprintBuff LordBuff => m_LordBuff?.Get();

	public BlueprintBuff MarkBuff => m_MarkBuff?.Get();

	public int LordEscapeMaxDistance => m_LordEscapeMaxDistance;

	public BlueprintBuff UndeathBuff => m_UndeathBuff?.Get();

	public int UndeathDamagePerTurnPct => m_UndeathDamagePerTurnPct;
}
