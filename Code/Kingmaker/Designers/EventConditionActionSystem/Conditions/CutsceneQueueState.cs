using System;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("f50bffd8f7990a748b616b0ee7848a46")]
public class CutsceneQueueState : Condition
{
	public bool First;

	public bool Last;

	protected override string GetConditionCaption()
	{
		if (First)
		{
			if (!Last)
			{
				return "Current cutscene is the first in queue";
			}
			return "Current cutscene is the only one in queue";
		}
		if (!Last)
		{
			return "Current cutscene is whatever";
		}
		return "Current cutscene is the last one in queue";
	}

	protected override bool CheckCondition()
	{
		CutscenePlayerData cutscenePlayerData = ContextData<NamedParametersContext.ContextData>.Current?.Context.Cutscene;
		if (cutscenePlayerData == null)
		{
			return false;
		}
		if (!First || cutscenePlayerData.IsFirstInQueue)
		{
			if (Last)
			{
				return cutscenePlayerData.IsLastInQueue;
			}
			return true;
		}
		return false;
	}
}
