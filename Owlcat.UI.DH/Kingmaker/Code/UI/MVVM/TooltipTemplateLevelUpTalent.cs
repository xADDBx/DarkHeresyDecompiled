using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

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
		TextValueElement title = new TextValueElement(m_UIFeature.Name);
		string abilityAcronym = UtilityAbilities.GetAbilityAcronym(m_UIFeature.Name);
		TalentIconInfo talentIconsInfo = m_UIFeature.TalentIconsInfo;
		yield return new BrickLevelUpHeaderVM(new LevelUpFeatureUIData(title, abilityAcronym, null, null, null, default(Color), IconDecor.Default, talentIconsInfo));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Caster;
			list.Add(new BrickTextVM(m_UIFeature.Description, TooltipTextType.LevelUpLineSpacing, TooltipTextAlignment.Midl, Caster));
			return list;
		}
	}
}
