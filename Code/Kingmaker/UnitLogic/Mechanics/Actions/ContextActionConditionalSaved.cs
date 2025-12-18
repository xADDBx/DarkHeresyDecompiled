using System;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("d61949c589ee885458c9439b2aa202b6")]
public class ContextActionConditionalSaved : ContextAction
{
	public ActionList Succeed;

	public ActionList Failed;

	public override string GetCaption()
	{
		return "Conditional saved";
	}

	protected override void RunAction()
	{
		RulePerformSavingThrow rulePerformSavingThrow = ContextData<SavingThrowData>.Current?.Rule;
		if (rulePerformSavingThrow == null)
		{
			Element.LogError(this, "Can't use ContextActionConditionalSaved if no saving throw rolled");
		}
		else if (rulePerformSavingThrow.IsPassed)
		{
			Succeed.Run();
		}
		else
		{
			Failed.Run();
		}
	}
}
