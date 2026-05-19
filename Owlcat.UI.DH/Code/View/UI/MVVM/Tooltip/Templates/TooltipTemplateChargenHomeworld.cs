using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateChargenHomeworld : TooltipBaseTemplate
{
	private readonly BlueprintFeature m_Homeworld;

	private readonly HomeworldUISettings m_HomeworldUISettings;

	private readonly MechanicEntity m_Unit;

	private readonly LevelUpManager m_LevelUpManager;

	private Sprite m_Icon;

	private string m_DisplayName = string.Empty;

	private string m_Subname = string.Empty;

	private string m_LoreDescription = string.Empty;

	private MechanicEntity FallbackCaster
	{
		get
		{
			object obj = m_Unit;
			if (obj == null)
			{
				LevelUpManager levelUpManager = m_LevelUpManager;
				if (levelUpManager == null)
				{
					return null;
				}
				obj = levelUpManager.PreviewUnit;
			}
			return (MechanicEntity)obj;
		}
	}

	public TooltipTemplateChargenHomeworld(BlueprintFeature homeworld, MechanicEntity unit = null, LevelUpManager levelUpManager = null)
	{
		m_Homeworld = homeworld;
		m_Unit = unit;
		m_LevelUpManager = levelUpManager;
		m_HomeworldUISettings = m_Homeworld.GetComponent<HomeworldUISettings>();
	}

	public override void Prepare(TooltipTemplateType type)
	{
		try
		{
			if (m_Homeworld != null)
			{
				if (m_HomeworldUISettings != null)
				{
					m_Icon = m_HomeworldUISettings.Icon.Load();
					m_DisplayName = m_HomeworldUISettings.PlanetName.Text;
					m_Subname = UIStrings.Instance.CharGen.Homeworld.Text;
					m_LoreDescription = m_HomeworldUISettings.PlanetDescription;
				}
				else
				{
					m_Icon = m_Homeworld.Icon;
					m_DisplayName = m_Homeworld.Name;
					m_Subname = UIStrings.Instance.CharGen.Homeworld.Text;
				}
			}
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickHomeworldTitleVM(m_Icon, m_DisplayName, m_Subname);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddKeystoneFeature(list);
		list.Add(new BrickSpaceVM(10f));
		UIUtilityFeaturesTooltip.AddStatBonuses(list, m_Homeworld, StatTypeHelper.Attributes, FeatureGroup.Attribute);
		UIUtilityFeaturesTooltip.AddStatBonuses(list, m_Homeworld, StatTypeHelper.Skills, FeatureGroup.Skill);
		UIUtilityFeaturesTooltip.ParseByBlockAndAddFeatures(list, m_Homeworld);
		UIUtilityFeaturesTooltip.AddChargenProgressionLockedFoldable(list, m_Homeworld);
		return list;
	}

	private void AddKeystoneFeature(List<ITooltipBrick> bricks)
	{
		List<AddKeystoneFeatureInfo> list = m_Homeworld?.GetComponents<AddKeystoneFeatureInfo>().ToList();
		if (!list.Empty())
		{
			list.ForEach(delegate(AddKeystoneFeatureInfo k)
			{
				bricks.Add(new BrickFeatureDescriptionVM(k, FallbackCaster));
			});
		}
	}
}
