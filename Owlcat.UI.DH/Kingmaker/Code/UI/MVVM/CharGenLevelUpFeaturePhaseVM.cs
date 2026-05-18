using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpFeaturePhaseVM : CharGenLevelUpBaseSelectionPhaseVM<CharGenLevelUpSelectorBaseItemVM>
{
	private readonly List<BlueprintFeature> m_Features = new List<BlueprintFeature>();

	public CharGenLevelUpFeaturePhaseVM(List<BlueprintFeature> features, CharGenContext charGenContext, InfoSectionVM infoSectionVM, int rank = 0, CharGenPhaseType phaseType = CharGenPhaseType.LevelUpFeature)
		: base(charGenContext, phaseType, (SelectionStateFeature)null, infoSectionVM, rank)
	{
		m_Features = features;
		m_PhaseName.Value = UIStrings.Instance.CharGen.LevelUpFeature;
		CreateItemList();
	}

	protected override void CreateItemList()
	{
		if (m_Features.Empty())
		{
			return;
		}
		foreach (BlueprintFeature feature in m_Features)
		{
			AddItem(new CharGenLevelUpSelectorBaseItemVM(feature, OnItemHovered, null, allowSwitchOff: false, new TooltipTemplateFeature(feature, withVariants: false, m_CharGenContext.LevelUpManager.CurrentValue.PreviewUnit)));
		}
		Items?.First().SetSelected(state: true);
	}

	protected override void CheckItems()
	{
	}

	protected override void SaveSelection()
	{
	}

	protected override void UpdateItem(CharGenLevelUpSelectorBaseItemVM itemVM)
	{
	}

	protected override bool CheckIsCompleted()
	{
		return m_IsAvailable.CurrentValue;
	}
}
