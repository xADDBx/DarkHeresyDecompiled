using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Events/UnitDeathTrigger")]
[TypeId("61054535efad8f742a9423ddbbb7a21f")]
public class UnitDeathTrigger : UnitFactComponentDelegate, IUnitDeathHandler, ISubscriber
{
	public enum FactionType
	{
		Any,
		Ally,
		Enemy,
		Companions
	}

	public bool AnyRadius;

	[HideIf("AnyRadius")]
	public ContextValue RadiusInMeters;

	public FactionType Faction;

	public ActionList Actions;

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		DoAction(unitEntity);
	}

	private void DoAction(AbstractUnitEntity unit)
	{
		if (unit == base.Owner || unit == null)
		{
			return;
		}
		if (Faction != 0)
		{
			bool flag = base.Owner.CombatGroup.IsEnemy(unit);
			if ((flag && Faction == FactionType.Ally) || (!flag && Faction == FactionType.Enemy) || (Faction == FactionType.Companions && (!unit.IsPlayerFaction || !unit.IsInCompanionRoster())))
			{
				return;
			}
		}
		if (AnyRadius || base.Owner.DistanceTo(unit) <= (float)RadiusInMeters.Calculate(base.Context))
		{
			base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
		}
	}
}
