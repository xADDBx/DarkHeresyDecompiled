using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateScatterDeviation : TooltipBaseTemplate
{
	private readonly string m_Header;

	private readonly int m_Min;

	private readonly int m_Max;

	private readonly int m_Value;

	private readonly Color m_Color;

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
		yield return new BrickTitleVM(m_Header);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new BrickShotDirectionVM(m_Min, m_Max, m_Value);
		yield return new BrickSeparatorVM(TooltipBrickElementType.Small);
		UICombatLogTexts combatLog = UIStrings.Instance.CombatLog;
		string text = AddColor(combatLog.CentralShotDirection.Text);
		string slightDeviationText = AddColor(combatLog.SlightDeviationShotDirection.Text);
		string strongDeviationText = AddColor(combatLog.StrongDeviationShotDirection.Text);
		string text2 = "<b>" + text + "</b>";
		int min = m_Min;
		yield return new BrickTextValueVM(text2, "0—" + min);
		string text3 = "<b>" + slightDeviationText + "</b>";
		string text4 = (m_Min + 1).ToString();
		min = m_Max;
		yield return new BrickTextValueVM(text3, text4 + "—" + min);
		yield return new BrickTextValueVM("<b>" + strongDeviationText + "</b>", m_Max + 1 + "—100");
		yield return new BrickSeparatorVM(TooltipBrickElementType.Small);
		yield return new BrickTextVM(UIStrings.Instance.CombatLog.DeviationDescription.Text);
		yield return new BrickSeparatorVM(TooltipBrickElementType.Small);
		BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry("BurstAttacks");
		yield return new BrickTextVM(glossaryEntry.GetDescription());
	}

	private string AddColor(string text)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGB(m_Color) + ">" + text + "</color>";
	}
}
