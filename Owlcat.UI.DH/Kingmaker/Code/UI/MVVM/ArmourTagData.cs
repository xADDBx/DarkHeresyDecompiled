using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Code.UI.MVVM;

public class ArmourTagData : TagData
{
	private readonly ArmourTagUISettings m_Tag;

	public ArmourTagData(ArmourTagUISettings armourTag, BlueprintItem blueprintItem = null)
	{
		m_Tag = armourTag;
		base.BlueprintItem = blueprintItem;
		FeatureTagsConfig featureTagsConfig = UIConfig.Instance.FeatureTagsConfig;
		base.Icon = featureTagsConfig.GetArmourTagIcon(armourTag);
		base.BgrColor = featureTagsConfig.GetArmourTagColor(armourTag);
		base.OwnerFeature = armourTag.OwnerBlueprint as BlueprintFeature;
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
