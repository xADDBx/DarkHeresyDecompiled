using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpPhaseDescription : TooltipBaseTemplate
{
	private readonly BlueprintSelectionWithUI m_BlueprintSelectionWithUI;

	public TooltipTemplateLevelUpPhaseDescription(BlueprintSelectionWithUI blueprintSelectionWithUI)
	{
		m_BlueprintSelectionWithUI = blueprintSelectionWithUI;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickLevelUpHeaderVM(new LevelUpFeatureUIData(new TextValueElement(new TextEntity(m_BlueprintSelectionWithUI.Title, TextFieldParams.Center))));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_BlueprintSelectionWithUI.DescriptionEntry == null)
		{
			return list;
		}
		foreach (EncyclopediaEntryBlock block in m_BlueprintSelectionWithUI.DescriptionEntry.Blocks)
		{
			ITooltipBrick tooltipBrickByEntryBlock = GetTooltipBrickByEntryBlock(block);
			if (tooltipBrickByEntryBlock != null)
			{
				list.Add(tooltipBrickByEntryBlock);
			}
		}
		return list;
	}

	private ITooltipBrick GetTooltipBrickByEntryBlock(EncyclopediaEntryBlock block)
	{
		return block.blockType switch
		{
			EncyclopediaEntryBlock.BlockType.Text => new BrickTextVM(block.GetDescription()), 
			EncyclopediaEntryBlock.BlockType.Image => new BrickImageVM(block.ImageLink?.Load()), 
			_ => null, 
		};
	}
}
