using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateLevelUpTalent : TooltipBaseTemplate
{
	private readonly UIFeature m_UIFeature;

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

	public TooltipTemplateLevelUpTalent(UIFeature uiFeature, BaseUnitEntity caster = null, LevelUpManager levelUpManager = null)
	{
		m_UIFeature = uiFeature;
		m_Caster = caster;
		m_LevelUpManager = levelUpManager;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		string name = m_UIFeature.Name;
		string abilityAcronym = UtilityAbilities.GetAbilityAcronym(m_UIFeature.Name);
		TalentIconInfo talentIconsInfo = m_UIFeature.TalentIconsInfo;
		yield return new TooltipBrickLevelUpHeader(new TooltipBrickLevelUpFeatureData(name, null, abilityAcronym, null, null, null, null, iconWithFrame: true, default(Vector2), talentIconsInfo));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickText(UIUtilityText.UpdateDescriptionWithUIProperties(m_UIFeature.Description, Caster), TooltipTextType.LevelUpLineSpacing)
		};
	}
}
