using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c41f2af47bcaf21488c2fa21a8724215")]
[PlayerUpgraderAllowed(true)]
public class AllowUnitToFreezeOutsideCamera : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public bool FreezeOutsideCamera;

	public override string GetCaption()
	{
		return (FreezeOutsideCamera ? "Allow" : "Don't allow") + " unit " + Target?.GetCaption() + " to freeze outside camera";
	}

	protected override void RunAction()
	{
		Target.GetValue().FreezeOutsideCamera = FreezeOutsideCamera;
	}
}
