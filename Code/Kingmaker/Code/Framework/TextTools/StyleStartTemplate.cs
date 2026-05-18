using System.Collections.Generic;
using System.Linq;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using TMPro;

namespace Kingmaker.Code.Framework.TextTools;

public class StyleStartTemplate : TextTemplate
{
	public override int Balance => 1;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (parameters.ElementAtOrDefault(0) != null)
		{
			return "<style=" + parameters[0] + ">";
		}
		TMP_Style tMP_Style = (GameLogContext.InScope ? GameLogContext.TextStyle.Value.Style : TMP_Settings.defaultStyleSheet.GetStyle("Normal"));
		return "<style=" + tMP_Style.name + ">";
	}
}
