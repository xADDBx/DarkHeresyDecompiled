using Kingmaker.Controllers.Combat;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("dd93bef8b0189c74587b94b1e4df84f0")]
public class OverrideInitiativeBuff : UnitFactComponentDelegate
{
	[SerializeField]
	private float m_OverrideInitiative;

	private float? m_SavedInitiative { get; set; }

	protected override void OnActivateOrPostLoad()
	{
		PartUnitCombatState combatState = base.Owner.CombatState;
		m_SavedInitiative = combatState.OverrideInitiative;
		combatState.OverrideInitiative = m_OverrideInitiative;
	}

	protected override void OnDeactivate()
	{
		base.Owner.CombatState.OverrideInitiative = m_SavedInitiative;
	}
}
