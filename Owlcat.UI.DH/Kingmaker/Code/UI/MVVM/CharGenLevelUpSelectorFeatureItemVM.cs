using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSelectorFeatureItemVM : CharGenLevelUpSelectorBaseItemVM
{
	public CharGenLevelUpSelectorFeatureItemVM(BlueprintFeature feature, Action<CharGenLevelUpSelectorBaseItemVM> onHover)
		: base(feature, onHover)
	{
		m_Sprite.Value = feature.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		m_Blueprint = feature;
	}
}
