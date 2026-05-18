using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Alignments;

namespace Code.View.UI.UIUtils;

public static class UIUtilityAlignment
{
	public static string GetGlossaryKeyByDirection(AlignmentAxis direction)
	{
		return direction switch
		{
			AlignmentAxis.Monodominance => "AlignmentMonodominance", 
			AlignmentAxis.Torian => "AlignmentTorian", 
			AlignmentAxis.Xanthite => "AlignmentXanthite", 
			AlignmentAxis.Xenophilia => "AlignmentXenophilia", 
			_ => string.Empty, 
		};
	}

	public static string GetAlignmentRankText(int index)
	{
		return UIUtilityText.ArabicToRoman(index);
	}

	public static LocalizedString GetAlignmentDirectionText(AlignmentAxis direction)
	{
		return ConfigRoot.Instance.AlignmentMarksRoot.GetAlignmentInfo(direction).Name;
	}

	public static string GetAlignmentText(IEnumerable<AlignmentShift> shifts)
	{
		return string.Join(", ", shifts.Select(GetLink));
		static string GetLink(AlignmentShift shift)
		{
			return $"<link=\"{EntityLink.Type.Alignment}:{shift.Axis}\">{GetAlignmentDirectionText(shift.Axis).Text}</link>";
		}
	}

	public static string GetAlignmentRequirementText(BlueprintAnswer blueprintAnswer)
	{
		IEnumerable<HasAlignment> source = blueprintAnswer.SelectConditions.Conditions.OfType<HasAlignment>();
		return string.Join(", ", source.Select(GetLink));
		static string GetLink(HasAlignment condition)
		{
			return $"<link=\"{EntityLink.Type.Alignment}:{condition.Axis}\">{GetFormattedCondition(condition)}</link>";
		}
	}

	public static string GetFormattedCondition(HasAlignment condition)
	{
		string text = (condition.CheckByMark ? "AlignmentReqMark" : "AlignmentReqRank");
		string text2 = (condition.CheckByMark ? GetAlignmentRankText(condition.Value) : condition.Value.ToString());
		return GetAlignmentDirectionText(condition.Axis).Text + "<style=" + text + ">" + text2 + "</style>";
	}
}
