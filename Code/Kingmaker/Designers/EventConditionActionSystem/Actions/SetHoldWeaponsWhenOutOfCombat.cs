using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("5dc3d9a8a70efdc438b56bf77b472e86")]
public class SetHoldWeaponsWhenOutOfCombat : GameAction
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private bool m_HoldWeaponsWhenOutOfCombat = true;

	public override string GetCaption()
	{
		if (!m_HoldWeaponsWhenOutOfCombat)
		{
			return $"Make {m_Unit} stop holding weapons in out of combat";
		}
		return $"Make {m_Unit} hold weapons in out of combat";
	}

	protected override void RunAction()
	{
		AbstractUnitEntity abstractUnitEntity = m_Unit?.GetValue();
		if (abstractUnitEntity != null)
		{
			abstractUnitEntity.GetOrCreate<UnitPartVisualChange>().HoldWeaponsWhenOutOfCombat = m_HoldWeaponsWhenOutOfCombat;
		}
	}
}
