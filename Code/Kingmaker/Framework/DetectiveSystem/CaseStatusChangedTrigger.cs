using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.DetectiveSystem;

[Serializable]
[ComponentName("Detective System/CaseStatusChangedTrigger")]
[TypeId("361998b903d2478ea7d147ffe93ef7e6")]
public class CaseStatusChangedTrigger : EntityFactComponentDelegate, ICaseStatusChanged, ISubscriber
{
	[ValidateNotNull]
	public BpRef<BlueprintCase> Case;

	public CaseStatus Status;

	public ActionList Actions;

	public void HandleCaseStatusChanged(BlueprintCase blueprint)
	{
		if (!(Case != blueprint) && Game.Instance.DetectiveSystem.GetCaseStatus(blueprint) == Status)
		{
			Actions.Run();
		}
	}
}
