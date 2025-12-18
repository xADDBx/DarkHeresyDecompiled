using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using TMPro;

namespace Kingmaker.Code.Framework.TextTools;

public class ClueIdTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (!GameLogContext.InScope || !(GameLogContext.CaseItem.Value is BlueprintClue blueprintClue))
		{
			return string.Empty;
		}
		TMP_Style tMP_Style = GameLogContext.TextStyle.Value.Style ?? TMP_Settings.defaultStyleSheet.GetStyle("Normal");
		string text = blueprintClue.UIClueType switch
		{
			BlueprintClue.UIType.Default => UtilityDetectiveDecor.GetDefaultUniqueId(blueprintClue), 
			BlueprintClue.UIType.Person => UtilityDetectiveDecor.GetPersonUniqueId(blueprintClue), 
			BlueprintClue.UIType.Location => UtilityDetectiveDecor.GetLocationUniqueId(blueprintClue), 
			BlueprintClue.UIType.Weapon => UtilityDetectiveDecor.GetDefaultUniqueId(blueprintClue), 
			BlueprintClue.UIType.Event => UtilityDetectiveDecor.GetDefaultUniqueId(blueprintClue), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		return "<style=" + tMP_Style.name + ">" + text + "</style>";
	}
}
