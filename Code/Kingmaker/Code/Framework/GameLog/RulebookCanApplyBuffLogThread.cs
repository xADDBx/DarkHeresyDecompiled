using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.UI;

namespace Kingmaker.Code.Framework.GameLog;

public class RulebookCanApplyBuffLogThread : LogThreadBase, IGameLogRuleHandler<RuleCalculateCanApplyBuff>
{
	public void HandleEvent(RuleCalculateCanApplyBuff rule)
	{
		CombatLogMessage combatLogMessage = GetCombatLogMessage(rule);
		if (combatLogMessage != null)
		{
			AddMessage(combatLogMessage);
		}
	}

	public static CombatLogMessage GetCombatLogMessage(RuleCalculateCanApplyBuff rule)
	{
		if (rule.ConcreteInitiator.IsDisposed || rule.ConcreteInitiator.IsDead)
		{
			return null;
		}
		if (rule.AppliedBuff.Blueprint.HideInLog || rule.Blueprint.IsHiddenInUI || ShowInLogOnlyOnYourself(rule))
		{
			return null;
		}
		MechanicEntity concreteInitiator = rule.ConcreteInitiator;
		GameLogContext.TargetEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)concreteInitiator;
		GameLogContext.Text = rule.Context.Name;
		GameLogMessage gameLogMessage = GetGameLogMessage(rule);
		bool flag = rule.AppliedBuff.IsProne && rule.AppliedBuff.Context.MaybeCaster != rule.Reason.Caster;
		TooltipBaseTemplate tooltipTemplate = CombatLogTooltipService.CreateTooltipTemplateBuff(new TooltipTemplateBuffArgs(rule.AppliedBuff, flag ? rule.Reason.Caster : null));
		return gameLogMessage?.CreateCombatLogMessage(tooltipTemplate, null, isPerformAttackMessage: false, concreteInitiator);
	}

	private static GameLogMessage GetGameLogMessage(RuleCalculateCanApplyBuff rule)
	{
		if ((bool)rule.Immunity)
		{
			return LogThreadBase.Strings.SpellImmunity;
		}
		if (rule.AppliedBuff.IsAttached)
		{
			return LogThreadBase.Strings.StatusEffect;
		}
		return null;
	}

	private static bool ShowInLogOnlyOnYourself(RuleCalculateCanApplyBuff rule)
	{
		MechanicsContext maybeContext = rule.AppliedBuff.MaybeContext;
		if (rule.Blueprint.ShowInLogOnlyOnYourself && maybeContext != null && maybeContext.MaybeOwner != null && maybeContext.MaybeCaster != null)
		{
			return maybeContext.MaybeOwner != maybeContext.MaybeCaster;
		}
		return false;
	}
}
