using Kingmaker.Designers.EventConditionActionSystem.Evaluators.BarkBanters;
using Kingmaker.ElementsSystem;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f6f3fe7bd2554b46892dc65cfd90a62e")]
public class FoundDetectiveTrace : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public DetectiveTraceEvaluator DetectiveTraceView;

	public override string GetCaption()
	{
		return "Founds Detective Trace " + DetectiveTraceView?.GetCaption();
	}

	protected override void RunAction()
	{
		if (DetectiveTraceView.DetectiveTrace.FindData() is DetectiveTraceEntity detectiveTraceEntity)
		{
			detectiveTraceEntity.Found();
		}
	}
}
