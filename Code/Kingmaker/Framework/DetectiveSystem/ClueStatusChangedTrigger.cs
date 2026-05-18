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
[ComponentName("Detective System/ClueStatusChangedTrigger")]
[TypeId("39aea2c3e91a48b1aab8ae879a8d2249")]
public class ClueStatusChangedTrigger : EntityFactComponentDelegate, IClueStatusChanged, ISubscriber
{
	public enum ExpectedStatus
	{
		Added,
		Removed
	}

	[ValidateNotNull]
	public BpRef<BlueprintClue> Clue;

	public ExpectedStatus Status;

	public ActionList Actions;

	public void HandleClueStatusChanged(BlueprintClue blueprint)
	{
		if (!(Clue != blueprint) && Game.Instance.DetectiveSystem.HasClueExcludingHidden(Clue) == (Status == ExpectedStatus.Added))
		{
			Actions.Run();
		}
	}
}
