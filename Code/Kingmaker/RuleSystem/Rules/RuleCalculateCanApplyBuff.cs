using JetBrains.Annotations;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.RuleSystem.Rules;

[RuleRoles(Initiator = "buff applier", Target = "buff target")]
public class RuleCalculateCanApplyBuff : RulebookEvent
{
	public readonly MechanicsContext Context;

	public readonly FlagModifiersManager Immunity = new FlagModifiersManager();

	public BuffDuration Duration { get; private set; }

	public bool CanApply { get; set; }

	public Buff AppliedBuff { get; }

	public BlueprintBuff Blueprint => AppliedBuff.Blueprint;

	public RuleCalculateCanApplyBuff([NotNull] MechanicEntity initiator, MechanicsContext context, Buff buff)
		: base(initiator)
	{
		AppliedBuff = buff;
		Context = context;
		Duration = new BuffDuration(buff);
		CanApply = base.Initiator is BaseUnitEntity;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (CanApply)
		{
			if (Context.MaybeCaster != null && Context.MaybeCaster != base.Initiator && Context.MaybeCaster.IsAttackingGreenNPC(base.Initiator))
			{
				CanApply = false;
			}
			else if ((bool)Immunity)
			{
				CanApply = false;
			}
		}
	}
}
