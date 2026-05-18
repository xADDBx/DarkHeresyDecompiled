using Kingmaker.Designers.EventConditionActionSystem.Evaluators.BarkBanters;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(true)]
[TypeId("9cb3c0795f9e49e2a0afcc02c67f8178")]
public class StartDetectiveTrace : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public DetectiveTraceRootEvaluator DetectiveTraceRoot;

	public override string GetCaption()
	{
		return "Starts Detective Root Trace " + DetectiveTraceRoot?.GetCaption();
	}

	protected override void RunAction()
	{
		if (DetectiveTraceRoot.DetectiveTraceRoot.FindData() is DetectiveTraceRootEntity detectiveTraceRootEntity)
		{
			detectiveTraceRootEntity.FoundRootTraces();
			InteractionPartDetectiveTrace optional = detectiveTraceRootEntity.GetOptional<InteractionPartDetectiveTrace>();
			if (optional != null)
			{
				optional.Enabled = false;
			}
		}
	}
}
