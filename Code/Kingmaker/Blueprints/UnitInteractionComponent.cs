using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("e090ed34b87f41e4e826b4d6dff90462")]
public abstract class UnitInteractionComponent : EntityFactComponentDelegate<AbstractUnitEntity>, IUnitInteraction
{
	[SerializeField]
	private int m_OverrideDistance;

	public ConditionsChecker Conditions;

	public bool AllowInCombat;

	public bool TriggerOnApproach;

	[ShowIf("TriggerOnApproach")]
	public bool TriggerOnParty = true;

	[ShowIf("TriggerOnApproach")]
	public float Cooldown = 5f;

	public int Distance
	{
		get
		{
			if (m_OverrideDistance != 0)
			{
				return m_OverrideDistance;
			}
			return 2;
		}
	}

	public bool IsApproach => TriggerOnApproach;

	public float ApproachCooldown => Cooldown;

	public bool MainPlayerPreferred => true;

	public abstract bool IsDialog { get; }

	bool IUnitInteraction.AllowInCombat => AllowInCombat;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartUnitInteractions>().AddInteraction(this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartUnitInteractions>()?.RemoveInteraction(this);
	}

	public virtual bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target)
	{
		if (target.IsHelpless)
		{
			return false;
		}
		if (TriggerOnApproach && TriggerOnParty)
		{
			if (!initiator.IsDirectlyControllable)
			{
				return false;
			}
			if (initiator.IsPet)
			{
				return false;
			}
			if (initiator.GetOptional<UnitPartSummonedMonster>() != null)
			{
				return false;
			}
		}
		if (!Conditions.HasConditions)
		{
			return true;
		}
		using (ContextData<InteractingUnitData>.Request().Setup(initiator))
		{
			using (ContextData<ClickedUnitData>.Request().Setup(target))
			{
				return Conditions.Check();
			}
		}
	}

	public abstract AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target);
}
