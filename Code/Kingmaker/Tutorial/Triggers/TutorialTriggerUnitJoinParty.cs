using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("`t|SourceUnit` - unit which join party")]
[TypeId("efdd9126b8884b1d8b17a0e40a040b5f")]
public abstract class TutorialTriggerUnitJoinParty : TutorialTrigger, IPartyHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	private bool ShouldTriggerInternal(BaseUnitEntity unit)
	{
		try
		{
			return ShouldTrigger(unit);
		}
		catch (Exception exception)
		{
			PFLog.Default.ExceptionWithReport(exception, null);
			return false;
		}
	}

	protected abstract bool ShouldTrigger(BaseUnitEntity unit);

	public void HandleAddCompanion()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (ShouldTriggerInternal(unit))
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit;
			});
		}
	}

	public void HandleCompanionActivated()
	{
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
	}

	public void HandleCapitalModeChanged()
	{
	}
}
