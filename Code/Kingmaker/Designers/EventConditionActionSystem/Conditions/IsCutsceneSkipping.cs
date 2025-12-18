using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Controllers;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("db9f89badbaa44e298e89a7425d26d59")]
public class IsCutsceneSkipping : Condition
{
	protected override string GetConditionCaption()
	{
		return "Cutscene is in skipping state";
	}

	protected override bool CheckCondition()
	{
		BlueprintCutscene blueprintCutscene = ContextData<NamedParametersContext.ContextData>.Current?.Context.Cutscene?.Cutscene;
		if (blueprintCutscene == null)
		{
			return false;
		}
		if (CutsceneController.Skipping)
		{
			return !blueprintCutscene.NonSkippable;
		}
		return false;
	}
}
