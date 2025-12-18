using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("2c7551b19a204254aaf39e1766aa081c")]
public class ChangeAsksListOnUnit : PlayerUpgraderOnlyAction
{
	[SerializeField]
	private BlueprintUnitAsksListReference m_CurrentList;

	[SerializeField]
	private BlueprintUnitAsksListReference m_TargetList;

	[SerializeField]
	private BlueprintUnitReference m_TargetPartyUnit;

	public BlueprintUnit TargetPartyUnit => m_TargetPartyUnit;

	public BlueprintUnitAsksList CurrentList => m_CurrentList.Get();

	public BlueprintUnitAsksList TargetList => m_TargetList.Get();

	public override string GetCaption()
	{
		return $"Remove {CurrentList} on {TargetPartyUnit?.name} and add {TargetList}";
	}

	protected override void RunActionOverride()
	{
		foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
		{
			if (allUnit is BaseUnitEntity unit)
			{
				Upgrade(unit);
			}
		}
	}

	private void Upgrade(BaseUnitEntity unit)
	{
		if ((TargetPartyUnit == null || TargetPartyUnit == unit.Blueprint || TargetPartyUnit == unit.OriginalBlueprint) && unit.GetOptional<PartUnitAsks>()?.List != null && unit.GetOptional<PartUnitAsks>()?.List == CurrentList)
		{
			unit.GetOptional<PartUnitAsks>()?.SetOverride(TargetList);
		}
	}
}
