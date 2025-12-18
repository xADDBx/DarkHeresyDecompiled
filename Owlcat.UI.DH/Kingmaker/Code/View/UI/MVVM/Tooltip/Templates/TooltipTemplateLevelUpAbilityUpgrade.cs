using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateLevelUpAbilityUpgrade : TooltipBaseTemplate
{
	private readonly IUIDataProvider m_UpgradeAbility;

	private readonly IUIDataProvider m_BaseAbility;

	private readonly LevelUpManager m_LevelUpManager;

	private readonly BaseUnitEntity m_Caster;

	private BaseUnitEntity Caster
	{
		get
		{
			BaseUnitEntity baseUnitEntity = m_Caster;
			if (baseUnitEntity == null)
			{
				LevelUpManager levelUpManager = m_LevelUpManager;
				if (levelUpManager == null)
				{
					return null;
				}
				baseUnitEntity = levelUpManager.PreviewUnit;
			}
			return baseUnitEntity;
		}
	}

	public TooltipTemplateLevelUpAbilityUpgrade(IUIDataProvider upgradeAbility, IUIDataProvider baseAbility, BaseUnitEntity caster = null, LevelUpManager levelUpManager = null)
	{
		m_UpgradeAbility = upgradeAbility;
		m_BaseAbility = baseAbility;
		m_Caster = caster;
		m_LevelUpManager = levelUpManager;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickLevelUpHeader(new TooltipBrickLevelUpFeatureData(m_UpgradeAbility.Name, null, null, null, null, null, m_UpgradeAbility.Icon));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickLevelUpAbilityUpgradeDescription(GetDescription(m_UpgradeAbility), GetDescription(m_BaseAbility))
		};
	}

	private string GetDescription(IUIDataProvider ability)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
			GameLogContext.DescriptionFactBlueprint = (BlueprintMechanicEntityFact)ability;
			return UIUtilityText.UpdateDescriptionWithUIProperties(ability?.Description ?? string.Empty, Caster);
		}
	}
}
