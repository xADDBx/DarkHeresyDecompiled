using System;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Levelup;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpModifier : TooltipBaseTemplate
{
	public readonly BlueprintAbilityModifier BlueprintModifier;

	private string m_Name = string.Empty;

	private string m_Description = string.Empty;

	private Sprite m_Icon;

	private readonly LevelUpManager m_LevelUpManager;

	private readonly MechanicEntity m_Caster;

	private MechanicEntity Caster
	{
		get
		{
			object obj = m_Caster;
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

	public TooltipTemplateLevelUpModifier(BlueprintAbilityModifier modifier, LevelUpManager levelUpManager = null, MechanicEntity caster = null)
	{
		BlueprintModifier = modifier;
		m_LevelUpManager = levelUpManager;
		m_Caster = caster;
	}

	public override void Prepare(TooltipTemplateType type)
	{
		if (BlueprintModifier != null)
		{
			FillAbilityModifierDataInfo();
		}
	}

	private void FillAbilityModifierDataInfo()
	{
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Caster;
				m_Name = BlueprintModifier.Name;
				m_Description = BlueprintModifier.Description;
				m_Icon = BlueprintModifier.Tags.FirstOrDefault()?.Icon;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {BlueprintModifier.name}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<string> values = BlueprintModifier.Tags.Select((BlueprintAbilityTag tag) => tag.Name.Text).ToList();
		yield return new BrickLevelUpHeaderVM(new LevelUpFeatureUIData(new TextValueElement(m_Name), null, new TextValueElement(string.Join(", ", values)), null, m_Icon));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDescription(list);
		list.Add(new BrickSpaceVM(10f));
		if (type == TooltipTemplateType.Info)
		{
			AddFittingAbilities(list);
		}
		else
		{
			AddCurrentAbilityLink(list);
		}
		return list;
	}

	private void AddCurrentAbilityLink(List<ITooltipBrick> bricks)
	{
		if (Caster == null)
		{
			return;
		}
		PartAbilityModifiers optional = Caster.GetOptional<PartAbilityModifiers>();
		if (optional == null)
		{
			return;
		}
		string text = string.Empty;
		Sprite sprite = null;
		TooltipBaseTemplate tooltipBaseTemplate = null;
		PartAbilityModifiers.AddedEntry validAbilityEntry = GetValidAbilityEntry(optional, BlueprintModifier);
		if (validAbilityEntry != null)
		{
			if (validAbilityEntry.Ability != null)
			{
				text = validAbilityEntry.Ability.LocalizedName.Text;
				sprite = validAbilityEntry.Ability.Icon;
				tooltipBaseTemplate = new TooltipTemplateAbility(validAbilityEntry.Ability, null, Caster);
			}
			else if (validAbilityEntry.AbilityTag != null)
			{
				text = validAbilityEntry.AbilityTag.Name.Text;
				sprite = validAbilityEntry.AbilityTag.Icon;
				tooltipBaseTemplate = new TooltipTemplateAbilityTag(validAbilityEntry.AbilityTag);
			}
		}
		PartAbilityModifiers.ToggleAbilityEntry toggleAbilityEntry = optional.BoundToToggleAbilityModifiers.FirstOrDefault((PartAbilityModifiers.ToggleAbilityEntry m) => m.Modifier == BlueprintModifier);
		if (toggleAbilityEntry != null)
		{
			text = toggleAbilityEntry.Target.Name;
			sprite = toggleAbilityEntry.Target.Icon;
			tooltipBaseTemplate = new TooltipTemplateToggleAbility(toggleAbilityEntry.Target, Caster);
		}
		if (!text.IsNullOrEmpty() || !(sprite == null))
		{
			TextValueElement title = new TextValueElement(text);
			Sprite icon = sprite;
			TooltipBaseTemplate tooltip = tooltipBaseTemplate;
			bricks.Add(new BrickLevelUpFeatureVM(new LevelUpFeatureUIData(title, null, null, null, icon, default(Color), IconDecor.Default, null, tooltip)));
		}
	}

	private PartAbilityModifiers.AddedEntry GetValidAbilityEntry(PartAbilityModifiers modifiers, BlueprintAbilityModifier modifier)
	{
		foreach (PartAbilityModifiers.AddedEntry addedModifier in modifiers.AddedModifiers)
		{
			if (addedModifier.Modifier == modifier && (addedModifier.Ability != null || addedModifier.AbilityTag != null))
			{
				return addedModifier;
			}
		}
		return null;
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		bricks.Add(new BrickTextVM(m_Description, TooltipTextType.LevelUpLineSpacing, TooltipTextAlignment.Midl, Caster));
	}

	private void AddFittingAbilities(List<ITooltipBrick> bricks)
	{
		if (Caster is BaseUnitEntity unit)
		{
			bricks.Add(new BrickLevelUpFittingAbilitiesVM(BlueprintModifier, unit));
		}
	}
}
