using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using TMPro;

namespace Kingmaker.Code.Framework.TextTools;

public class CaseIdTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (!GameLogContext.InScope)
		{
			return string.Empty;
		}
		BlueprintCase value = GameLogContext.Case.Value;
		TMP_Style tMP_Style = GameLogContext.TextStyle.Value.Style ?? TMP_Settings.defaultStyleSheet.GetStyle("Normal");
		return "<style=" + tMP_Style.name + ">" + UtilityDetectiveDecor.GetCaseUniqueId(value) + "</style>";
	}
}
