using Kingmaker.Blueprints.Attributes;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[ComponentName("Ability/AddFactContextActions")]
[TypeId("25d172d2be8f52f468b2050d14d59806")]
public class AddFactContextActions : EntityFactComponentDelegate, ITickEachRound, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEntitySubscriber, IEventTag<ITurnEndHandler, EntitySubscriber>
{
	public bool DisableForLevelUpPreviewUnit;

	public bool DisableWhenReapplying;

	public ActionList Activated;

	public ActionList Deactivated;

	public ActionList NewRound;

	public ActionList RoundEnd;

	public override bool IsOverrideOnActivateMethod
	{
		get
		{
			ActionList deactivated = Deactivated;
			if (deactivated == null || !deactivated.HasActions)
			{
				return NewRound?.HasActions ?? false;
			}
			return true;
		}
	}

	private bool DisabledBecauseOfLevelUp
	{
		get
		{
			if (DisableForLevelUpPreviewUnit)
			{
				return base.Owner.GetOptional<PartPreviewUnit>() != null;
			}
			return false;
		}
	}

	private bool DisabledBecauseOfReapply
	{
		get
		{
			if (DisableWhenReapplying)
			{
				return base.IsReapplying;
			}
			return false;
		}
	}

	protected override void OnActivate()
	{
		if (!DisabledBecauseOfReapply && !DisabledBecauseOfLevelUp)
		{
			base.Fact.RunActionInContext(Activated);
		}
	}

	protected override void OnDeactivate()
	{
		if (!DisabledBecauseOfReapply && !DisabledBecauseOfLevelUp)
		{
			base.Fact.RunActionInContext(Deactivated);
		}
	}

	void ITickEachRound.OnNewRound()
	{
		if (!DisabledBecauseOfLevelUp)
		{
			base.Fact.RunActionInContext(NewRound);
		}
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		if (!DisabledBecauseOfLevelUp)
		{
			base.Fact.RunActionInContext(RoundEnd);
		}
	}
}
