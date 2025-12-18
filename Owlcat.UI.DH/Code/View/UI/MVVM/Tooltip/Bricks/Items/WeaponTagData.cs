using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.UnitLogic.Progression.Features;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class WeaponTagData : TagData
{
	private readonly WeaponTagUISettings m_Tag;

	public WeaponTagData(WeaponTagUISettings weaponTag)
	{
		m_Tag = weaponTag;
		FeatureTagsConfig featureTagsConfig = UIConfig.Instance.FeatureTagsConfig;
		base.Icon = featureTagsConfig.GetWeaponTagIcon(weaponTag);
		base.BgrColor = featureTagsConfig.GetWeaponTagColor(weaponTag);
		base.OwnerFeature = weaponTag.OwnerBlueprint as BlueprintFeature;
	}

	public override void GetNameAndDescription(out string name, out string description)
	{
		TempTagUtils.GetTagNameAndDescription(m_Tag, out name, out description);
	}
}
