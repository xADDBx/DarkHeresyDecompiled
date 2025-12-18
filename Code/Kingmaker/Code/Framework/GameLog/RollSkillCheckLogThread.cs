using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Code.Framework.GameLog;

public class RollSkillCheckLogThread : BaseRollSkillCheckLogThread
{
	public override void HandleEvent(RulePerformSkillCheck check)
	{
		if (check.StatType != StatType.SkillAwareness || check.ResultIsSuccess || Game.Instance.Player.UISettings.ShowFailedPerceptionChecks || check.ShowAnyway)
		{
			base.HandleEvent(check);
		}
	}
}
