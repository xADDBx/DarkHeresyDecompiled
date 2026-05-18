using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.Utility.UnitDescription;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpAbility : TooltipBaseTemplate
{
	public readonly BlueprintAbility BlueprintAbility;

	public readonly BlueprintItem SourceItem;

	public readonly MechanicEntity Caster;

	private AbilityData m_AbilityData;

	private string m_Name = string.Empty;

	private Sprite m_Icon;

	private int? m_IntCost;

	private string m_Target = string.Empty;

	private Sprite m_TargetIcon;

	private string m_ShortDescriptionText = string.Empty;

	private string m_LongDescriptionText = string.Empty;

	private readonly UnitDescription.UIDamageInfo[] m_DamageInfo;

	private readonly string m_AutoCastHint = string.Empty;

	private UIUtilityItem.UIAbilityData m_UIAbilityData;

	private bool m_IsReload;

	private readonly ItemEntityWeapon m_Weapon;

	private List<BlueprintAbilityModifier> m_AppliedModifiers = new List<BlueprintAbilityModifier>();

	private readonly LevelUpManager m_LevelUpManager;

	public AbilityData AbilityData => m_AbilityData;

	private MechanicEntity FallbackCaster
	{
		get
		{
			object obj = Caster;
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

	public TooltipTemplateLevelUpAbility(BlueprintAbility blueprintAbility, BlueprintItem sourceItem = null, MechanicEntity caster = null, bool _ = false, LevelUpManager levelUpManager = null)
	{
		BlueprintAbility = blueprintAbility;
		SourceItem = sourceItem;
		Caster = caster;
		m_LevelUpManager = levelUpManager;
		m_AbilityData = Caster?.GetAbilityData(BlueprintAbility);
	}

	public override void Prepare(TooltipTemplateType type)
	{
		if (AbilityData != null)
		{
			FillAbilityDataInfo(AbilityData);
		}
		else if (BlueprintAbility != null)
		{
			FillBlueprintAbilityData(BlueprintAbility);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return AddAbilityHeader();
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDescription(list, type);
		AddAbilityModificationsDescription(list);
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Tooltip && !string.IsNullOrEmpty(m_AutoCastHint))
		{
			yield return new BrickSeparatorVM(TooltipBrickElementType.Medium);
			yield return new BrickTextVM(m_AutoCastHint, TooltipTextType.Italic, TooltipTextAlignment.Midl, Caster);
		}
	}

	private void FillAbilityDataInfo(AbilityData abilityData)
	{
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionFactBlueprint = BlueprintAbility;
				m_Name = abilityData.Name;
				m_Icon = abilityData.Icon;
				BlueprintAbility originalBlueprint = abilityData.Blueprint.OriginalBlueprint;
				m_Target = UIUtilityAbilities.GetAbilityTarget(abilityData);
				m_TargetIcon = UIUtilityAbilities.GetTargetImage(originalBlueprint);
				m_ShortDescriptionText = abilityData.ShortenedDescription;
				m_LongDescriptionText = abilityData.Description;
				m_UIAbilityData = UIUtilityItem.GetUIAbilityData(originalBlueprint, abilityData.Weapon);
				FindAppliedModifiers(originalBlueprint);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {abilityData?.Blueprint?.name}: {arg}");
		}
	}

	private void FillBlueprintAbilityData(BlueprintAbility blueprintAbility)
	{
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)FallbackCaster;
				GameLogContext.DescriptionFactBlueprint = BlueprintAbility;
				BlueprintItemWeapon blueprintItem = SourceItem as BlueprintItemWeapon;
				m_Name = blueprintAbility.Name;
				m_Icon = blueprintAbility.Icon;
				m_Target = UIUtilityAbilities.GetAbilityTarget(blueprintAbility, blueprintItem);
				m_TargetIcon = UIUtilityAbilities.GetTargetImage(blueprintAbility);
				m_ShortDescriptionText = blueprintAbility.GetShortenedDescription();
				m_LongDescriptionText = blueprintAbility.Description;
				m_UIAbilityData = UIUtilityItem.GetUIAbilityData(blueprintAbility, blueprintItem, Caster);
				FindAppliedModifiers(blueprintAbility);
				m_IntCost = new AbilityData(blueprintAbility, FallbackCaster, 0, m_AppliedModifiers).GetBaseActionPointCost();
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {blueprintAbility.name}: {arg}");
		}
	}

	private ITooltipBrick AddAbilityHeader()
	{
		List<string> values = BlueprintAbility.Tags.Select((BpRef<BlueprintAbilityTag> tag) => tag.Blueprint.Reference().Blueprint.Name.Text).ToList();
		TooltipLevelUpAbilityData abilityData = new TooltipLevelUpAbilityData(null, GetTarget());
		string value = (m_IntCost.HasValue ? string.Format(UIStrings.Instance.Tooltips.AbilityAPCost.Text, m_IntCost) : null);
		return new BrickLevelUpHeaderVM(new LevelUpFeatureUIData(new TextValueElement(m_Name), null, new TextValueElement(string.Join(", ", values), value), null, m_Icon), null, abilityData);
	}

	private BrickIconPatternVM GetTarget()
	{
		if (m_IsReload)
		{
			return null;
		}
		if (string.IsNullOrEmpty(m_Target) || m_TargetIcon == null)
		{
			return null;
		}
		TextEntity title = new TextEntity(string.Empty);
		TextValueElement secondaryValuesElement = new TextValueElement(m_Target);
		return new BrickIconPatternVM(m_TargetIcon, m_UIAbilityData.PatternData, title, secondaryValuesElement, null, null, IconPatternMode.IconMode);
	}

	private void AddDescription(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		string description = string.Empty;
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
			description = m_ShortDescriptionText;
			break;
		case TooltipTemplateType.Info:
			description = m_LongDescriptionText;
			break;
		}
		description = TooltipTemplateUtils.AggregateDescription(description, TooltipTemplateUtils.GetAdditionalDescription(BlueprintAbility));
		if (!string.IsNullOrEmpty(description))
		{
			bricks.Add(new BrickTextVM(description, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, Caster));
		}
	}

	private void FindAppliedModifiers(BlueprintAbility blueprintAbility)
	{
		MechanicEntity fallbackCaster = FallbackCaster;
		if (fallbackCaster == null)
		{
			return;
		}
		PartAbilityModifiers optional = fallbackCaster.GetOptional<PartAbilityModifiers>();
		if (optional != null)
		{
			m_AppliedModifiers = (from modifier in optional.AddedModifiers
				where (modifier.Ability == null) ? blueprintAbility.Tags.Contains(modifier.AbilityTag) : (modifier.Ability == blueprintAbility)
				select modifier into m
				select m.Modifier).ToList();
		}
	}

	private void AddAbilityModificationsDescription(List<ITooltipBrick> bricks)
	{
		foreach (BlueprintAbilityModifier appliedModifier in m_AppliedModifiers)
		{
			TooltipTemplateLevelUpModifier tooltipTemplateLevelUpModifier = new TooltipTemplateLevelUpModifier(appliedModifier, null, FallbackCaster);
			tooltipTemplateLevelUpModifier.Prepare(TooltipTemplateType.Tooltip);
			foreach (ITooltipBrick item in tooltipTemplateLevelUpModifier.GetHeader(TooltipTemplateType.Info))
			{
				bricks.Add(item);
			}
			foreach (ITooltipBrick item2 in tooltipTemplateLevelUpModifier.GetBody(TooltipTemplateType.Info))
			{
				bricks.Add(item2);
			}
			bricks.Add(new BrickSeparatorVM(TooltipBrickElementType.Small));
		}
	}
}
