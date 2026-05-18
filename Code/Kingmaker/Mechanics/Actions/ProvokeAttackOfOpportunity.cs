using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("b8417df48902432b9b5a5b22e9f30a75")]
public class ProvokeAttackOfOpportunity : ContextAction
{
	public enum Type
	{
		TargetFromAnyone,
		TargetFromCaster,
		CasterFromAnyone,
		CasterFromTarget,
		TargetFromAlliesAdjacentToCaster,
		TargetFromAlliesAdjacentToTarget
	}

	public Type m_Type;

	[ShowIf("m_ShowConditions")]
	public ConditionsChecker ConditionsOnOpportunityAttacker;

	[ShowIf("m_ShowConditions")]
	public ActionList ActionsOnOpportunityAttacker;

	[UsedImplicitly]
	private bool m_ShowConditions
	{
		get
		{
			if (m_Type != Type.TargetFromAlliesAdjacentToCaster)
			{
				return m_Type == Type.TargetFromAlliesAdjacentToTarget;
			}
			return true;
		}
	}

	public override string GetCaption()
	{
		return $"Provoke attack of opportunity {m_Type}";
	}

	protected override void RunAction()
	{
		BaseUnitEntity casterUnit = base.Caster as BaseUnitEntity;
		BaseUnitEntity targetUnit = base.Target.Entity as BaseUnitEntity;
		AttackOfOpportunityController attackOfOpportunityController = Game.Instance.Controllers.AttackOfOpportunityController;
		switch (m_Type)
		{
		case Type.TargetFromAnyone:
			if (targetUnit != null)
			{
				attackOfOpportunityController.Provoke(targetUnit, base.Context.Blueprint as BlueprintFact);
			}
			break;
		case Type.TargetFromCaster:
			if (targetUnit != null && casterUnit != null)
			{
				attackOfOpportunityController.Provoke(targetUnit, casterUnit, base.Context.Blueprint as BlueprintFact);
			}
			break;
		case Type.CasterFromAnyone:
			if (casterUnit != null)
			{
				attackOfOpportunityController.Provoke(casterUnit, base.Context.Blueprint as BlueprintFact);
			}
			break;
		case Type.CasterFromTarget:
			if (targetUnit != null && casterUnit != null)
			{
				attackOfOpportunityController.Provoke(casterUnit, targetUnit, base.Context.Blueprint as BlueprintFact);
			}
			break;
		case Type.TargetFromAlliesAdjacentToCaster:
			if (targetUnit == null || casterUnit == null)
			{
				break;
			}
			{
				foreach (BaseUnitEntity item in from p in GameHelper.GetTargetsAround(casterUnit.Position, 1)
					where p != casterUnit && p.IsAlly(casterUnit) && targetUnit.GetEngagedByUnits().Contains(p)
					select p)
				{
					using (base.Context.PushTarget(item))
					{
						if (ConditionsOnOpportunityAttacker.Check())
						{
							attackOfOpportunityController.Provoke(targetUnit, item, base.Context.Blueprint as BlueprintFact);
							ActionsOnOpportunityAttacker.Run();
						}
					}
				}
				break;
			}
		case Type.TargetFromAlliesAdjacentToTarget:
			if (targetUnit == null || casterUnit == null)
			{
				break;
			}
			{
				foreach (BaseUnitEntity item2 in from p in GameHelper.GetTargetsAround(targetUnit.Position, 1)
					where p != targetUnit && p != casterUnit && p.IsAlly(casterUnit) && targetUnit.GetEngagedByUnits().Contains(p)
					select p)
				{
					using (base.Context.PushTarget(item2))
					{
						if (ConditionsOnOpportunityAttacker.Check())
						{
							attackOfOpportunityController.Provoke(targetUnit, item2, base.Context.Blueprint as BlueprintFact);
							ActionsOnOpportunityAttacker.Run();
						}
					}
				}
				break;
			}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
