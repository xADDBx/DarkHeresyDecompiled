using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("13403af74a2bb8c45b7af11f21195046")]
public class RemoveBuffsFromGroup : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[SerializeField]
	private BpRef<BlueprintAbilityGroup>[] m_Groups;

	public override string GetCaption()
	{
		return $"Remove buffs from group list on {Target}";
	}

	protected override void RunAction()
	{
		AbstractUnitEntity abstractUnitEntity = Target?.GetValue();
		if (abstractUnitEntity == null || m_Groups == null || m_Groups.Length == 0)
		{
			return;
		}
		foreach (Buff item in abstractUnitEntity.Buffs.Enumerable.Where(MatchesAnyGroup).ToList())
		{
			item.Remove();
		}
	}

	private bool MatchesAnyGroup(Buff buff)
	{
		foreach (BlueprintAbilityGroup abilityGroup in buff.Blueprint.AbilityGroups)
		{
			BpRef<BlueprintAbilityGroup>[] groups = m_Groups;
			for (int i = 0; i < groups.Length; i++)
			{
				if (groups[i].Guid == abilityGroup.AssetGuid)
				{
					return true;
				}
			}
		}
		return false;
	}
}
