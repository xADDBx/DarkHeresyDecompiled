using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("04d841c4b838459b930b877555a8b32a")]
public class IsGateActivated : Condition
{
	[SerializeField]
	public CutsceneReference? Cutscene;

	protected override string GetConditionCaption()
	{
		return "Gate was activated";
	}

	protected override bool CheckCondition()
	{
		if (Cutscene == null)
		{
			return false;
		}
		_ = ContextData<NamedParametersContext.ContextData>.Current?.Context.Cutscene;
		return false;
	}
}
