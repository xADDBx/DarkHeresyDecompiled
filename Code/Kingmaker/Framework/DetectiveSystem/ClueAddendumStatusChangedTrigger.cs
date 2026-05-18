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
[Obsolete("Unused")]
[ComponentName("Detective System/ClueAddendumStatusChangedTrigger")]
[TypeId("44798af63b834d70b7b3bf88d90b3020")]
public class ClueAddendumStatusChangedTrigger : EntityFactComponentDelegate, IClueAddendumStatusChanged, ISubscriber
{
	public enum ExpectedStatus
	{
		Added,
		Removed
	}

	[ValidateNotNull]
	public BpRef<BlueprintClueAddendum> Addendum;

	public ExpectedStatus Status;

	public ActionList Actions;

	public void HandleClueAddendumStatusChanged(BlueprintClueAddendum blueprint)
	{
		if (!(Addendum != blueprint) && Game.Instance.DetectiveSystem.HasClueAddendumExcludingHidden(Addendum) == (Status == ExpectedStatus.Added))
		{
			Actions.Run();
		}
	}
}
