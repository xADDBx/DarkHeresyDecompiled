using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateScatterDeviation : TooltipBaseTemplate
{
	private string m_Header;

	private int m_Min;

	private int m_Max;

	private int m_Value;

	private Color m_Color;

	public TooltipTemplateScatterDeviation(string header, int min, int max, int value, Color color)
	{
		m_Header = header;
		m_Min = min;
		m_Max = max;
		m_Value = value;
		m_Color = color;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_Header);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickShotDirection(m_Min, m_Max, m_Value);
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Small);
		string text = AddColor(UIStrings.Instance.CombatLog.CentralShotDirection.Text);
		string slightDeviationText = AddColor(UIStrings.Instance.CombatLog.SlightDeviationShotDirection.Text);
		string strongDeviationText = AddColor(UIStrings.Instance.CombatLog.StrongDeviationShotDirection.Text);
		yield return new TooltipBrickTextValue("<b>" + text + "</b>", "0—" + m_Min);
		yield return new TooltipBrickTextValue("<b>" + slightDeviationText + "</b>", m_Min + 1 + "—" + m_Max);
		yield return new TooltipBrickTextValue("<b>" + strongDeviationText + "</b>", m_Max + 1 + "—100");
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Small);
		yield return new TooltipBrickText(UIStrings.Instance.CombatLog.DeviationDescription.Text);
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Small);
		BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry("BurstAttacks");
		yield return new TooltipBrickText(glossaryEntry.GetDescription());
	}

	private string AddColor(string text)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(m_Color) + ">" + text + "</color>";
	}
}
