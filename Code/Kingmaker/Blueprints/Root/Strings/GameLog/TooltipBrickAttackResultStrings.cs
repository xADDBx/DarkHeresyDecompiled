using System;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Blueprints.Root.Strings.GameLog;

[Serializable]
public class TooltipBrickAttackResultStrings
{
	public LocalizedString AttackResultUnknown;

	public LocalizedString AttackResultHit;

	public LocalizedString AttackResultCoverHit;

	public LocalizedString AttackResultMiss;

	public LocalizedString AttackResultDefended;

	public string GetAttackResultText(AttackResult result)
	{
		return result switch
		{
			AttackResult.Unknown => AttackResultUnknown, 
			AttackResult.Hit => AttackResultHit, 
			AttackResult.CoverHit => AttackResultCoverHit, 
			AttackResult.Miss => AttackResultMiss, 
			AttackResult.Defended => AttackResultDefended, 
			_ => AttackResultUnknown, 
		};
	}
}
