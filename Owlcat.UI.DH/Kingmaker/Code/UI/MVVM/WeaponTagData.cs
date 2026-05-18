using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Code.UI.MVVM;

public class WeaponTagData : TagData
{
	private readonly WeaponTagUISettings m_Tag;

	public WeaponTagData(WeaponTagUISettings weaponTag, BlueprintItem blueprintItem = null)
	{
		m_Tag = weaponTag;
		base.BlueprintItem = blueprintItem;
		FeatureTagsConfig featureTagsConfig = UIConfig.Instance.FeatureTagsConfig;
		base.Icon = featureTagsConfig.GetWeaponTagIcon(weaponTag);
		base.BgrColor = featureTagsConfig.GetWeaponTagColor(weaponTag);
		base.OwnerFeature = weaponTag.OwnerBlueprint as BlueprintFeature;
	}

	public override string GetName()
	{
		return UIUtilityItem.GetTagName(m_Tag);
	}

	public override string GetDescription()
	{
		return UIUtilityItem.GetTagDescription(m_Tag);
	}
}
