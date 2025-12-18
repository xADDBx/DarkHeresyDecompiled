using Kingmaker.Blueprints.Root.Strings;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageAttackOfOpportunity : CombatMessageBase
{
	public override string GetText()
	{
		return UIStrings.Instance.CombatTexts.AttackOfOpportunity;
	}
}
