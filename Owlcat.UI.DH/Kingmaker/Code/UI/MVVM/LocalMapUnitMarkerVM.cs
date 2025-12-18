using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapUnitMarkerVM : LocalMapMarkerVM
{
	private readonly BaseUnitEntity m_Unit;

	public UnitGroupMemory.UnitInfo UnitInfo { get; }

	public LocalMapUnitMarkerVM(UnitGroupMemory.UnitInfo unitInfo)
	{
		UnitInfo = unitInfo;
		BaseUnitEntity baseUnitEntity = (m_Unit = unitInfo.Unit);
		bool num = unitInfo.Unit.GetOptional<PartAdditionalCombatObjectiveUnit>() != null;
		bool flag = TurnController.IsInTurnBasedCombat();
		if (num && flag)
		{
			base.MarkerType = LocalMapMarkType.AdditionalCombatObjective;
		}
		m_Position.Value = baseUnitEntity.Position;
		m_IsVisible.Value = true;
		m_Description.Value = baseUnitEntity.CharacterName;
		m_IsEnemy.Value = baseUnitEntity.Faction.IsPlayerEnemy;
	}

	protected override void OnUpdateHandler()
	{
		BaseUnitEntity unit = m_Unit;
		if (unit != null && !unit.IsDisposed)
		{
			m_Position.Value = m_Unit.Position;
		}
	}

	public override Entity GetEntity()
	{
		return m_Unit;
	}
}
