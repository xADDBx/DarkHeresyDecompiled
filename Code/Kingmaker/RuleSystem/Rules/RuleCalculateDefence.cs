using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateDefence : RulebookTargetEvent<MechanicEntity, MechanicEntity>
{
	public readonly CompositeModifiersManager DefenceValueModifiers = new CompositeModifiersManager(0);

	public int ResultDefence { get; private set; }

	public int BaseDefence { get; private set; }

	public RuleCalculateDefence([CanBeNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target)
		: base(initiator, target)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		BaseDefence = base.Target.GetStatOptional(StatType.Defence);
		ResultDefence = DefenceValueModifiers.Apply(BaseDefence);
	}
}
