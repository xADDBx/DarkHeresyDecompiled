using System.Collections.Generic;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLootEntity : TooltipBaseTemplate
{
	private readonly Sprite m_Icon;

	private readonly string m_Title;

	private readonly string m_Description;

	private readonly int m_Count;

	private readonly float m_ProfitFactorCost;

	public TooltipTemplateLootEntity(LootEntry lootEntry)
	{
		m_Icon = lootEntry.Item.Icon;
		m_Title = lootEntry.Item.Name;
		m_Description = lootEntry.Item.Description;
		m_Count = lootEntry.Count;
		m_ProfitFactorCost = lootEntry.ProfitFactorCost;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickIconAndNameVM(m_Title, m_Icon);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new BrickTextVM(m_Description, TooltipTextType.Paragraph);
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		yield return new BrickSeparatorVM();
		MultipleTextData[] array = new MultipleTextData[2];
		int count = m_Count;
		array[0] = new MultipleTextData(new TextEntity(count.ToString()));
		float profitFactorCost = m_ProfitFactorCost;
		array[1] = new MultipleTextData(new TextEntity(profitFactorCost.ToString()));
		yield return new BrickMultipleTextVM(array);
	}
}
