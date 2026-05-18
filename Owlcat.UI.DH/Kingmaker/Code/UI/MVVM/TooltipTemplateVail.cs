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
	private readonly string m_GlossaryHeader = UIStrings.Instance.ActionBar.VailHeader.Text;

	private readonly string m_GlossaryFooter = UIStrings.Instance.ActionBar.VeilPhenomenaDescription.Text;

	private int m_VeilValue;

	private int m_PerilChance;

	private int m_PhenomenaChance;

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new BrickIconValueStatVM(new TextValueElement(UIUtilityEncyclopedy.GetGlossaryEntryName("VeilThickness"), m_VeilValue.ToString()), ConfigRoot.Instance.UIConfig.UIIcons.TooltipIcons.Vail, IconColor.White)
		};
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> obj = new List<ITooltipBrick>
		{
			new BrickTextVM(m_GlossaryHeader, TooltipTextType.Simple, TooltipTextAlignment.Left),
			new BrickSeparatorVM(TooltipBrickElementType.Small)
		};
		int maxVeilDamage = ConfigRoot.Instance.PsykerRoot.MaxVeilDamage;
		int num = m_VeilValue * 100 / maxVeilDamage;
		int veilValue = m_VeilValue;
		List<SliderValuesVM> list = new List<SliderValuesVM>();
		int value = Mathf.RoundToInt((float)m_PhenomenaChance / 100f * (float)num);
		Color32 veilPhenomenaColor = UIConfig.Instance.TooltipColors.VeilPhenomenaColor;
		string text = $"{m_PhenomenaChance}%";
		list.Add(new SliderValuesVM(0, 100, value, null, needColor: true, veilPhenomenaColor, null, isValueOnBottom: false, needValueText: false, text));
		Color32 veilPerilColor = UIConfig.Instance.TooltipColors.VeilPerilColor;
		text = $"{m_PerilChance}%";
		list.Add(new SliderValuesVM(0, 100, num, null, needColor: true, veilPerilColor, null, isValueOnBottom: false, needValueText: false, text));
		obj.Add(new BrickSliderVM(0, maxVeilDamage, veilValue, list, showValue: false, UIConfig.Instance.TooltipColors.ProgressbarNeutral));
		obj.Add(new BrickTextVM(m_GlossaryFooter, TooltipTextType.Simple, TooltipTextAlignment.Left));
		return obj;
	}

	public void ChangeValue(int veilValue, int perilChance)
	{
		m_VeilValue = veilValue;
		m_PerilChance = perilChance;
		m_PhenomenaChance = 100 - perilChance;
	}
}
