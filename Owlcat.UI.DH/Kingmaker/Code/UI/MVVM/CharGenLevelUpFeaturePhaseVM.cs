using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpFeaturePhaseVM : CharGenLevelUpBasePhaseVM<CharGenLevelUpSelectorBaseItemVM>
{
	private readonly List<BlueprintFeature> m_Features = new List<BlueprintFeature>();

	private bool m_IsCompleted;

	public CharGenLevelUpFeaturePhaseVM(CharGenContext charGenContext, List<BlueprintFeature> features, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, CharGenPhaseType.LevelUpFeature, infoSectionVM, rank)
	{
		m_Features = features;
		CreateItemList();
	}

	protected override void CreateItemList()
	{
		foreach (BlueprintFeature feature in m_Features)
		{
			AddItem(new CharGenLevelUpSelectorFeatureItemVM(feature, OnItemHovered));
		}
	}

	protected override void OnBeginDetailedView()
	{
		base.OnBeginDetailedView();
		m_IsCompleted = true;
		UpdateIsCompleted();
	}

	protected override bool CheckIsCompleted()
	{
		return m_IsCompleted;
	}
}
