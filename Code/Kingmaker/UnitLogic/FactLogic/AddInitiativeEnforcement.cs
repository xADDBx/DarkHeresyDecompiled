using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Combat/AddInitiativeEnforcement")]
[TypeId("69b15731226e4e07a206170bbe9c09c4")]
public class AddInitiativeEnforcement : UnitFactComponentDelegate, IAnyUnitCombatHandler, ISubscriber, IUnitDeathHandler
{
	[InfoBox("Используется для разрешения конфликтов, если юниты претендуют на одну позицию. Выше значение - ниже приоритет")]
	[SerializeField]
	[Range(1f, 20f)]
	public int InnerPriority = 10;

	[SerializeField]
	private bool FollowUnit;

	[SerializeReference]
	[ShowIf("FollowUnit")]
	public AbstractUnitEvaluator UnitToFollow;

	[InfoBox("Процентное место в очереди инициативы. 0 - первый. 100 - последний")]
	[SerializeField]
	[HideIf("FollowUnit")]
	[Range(0f, 100f)]
	public int InitiativePositionPercent;

	[InfoBox("Старается поддерживать желаемое местов в инициативе не смотря ни на что : смерть других, новых юнитов в бою, любые перерасчеты инициативы")]
	[SerializeField]
	public bool Persist;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Initiative.Overrides.Add(new InitiativeOverride
		{
			InnerPriority = InnerPriority,
			PercentPosition = (FollowUnit ? (-1) : InitiativePositionPercent),
			UnitEvaluator = (FollowUnit ? UnitToFollow : null),
			Source = base.Fact.Blueprint.Reference(),
			Persistent = Persist
		});
		base.Owner.Initiative.Clear();
		EventBus.RaiseEvent(delegate(IInitiativeChangeHandler h)
		{
			h.HandleInitiativeChanged();
		});
	}

	protected override void OnDeactivate()
	{
		InitiativeOverride initiativeOverride = base.Owner.Initiative.Overrides.Find((InitiativeOverride o) => o.Source == base.Fact.Blueprint);
		if (initiativeOverride != null)
		{
			base.Owner.Initiative.Overrides.Remove(initiativeOverride);
		}
		base.Owner.Initiative.Clear();
	}

	public void HandleUnitJoinCombat(BaseUnitEntity unit)
	{
		if (Persist && unit != base.Owner && base.Owner.IsInCombat && base.Owner.Initiative.Override.Source == base.Fact.Blueprint)
		{
			base.Owner.Initiative.Clear();
		}
	}

	public void HandleUnitLeaveCombat(BaseUnitEntity unit)
	{
		if (Persist && unit != base.Owner && base.Owner.IsInCombat && base.Owner.Initiative.Override.Source == base.Fact.Blueprint)
		{
			base.Owner.Initiative.Clear();
		}
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (Persist && unitEntity != base.Owner && base.Owner.IsInCombat && base.Owner.Initiative.Override.Source == base.Fact.Blueprint)
		{
			base.Owner.Initiative.Clear();
		}
	}
}
