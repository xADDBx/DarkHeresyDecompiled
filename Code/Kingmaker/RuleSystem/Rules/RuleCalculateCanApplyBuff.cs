using System;
using JetBrains.Annotations;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace Kingmaker.RuleSystem.Rules;

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
		if (!CanApply)
		{
			return;
		}
		if (Context.MaybeCaster != null && Context.MaybeCaster != base.Initiator && Context.MaybeCaster.IsAttackingGreenNPC(base.Initiator))
		{
			CanApply = false;
		}
		else if ((bool)Immunity)
		{
			CanApply = false;
		}
		else if (base.Initiator.IsPlayerFaction && Blueprint.IsHardCrowdControl)
		{
			HardCrowdControlDurationLimit value = SettingsRoot.Difficulty.HardCrowdControlOnPartyMaxDurationRounds.GetValue();
			if (value != HardCrowdControlDurationLimit.Unlimited)
			{
				Rounds rounds = value.ToRounds();
				Duration = (Duration.Rounds.HasValue ? new Rounds(Math.Min(Duration.Rounds.Value.Value, rounds.Value)) : rounds);
			}
		}
	}
}
