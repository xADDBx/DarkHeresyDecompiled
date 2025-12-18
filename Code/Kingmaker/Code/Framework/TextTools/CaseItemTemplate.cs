using System.Collections.Generic;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using TMPro;

namespace Kingmaker.Code.Framework.TextTools;

public class CaseItemTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (!GameLogContext.InScope || GameLogContext.CaseItem.Value == null)
		{
			return string.Empty;
		}
		TMP_Style tMP_Style = GameLogContext.TextStyle.Value.Style ?? TMP_Settings.defaultStyleSheet.GetStyle("Normal");
		BlueprintCaseItem value = GameLogContext.CaseItem.Value;
		string text = ((value is BlueprintClue blueprintClue) ? blueprintClue.Name.Text : ((value is BlueprintClueAddendum blueprintClueAddendum) ? blueprintClueAddendum.Description.Text : ((!(value is BlueprintConclusion blueprintConclusion)) ? "" : blueprintConclusion.Description.Text)));
		string text2 = text;
		return "<style=" + tMP_Style.name + ">" + text2 + "</style>";
	}
}
