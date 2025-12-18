using System;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.ContextActions;

[Serializable]
[TypeId("49572af4a58c4f21a64433b34461101e")]
public sealed class ContextActionHandleSkillCheckResult : ContextAction
{
	public ActionList Success;

	public ActionList Failure;

	public override string GetCaption()
	{
		return "Handle skill check result";
	}

	protected override void RunAction()
	{
		if (((SimpleContextData<IRulebookEvent, MechanicsContext.Scope.Rule>.Current as RulePerformSkillCheck) ?? throw new InvalidOperationException("RulePerformSkillCheck not found")).ResultIsSuccess)
		{
			Success.Run();
		}
		else
		{
			Failure.Run();
		}
	}
}
