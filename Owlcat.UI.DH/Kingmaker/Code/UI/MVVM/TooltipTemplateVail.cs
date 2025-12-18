using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateVail : TooltipBaseTemplate
{
	private readonly string m_GlossaryHeader;

	private readonly string m_GlossaryFooter;

	private int m_VeilValue;

	private int m_PerilChance;

	private int m_PhenomenaChance;

	public TooltipTemplateVail()
	{
		m_GlossaryHeader = UIStrings.Instance.ActionBar.VailHeader.Text;
		m_GlossaryFooter = UIStrings.Instance.ActionBar.VeilPhenomenaDescription.Text;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickIconValueStat(UIUtilityEncyclopedy.GetGlossaryEntryName("VeilThickness"), m_VeilValue.ToString(), ConfigRoot.Instance.UIConfig.UIIcons.TooltipIcons.Vail, TooltipIconValueStatType.Normal, isWhite: true)
		};
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> obj = new List<ITooltipBrick>
		{
			new TooltipBrickText(m_GlossaryHeader, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left),
			new TooltipBrickSeparator(TooltipBrickElementType.Small)
		};
		int maxVeilDamage = ConfigRoot.Instance.PsykerRoot.MaxVeilDamage;
		int num = m_VeilValue * 100 / maxVeilDamage;
		int veilValue = m_VeilValue;
		List<BrickSliderValueVM> list = new List<BrickSliderValueVM>();
		int value = Mathf.RoundToInt((float)m_PhenomenaChance / 100f * (float)num);
		Color32 veilPhenomenaColor = UIConfig.Instance.TooltipColors.VeilPhenomenaColor;
		string text = $"{m_PhenomenaChance}%";
		list.Add(new BrickSliderValueVM(0, 100, value, null, needColor: true, veilPhenomenaColor, null, isValueOnBottom: false, needValueText: false, text));
		Color32 veilPerilColor = UIConfig.Instance.TooltipColors.VeilPerilColor;
		text = $"{m_PerilChance}%";
		list.Add(new BrickSliderValueVM(0, 100, num, null, needColor: true, veilPerilColor, null, isValueOnBottom: false, needValueText: false, text));
		obj.Add(new TooltipBrickSlider(0, maxVeilDamage, veilValue, list, showValue: false, 50, UIConfig.Instance.TooltipColors.ProgressbarNeutral));
		obj.Add(new TooltipBrickText(m_GlossaryFooter, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		return obj;
	}

	public void ChangeValue(int veilValue, int perilChance)
	{
		m_VeilValue = veilValue;
		m_PerilChance = perilChance;
		m_PhenomenaChance = 100 - perilChance;
	}
}
