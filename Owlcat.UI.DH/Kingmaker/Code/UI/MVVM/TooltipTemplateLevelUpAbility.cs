using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.MVVM.Tooltip.Bricks.Items;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Bricks;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Features.Scaling.Utility;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Utility.UnitDescription;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpAbility : TooltipBaseTemplate
{
	public readonly BlueprintAbility BlueprintAbility;

	public readonly BlueprintItem SourceItem;

	public readonly MechanicEntity Caster;

	private AbilityData m_AbilityData;

	private string m_Name = string.Empty;

	private Sprite m_Icon;

	private string m_Type = string.Empty;

	private string m_Cost = string.Empty;

	private int? m_IntCost;

	private readonly string m_Level = string.Empty;

	private string m_Veil = string.Empty;

	private string m_Target = string.Empty;

	private Sprite m_TargetIcon;

	private string m_Cooldown = string.Empty;

	private string m_EndTurn = string.Empty;

	private string m_AttackAbilityGroupCooldown = string.Empty;

	private string m_ShortDescriptionText = string.Empty;

	private string m_LongDescriptionText = string.Empty;

	private string m_SpellDescriptor = string.Empty;

	private string m_ActionTime = string.Empty;

	private readonly UnitDescription.UIDamageInfo[] m_DamageInfo;

	private readonly string m_AutoCastHint = string.Empty;

	private UIUtilityItem.UIAbilityData m_UIAbilityData;

	private bool m_IsReload;

	private readonly ItemEntityWeapon m_Weapon;

	private bool m_IsOnTimeInBattleAbility;

	private bool m_IsScreenWindowTooltip;

	private List<BlueprintAbilityModifier> m_ApliedModifiers = new List<BlueprintAbilityModifier>();

	private List<Ability> m_AbilitiesWillBeLost = new List<Ability>();

	private LevelUpManager m_LevelUpManager;

	private ScalingInfo? m_ScalingInfo;

	public AbilityData AbilityData => m_AbilityData;

	private bool IsWeaponAbility => SourceItem is BlueprintItemWeapon;

	private MechanicEntity m_Caster
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

	public TooltipTemplateLevelUpAbility(BlueprintAbility blueprintAbility, BlueprintItem sourceItem = null, MechanicEntity caster = null, bool isScreenWindowTooltip = false, LevelUpManager levelUpManager = null)
	{
		BlueprintAbility = blueprintAbility;
		SourceItem = sourceItem;
		Caster = caster;
		m_IsScreenWindowTooltip = isScreenWindowTooltip;
		m_LevelUpManager = levelUpManager;
	}

	public TooltipTemplateLevelUpAbility(AbilityData abilityData, bool isScreenWindowTooltip = false)
	{
		m_AbilityData = abilityData;
		m_DamageInfo = null;
		BlueprintAbility = abilityData.Blueprint.OriginalBlueprint;
		Caster = abilityData.Caster;
		SourceItem = abilityData.SourceItem?.Blueprint;
		m_Weapon = abilityData.SourceItem as ItemEntityWeapon;
		m_IsScreenWindowTooltip = isScreenWindowTooltip;
	}

	private void FillBlueprintAbilityData(BlueprintAbility blueprintAbility)
	{
		try
		{
			BlueprintItemWeapon blueprintItem = SourceItem as BlueprintItemWeapon;
			m_Name = blueprintAbility.Name;
			m_Icon = blueprintAbility.Icon;
			m_Type = GetAbilityType(blueprintAbility);
			m_Target = UIUtilityAbilities.GetAbilityTarget(blueprintAbility, blueprintItem);
			m_TargetIcon = UIUtilityAbilities.GetTargetImage(blueprintAbility);
			m_Cooldown = blueprintAbility.CooldownRounds.ToString();
			m_IsOnTimeInBattleAbility = CheckOneTimeInBattleAbility(blueprintAbility);
			m_EndTurn = GetEndTurn(blueprintAbility);
			m_AttackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(blueprintAbility);
			m_ShortDescriptionText = blueprintAbility.GetShortenedDescription();
			m_LongDescriptionText = blueprintAbility.Description;
			m_SpellDescriptor = UIUtilityAbilities.GetSpellDescriptorsText(blueprintAbility);
			m_ActionTime = UIUtilityAbilities.GetAbilityActionText(blueprintAbility);
			m_UIAbilityData = UIUtilityItem.GetUIAbilityData(blueprintAbility, blueprintItem, Caster);
			m_IsReload = UIUtilityItem.IsReload(blueprintAbility);
			FindAppliedModifiers(blueprintAbility);
			m_AbilityData = Caster?.GetAbilityData(blueprintAbility) ?? new AbilityData(blueprintAbility, m_Caster, 0, m_ApliedModifiers);
			m_ScalingInfo = m_AbilityData?.GetScaling() ?? blueprintAbility.GetScaling(m_Caster);
			m_IntCost = m_AbilityData?.CalculateActionPointCost();
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {blueprintAbility.name}: {arg}");
		}
	}

	private void FillAbilityDataInfo(AbilityData abilityData)
	{
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Caster;
				m_Name = abilityData.Name;
				m_Icon = abilityData.Icon;
				m_Type = GetAbilityType(abilityData.Blueprint.OriginalBlueprint);
				m_Veil = GetVeil(abilityData);
				m_EndTurn = GetEndTurn(abilityData.Blueprint.OriginalBlueprint);
				m_AttackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(abilityData.Blueprint.OriginalBlueprint);
				m_Target = UIUtilityAbilities.GetAbilityTarget(abilityData);
				m_TargetIcon = UIUtilityAbilities.GetTargetImage(abilityData.Blueprint.OriginalBlueprint);
				m_Cooldown = abilityData.Blueprint.CooldownRounds.ToString();
				m_IsOnTimeInBattleAbility = CheckOneTimeInBattleAbility(abilityData.Blueprint.OriginalBlueprint);
				m_ShortDescriptionText = abilityData.ShortenedDescription;
				m_LongDescriptionText = abilityData.Description;
				m_SpellDescriptor = UIUtilityAbilities.GetSpellDescriptorsText(abilityData.Blueprint.OriginalBlueprint);
				m_ActionTime = UIUtilityAbilities.GetAbilityActionText(abilityData);
				m_UIAbilityData = UIUtilityItem.GetUIAbilityData(abilityData.Blueprint.OriginalBlueprint, abilityData.Weapon);
				m_IsReload = UIUtilityItem.IsReload(abilityData);
				m_AbilitiesWillBeLost = UIUtilityAbilities.TryGetAbilitiesWillBeLost(abilityData);
				FindAppliedModifiers(abilityData.Blueprint.OriginalBlueprint);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {abilityData?.Blueprint?.name}: {arg}");
		}
	}

	private void AddCantUseInfo(List<ITooltipBrick> result)
	{
		if (BlueprintAbility == null || Caster == null)
		{
			return;
		}
		foreach (AbilityCasterHasNoFacts component3 in BlueprintAbility.GetComponents<AbilityCasterHasNoFacts>())
		{
			if (component3 == null || component3.HideInUI)
			{
				continue;
			}
			foreach (BlueprintUnitFact fact in component3.Facts)
			{
				if (Caster.Facts.Contains(fact))
				{
					string arg = GenerateLink(fact);
					string cantUseLabel = string.Format(ReasonStrings.Instance.CantUseRemove, arg);
					result.Add(new TooltipBrickCantUse(cantUseLabel));
				}
			}
		}
		foreach (AbilityCasterHasFacts component4 in BlueprintAbility.GetComponents<AbilityCasterHasFacts>())
		{
			if (component4 == null || component4.HideInUI)
			{
				continue;
			}
			foreach (BlueprintUnitFact fact2 in component4.Facts)
			{
				if (!Caster.Facts.Contains(fact2))
				{
					string arg2 = GenerateLink(fact2);
					string cantUseLabel2 = string.Format(ReasonStrings.Instance.CantUseNeed, arg2);
					result.Add(new TooltipBrickCantUse(cantUseLabel2));
				}
			}
		}
		AbilityCasterStatGreaterOrEqual10 component = BlueprintAbility.GetComponent<AbilityCasterStatGreaterOrEqual10>();
		if (component != null && !component.IsCasterRestrictionPassed(Caster))
		{
			string text = LocalizedTexts.Instance.Stats.GetText(component.Stat);
			string cantUseLabel3 = string.Format(ConfigRoot.Instance.LocalizedTexts.Reasons.NotEnoughStat, text);
			result.Add(new TooltipBrickCantUse(cantUseLabel3));
		}
		AbilitySpecialMoraleAction component2 = BlueprintAbility.GetComponent<AbilitySpecialMoraleAction>();
		if (component2 != null)
		{
			BlueprintEncyclopediaGlossaryEntry blueprintEncyclopediaGlossaryEntry = component2.MoralePhaseType switch
			{
				MoraleAbilityType.Heroic => UIUtilityEncyclopedy.GetGlossaryEntry("MoraleHeroic"), 
				MoraleAbilityType.Broken => UIUtilityEncyclopedy.GetGlossaryEntry("MoraleBroken"), 
				MoraleAbilityType.Both => null, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			if (blueprintEncyclopediaGlossaryEntry != null)
			{
				string arg3 = "<b><color=#" + UIConfig.Instance.LinkColor.HTML() + "><link=\"Encyclopedia:" + blueprintEncyclopediaGlossaryEntry.name + "\">" + blueprintEncyclopediaGlossaryEntry.Title.Text + "</link></color></b>";
				string cantUseLabel4 = string.Format(ConfigRoot.Instance.LocalizedTexts.Reasons.MoraleShouldBe, arg3);
				result.Add(new TooltipBrickCantUse(cantUseLabel4));
			}
		}
	}

	private string GenerateLink(BlueprintUnitFact fact)
	{
		return "<b><color=#" + UIConfig.Instance.LinkColor.HTML() + "><link=\"f:" + fact.AssetGuid + "\">" + fact.LocalizedName.Text + "</link></color></b>";
	}

	private bool CheckOneTimeInBattleAbility(BlueprintAbility blueprintAbility)
	{
		return blueprintAbility.GetComponent<Cooldown>()?.UntilEndOfCombat ?? false;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return AddAbilityHeader();
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		using (GameLogContext.Scope)
		{
			GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
			GameLogContext.DescriptionFactBlueprint = BlueprintAbility;
			GameLogContext.DescriptionAbility = m_AbilityData;
			List<ITooltipBrick> list = new List<ITooltipBrick>();
			AddDescription(list, type);
			AddAbilityModificationsDescription(list);
			return list;
		}
	}

	private void AddAbilitiesWillBeLost(List<ITooltipBrick> bricks)
	{
		if (m_AbilitiesWillBeLost == null || m_AbilitiesWillBeLost.Empty())
		{
			return;
		}
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.AbilitiesWillBeLostHeader, TooltipTitleType.H4));
		bricks.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.AbilitiesWillBeLostDescription));
		foreach (Ability item in m_AbilitiesWillBeLost)
		{
			bricks.Add(new TooltipBrickFeature(item));
		}
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Tooltip && !string.IsNullOrEmpty(m_AutoCastHint))
		{
			yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
			yield return new TooltipBrickText(m_AutoCastHint, TooltipTextType.Italic);
		}
	}

	private ITooltipBrick AddAbilityHeader()
	{
		List<string> values = BlueprintAbility.Tags.Select((BpRef<BlueprintAbilityTag> tag) => tag.Blueprint.Reference().Blueprint.Name.Text).ToList();
		TooltipLevelUpAbilityData abilityData = new TooltipLevelUpAbilityData(null, GetTarget());
		string value = (m_IntCost.HasValue ? string.Format(UIStrings.Instance.Tooltips.AbilityAPCost.Text, m_IntCost) : null);
		string name = m_Name;
		string subtitle = string.Join(", ", values);
		Sprite icon = m_Icon;
		return new TooltipBrickLevelUpHeader(new TooltipBrickLevelUpFeatureData(name, null, null, subtitle, value, null, icon), null, abilityData);
	}

	private string GetCost(BlueprintAbility blueprintAbility)
	{
		int actionPointCost = blueprintAbility.ActionPointCost;
		return GetCost(actionPointCost, blueprintAbility.AbilityParamsSource, blueprintAbility.GetVeilDamage());
	}

	private string GetCost(AbilityData abilityData)
	{
		int cost = abilityData.CalculateActionPointCost();
		return GetCost(cost, abilityData.Blueprint.AbilityParamsSource, abilityData.Blueprint.GetVeilDamage());
	}

	private string GetVeil(AbilityData abilityData)
	{
		if (abilityData.Blueprint.AbilityParamsSource != WarhammerAbilityParamsSource.PsychicPower)
		{
			return string.Empty;
		}
		return UIStrings.Instance.Tooltips.MajorVeilDegradation.Text;
	}

	private string GetCost(int cost, WarhammerAbilityParamsSource warhammerAbilityParamsSource, int veilDamage)
	{
		if (warhammerAbilityParamsSource != WarhammerAbilityParamsSource.PsychicPower)
		{
			return string.Format(UIStrings.Instance.Tooltips.CostAP, cost);
		}
		return string.Format(UIStrings.Instance.Tooltips.PsychicPowerCostAP, cost, veilDamage);
	}

	private string GetAbilityType(BlueprintAbility blueprintAbility)
	{
		if (blueprintAbility.AbilityParamsSource != WarhammerAbilityParamsSource.PsychicPower)
		{
			return LocalizedTexts.Instance.AbilityTypes.GetText(blueprintAbility.Type);
		}
		return UIStrings.Instance.Tooltips.PsykerPower;
	}

	private string GetEndTurn(BlueprintAbility blueprintAbility)
	{
		EndTurn component = blueprintAbility.GetComponent<EndTurn>();
		if (component != null)
		{
			return component.clearMPInsteadOfEndingTurn ? UIStrings.Instance.Tooltips.SpendAllMovementPoints : UIStrings.Instance.Tooltips.EndsTurn;
		}
		return string.Empty;
	}

	private string GetAttackAbilityGroupCooldown(BlueprintAbility blueprintAbility)
	{
		bool flag = false;
		foreach (BlueprintAbilityGroup abilityGroup in blueprintAbility.AbilityGroups)
		{
			if (abilityGroup.NameSafe() == "WeaponAttackAbilityGroup")
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return string.Empty;
		}
		return UIStrings.Instance.Tooltips.AttackAbilityGroupCooldown;
	}

	private void AddTarget(List<ITooltipBrick> bricks)
	{
		TooltipBrickIconPattern target = GetTarget();
		if (target != null)
		{
			bricks.Add(target);
		}
	}

	private TooltipBrickIconPattern GetTarget()
	{
		if (m_IsReload)
		{
			return null;
		}
		if (string.IsNullOrEmpty(m_Target) || m_TargetIcon == null)
		{
			return null;
		}
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = string.Empty
		};
		TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = m_Target
		};
		return new TooltipBrickIconPattern(m_TargetIcon, m_UIAbilityData.PatternData, titleValues, secondaryValues, null, null, IconPatternMode.IconMode);
	}

	private void AddCooldown(List<ITooltipBrick> bricks)
	{
		if ((!string.IsNullOrEmpty(m_Cooldown) && !(m_Cooldown == "0")) || m_IsOnTimeInBattleAbility)
		{
			TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Cooldown)
			};
			TooltipBrickIconPattern.TextFieldValues textFieldValues = new TooltipBrickIconPattern.TextFieldValues();
			if (m_IsOnTimeInBattleAbility)
			{
				textFieldValues.Text = UIUtilityText.WrapWithWeight(UIStrings.Instance.TurnBasedTexts.CanUseOneTimeInCombat, TextFontWeight.SemiBold);
			}
			else
			{
				textFieldValues.Text = UIStrings.Instance.TurnBasedTexts.Rounds;
				textFieldValues.Value = m_Cooldown;
			}
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Cooldown, null, titleValues, textFieldValues, null, null, IconPatternMode.IconMode));
		}
	}

	private void AddHitChances(List<ITooltipBrick> bricks)
	{
		if (m_IsReload || !IsWeaponAbility)
		{
			return;
		}
		if (m_UIAbilityData.IsRange)
		{
			if (m_UIAbilityData.IsScatter)
			{
				AddScatterHitChances(bricks);
			}
			else
			{
				AddRangeHitChances(bricks);
			}
		}
		else if (m_UIAbilityData.HitChance.HasValue)
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Tooltips.HitChances, UIUtilityText.AddPercentTo(m_UIAbilityData.HitChance.Value)));
		}
	}

	private void AddRangeHitChances(List<ITooltipBrick> bricks)
	{
		if (m_UIAbilityData.HitChance.HasValue)
		{
			TextFieldParams textParams = new TextFieldParams
			{
				FontStyles = TMPro.FontStyles.Bold
			};
			TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIStrings.Instance.Tooltips.HitChances
			};
			TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIStrings.Instance.Tooltips.HitChancesEffectiveDistance,
				Value = UIUtilityText.AddPercentTo(m_UIAbilityData.HitChance.Value),
				TextParams = textParams
			};
			TooltipBrickIconPattern.TextFieldValues tertiaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIStrings.Instance.Tooltips.HitChancesMaxDistance,
				Value = UIUtilityText.AddPercentTo(m_UIAbilityData.HitChance.Value / 2),
				TextParams = textParams
			};
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.HitChances, null, titleValues, secondaryValues, tertiaryValues, null, IconPatternMode.IconMode));
		}
	}

	private void AddScatterHitChances(List<ITooltipBrick> bricks)
	{
		UIUtilityItem.UIScatterHitChanceData scatterHitChanceData = m_UIAbilityData.ScatterHitChanceData;
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.HitChances, TooltipTitleType.H1));
		bricks.Add(new TooltipBricksGroupStart());
		bricks.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.HitChancesEffectiveDistance, TooltipTextType.Bold));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterMainLineClose, UIUtilityText.AddPercentTo(scatterHitChanceData.MainLineClose)));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterClose, UIUtilityText.AddPercentTo(scatterHitChanceData.ScatterClose)));
		bricks.Add(new TooltipBricksGroupEnd());
		bricks.Add(new TooltipBricksGroupStart());
		bricks.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.HitChancesMaxDistance, TooltipTextType.Bold));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterMainLine, UIUtilityText.AddPercentTo(scatterHitChanceData.MainLine)));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterNear, UIUtilityText.AddPercentTo(scatterHitChanceData.ScatterNear)));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterFar, UIUtilityText.AddPercentTo(scatterHitChanceData.ScatterFar)));
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void AddUIProperties(List<ITooltipBrick> bricks)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			if (!m_UIAbilityData.UIProperties.Any())
			{
				return;
			}
			bricks.Add(new TooltipBricksGroupStart());
			foreach (UIProperty uIProperty in m_UIAbilityData.UIProperties)
			{
				string title = ((!string.IsNullOrEmpty(uIProperty.Name)) ? uIProperty.Name : uIProperty.NameType.GetLocalizedName());
				bricks.Add(new TooltipBrickCalculatedFormula(title, uIProperty.Description, uIProperty.PropertyValue?.ToString() ?? ((string)UIStrings.Instance.SettingsUI.Value), !uIProperty.PropertyValue.HasValue));
			}
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	private void AddDamageInfo(List<ITooltipBrick> bricks)
	{
		if (!m_IsReload)
		{
			string baseDamageText = m_UIAbilityData.BaseDamageText;
			string value = UIUtilityText.AddPercentTo(m_UIAbilityData.Penetration);
			if (!string.IsNullOrEmpty(baseDamageText))
			{
				Sprite damage = UIConfig.Instance.UIIcons.Damage;
				Sprite penetration = UIConfig.Instance.UIIcons.Penetration;
				StatData leftStat = new StatData(baseDamageText, TooltipElement.Damage, damage, ComparisonResult.Equal, StatData.StatHighlight.Default);
				StatData rightStat = new StatData(value, TooltipElement.Penetration, penetration, ComparisonResult.Equal, StatData.StatHighlight.Default);
				bricks.Add(new TooltipBrickTwoColumnsStat(leftStat, rightStat));
			}
		}
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
			description = UIUtilityText.UpdateDescriptionWithUIProperties(description, Caster);
			description = UIUtilityText.UpdateDescriptionWithUICommonProperties(description, Caster);
			bricks.Add(new TooltipBrickText(description, TooltipTextType.Paragraph));
		}
	}

	private void AddMovementActionVeil(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (type == TooltipTemplateType.Info)
		{
			AddEndTurnInfo(list);
			AddAttckAbilityGroupCooldown(list);
			AddVeilDegradation(list);
		}
		else
		{
			string leftLine = string.Empty;
			Sprite sprite = null;
			if (!string.IsNullOrEmpty(m_EndTurn))
			{
				sprite = UIConfig.Instance.UIIcons.TooltipIcons.MoveEndPoints;
				leftLine = UIStrings.Instance.Tooltips.SpendAllMovementPointsShort;
			}
			string middleLine = string.Empty;
			Sprite sprite2 = null;
			if (!string.IsNullOrEmpty(m_AttackAbilityGroupCooldown))
			{
				sprite2 = UIConfig.Instance.UIIcons.TooltipIcons.ActionEndPoints;
				middleLine = UIStrings.Instance.Tooltips.AttackAbilityGroupCooldownShort;
			}
			string rightLine = string.Empty;
			Sprite sprite3 = null;
			if (!string.IsNullOrEmpty(m_Veil))
			{
				rightLine = UIStrings.Instance.Tooltips.IncreaseVeilDegradationShort;
				sprite3 = UIConfig.Instance.UIIcons.TooltipIcons.Vail;
			}
			if (sprite != null || sprite2 != null || sprite3 != null)
			{
				TextFieldParams textFieldParams = new TextFieldParams
				{
					FontColor = UIConfig.Instance.TooltipColors.Default,
					FontStyles = TMPro.FontStyles.Strikethrough
				};
				list.Add(new TooltipBrickTripleText(leftLine, middleLine, rightLine, sprite, sprite2, sprite3, textFieldParams, textFieldParams, textFieldParams));
			}
		}
		if (list.Count > 0)
		{
			bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
			bricks.AddRange(list);
		}
	}

	private void AddEndTurnInfo(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_EndTurn))
		{
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.MoveEndPoints, null, m_EndTurn, null, null, null, IconPatternMode.IconMode));
		}
	}

	private void AddAttckAbilityGroupCooldown(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_AttackAbilityGroupCooldown))
		{
			if (bricks.Count > 0)
			{
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
			}
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.ActionEndPoints, null, m_AttackAbilityGroupCooldown, null, null, null, IconPatternMode.IconMode));
		}
	}

	private void AddVeilDegradation(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_Veil))
		{
			if (bricks.Count > 0)
			{
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
			}
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Vail, null, m_Veil, null, null, null, IconPatternMode.IconMode));
		}
	}

	private void FindAppliedModifiers(BlueprintAbility blueprintAbility)
	{
		if (Caster != null)
		{
			PartAbilityModifiers orCreate = Caster.GetOrCreate<PartAbilityModifiers>();
			m_ApliedModifiers = (from modifier in orCreate.AddedModifiers
				where (modifier.Ability == null) ? blueprintAbility.Tags.Contains(modifier.AbilityTag) : (modifier.Ability == blueprintAbility)
				select modifier into m
				select m.Modifier).ToList();
		}
	}

	private void AddAbilityModificationsDescription(List<ITooltipBrick> bricks)
	{
		foreach (BlueprintAbilityModifier apliedModifier in m_ApliedModifiers)
		{
			TooltipTemplateLevelUpModifier tooltipTemplateLevelUpModifier = new TooltipTemplateLevelUpModifier(apliedModifier, null, (BaseUnitEntity)m_Caster);
			tooltipTemplateLevelUpModifier.Prepare(TooltipTemplateType.Tooltip);
			foreach (ITooltipBrick item in tooltipTemplateLevelUpModifier.GetHeader(TooltipTemplateType.Info))
			{
				bricks.Add(item);
			}
			foreach (ITooltipBrick item2 in tooltipTemplateLevelUpModifier.GetBody(TooltipTemplateType.Info))
			{
				bricks.Add(item2);
			}
			bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
		}
	}
}
