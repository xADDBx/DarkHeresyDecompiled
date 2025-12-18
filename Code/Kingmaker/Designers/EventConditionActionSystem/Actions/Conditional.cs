using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Conditional")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("52d8973f2e470e14c97b74209680491a")]
public class Conditional : GameAction
{
	public string Comment;

	public ConditionsChecker ConditionsChecker;

	public ActionList IfTrue;

	public ActionList IfFalse;

	public override string GetDescription()
	{
		return "Позволяет добавить условия в последовательность действий";
	}

	protected override void RunAction()
	{
		(ConditionsChecker.Check(null, @unsafe: true) ? IfTrue : IfFalse).Run(ActionList.ExceptionHandlingMode.ThrowImmediately);
	}

	public override string GetCaption()
	{
		return "Conditional (" + Comment + " )";
	}
}
