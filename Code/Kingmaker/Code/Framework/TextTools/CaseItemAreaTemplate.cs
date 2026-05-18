using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using TMPro;

namespace Kingmaker.Code.Framework.TextTools;

public class CaseItemAreaTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (!GameLogContext.InScope || GameLogContext.CaseItemArea.Value == null)
		{
			return string.Empty;
		}
		BlueprintArea value = GameLogContext.CaseItemArea.Value;
		TMP_Style tMP_Style = GameLogContext.TextStyle.Value.Style ?? TMP_Settings.defaultStyleSheet.GetStyle("Normal");
		return "<style=" + tMP_Style.name + ">" + value?.Name + "</style>";
	}
}
