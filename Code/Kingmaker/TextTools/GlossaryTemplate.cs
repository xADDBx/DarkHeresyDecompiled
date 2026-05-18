using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.TextTools;

public class GlossaryTemplate : TextTemplate
{
	public override int MinParameters => 1;

	public override int MaxParameters => 1;

	public override int Balance => 1;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (parameters.Count < 1)
		{
			PFLog.Default.Error("UITemplate.Generate: parameter is missing");
		}
		string text = ((parameters.Count > 0) ? parameters[0] : "");
		if (GlossaryHolder.GetEntry(text) == null)
		{
			return "<unknown key>";
		}
		string empty = string.Empty;
		string text2 = "ui:" + text;
		TutorialColors tutorialColors = ConfigRoot.Instance.UIConfig.TutorialColors;
		string text3 = "#" + ColorUtility.ToHtmlStringRGB((Color)tutorialColors.UILinkColor);
		return "<b><color=" + text3 + "><link=\"" + text2 + "\">" + empty + "</link></color></b>";
	}
}
