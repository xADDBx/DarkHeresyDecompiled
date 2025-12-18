using System.Collections.Generic;
using System.Linq;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using TMPro;

namespace Kingmaker.Code.Framework.TextTools;

public class CaseAnswerTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (!GameLogContext.InScope || GameLogContext.CaseAnswer.Value == null)
		{
			return string.Empty;
		}
		TMP_Style tMP_Style = GameLogContext.TextStyle.Value.Style ?? TMP_Settings.defaultStyleSheet.GetStyle("Normal");
		BlueprintCaseAnswer value = GameLogContext.CaseAnswer.Value;
		if (!Game.Instance.DetectiveSystem.TryGetAnswerDegree(value, out var degree))
		{
			return string.Empty;
		}
		return "<style=" + tMP_Style.name + ">" + value.DegreeProgression.ElementAt(degree)?.Description.Text + "</style>";
	}
}
