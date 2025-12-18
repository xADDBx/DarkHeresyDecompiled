using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.UnitLogic.Progression.Features;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class ArmourTagData : TagData
{
	private readonly ArmourTagUISettings m_Tag;

	public ArmourTagData(ArmourTagUISettings armourTag)
	{
		m_Tag = armourTag;
		FeatureTagsConfig featureTagsConfig = UIConfig.Instance.FeatureTagsConfig;
		base.Icon = featureTagsConfig.GetArmourTagIcon(armourTag);
		base.BgrColor = featureTagsConfig.GetArmourTagColor(armourTag);
		base.OwnerFeature = armourTag.OwnerBlueprint as BlueprintFeature;
	}

	public override void GetNameAndDescription(out string name, out string description)
	{
		TempTagUtils.GetTagNameAndDescription(m_Tag, out name, out description);
	}
}
