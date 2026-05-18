using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.Gameplay.Components.Features;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateAbility : TooltipBaseTemplate
{
	private const float BrickBigSpaceHeight = 25f;

	private const float BrickSmallSpaceHeight = 10f;

	private readonly AbilityData m_AbilityData;

	private readonly BlueprintItem m_SourceItem;

	private readonly string m_AutoCastHint = string.Empty;

	private string m_Name = string.Empty;

	private string m_Type = string.Empty;

	private IReadOnlyList<string> m_Tags;

	private string m_Veil = string.Empty;

	private string m_Target = string.Empty;

	private string m_EndTurn = string.Empty;

	private string m_AttackAbilityGroupCooldown = string.Empty;

	private string m_ShortDescriptionText = string.Empty;

	private string m_LongDescriptionText = string.Empty;

	private bool m_HasCasterRestrictions;

	private bool m_CasterRestrictionsPassed;

	private Sprite m_Icon;

	private Sprite m_TargetIcon;

	private UIUtilityItem.UIAbilityData m_UIAbilityData;

	private int m_ActionPointsCost;

	private int m_AttackCount;

	private int m_AttackRangeCells;

	private IEnumerable<StatType> m_ScalingStats;

	private readonly ItemEntityWeapon m_Weapon;

	private BlueprintItemWeapon m_BlueprintItemWeapon;

	private IReadOnlyList<BlueprintAbilityModifier> m_AppliedModifiers;

	private BlueprintAbilityModifier m_ManualModifier;

	private List<Ability> m_AbilitiesWillBeLost = new List<Ability>();

	private readonly bool m_ShowModifiedAPCost;

	protected readonly MechanicEntity m_Caster;

	public readonly BlueprintAbility BlueprintAbility;

	public override void Prepare(TooltipTemplateType type)
	{
		if (m_AbilityData != null)
		{
			FillAbilityDataInfo(m_AbilityData);
		}
		else if (BlueprintAbility != null)
		{
			FillBlueprintAbilityData(BlueprintAbility);
		}
	}

	public TooltipTemplateAbility(BlueprintAbility blueprintAbility, BlueprintItem sourceItem = null, MechanicEntity caster = null)
	{
		BlueprintAbility = blueprintAbility;
		m_SourceItem = sourceItem;
		m_Caster = caster;
	}

	public TooltipTemplateAbility(AbilityData abilityData, bool showModifiedAPCost = false)
	{
		ContentSpacing = 0f;
		m_AbilityData = abilityData;
		BlueprintAbility = abilityData.Blueprint.OriginalBlueprint;
		m_Caster = abilityData.Caster;
		m_SourceItem = abilityData.SourceItem?.Blueprint;
		m_Weapon = abilityData.SourceItem as ItemEntityWeapon;
		m_ShowModifiedAPCost = showModifiedAPCost;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (!m_CasterRestrictionsPassed)
		{
			ITooltipBrick casterRestrictionsBrick = GetCasterRestrictionsBrick();
			if (casterRestrictionsBrick != null)
			{
				yield return casterRestrictionsBrick;
			}
		}
		yield return GetAbilityHeaderBrick();
	}

	private ITooltipBrick GetCasterRestrictionsBrick()
	{
		if (BlueprintAbility == null || m_Caster == null || !m_HasCasterRestrictions)
		{
			return null;
		}
		return new BrickAbilityRestrictionsVM(BlueprintAbility, m_Caster);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_Tags.Count > 0)
		{
			list.Add(new BrickAbilityTagsVM(m_Tags));
		}
		list.Add(new BrickAbilitySeparatorVM());
		AddDamageInfo(list);
		AddAbilityRange(list);
		AddAttacksCount(list);
		AddTargetInfo(list);
		AddScalingCharacteristics(list);
		AddBodyWeaponTags(list);
		AddDescription(list, type);
		AddAppliedModifiers(list);
		AddEndTurnInfo(list);
		AddAttackAbilityGroupCooldown(list);
		AddVeilDegradation(list);
		AddAbilitiesWillBeLost(list);
		if (m_HasCasterRestrictions && m_CasterRestrictionsPassed)
		{
			ITooltipBrick casterRestrictionsBrick = GetCasterRestrictionsBrick();
			if (casterRestrictionsBrick != null)
			{
				list.Add(new BrickSpaceVM(25f));
				list.Add(casterRestrictionsBrick);
			}
		}
		return list;
	}

	private void FillBlueprintAbilityData(BlueprintAbility blueprintAbility)
	{
		try
		{
			m_BlueprintItemWeapon = m_SourceItem as BlueprintItemWeapon;
			m_Name = blueprintAbility.Name;
			m_Icon = blueprintAbility.Icon;
			m_Type = GetAbilityType(blueprintAbility);
			m_Tags = (from t in blueprintAbility.Tags
				where !t.Blueprint.Name.IsEmpty()
				select t.Blueprint.Name.Text).ToList();
			m_Target = UIUtilityAbilities.GetAbilityTarget(blueprintAbility, m_BlueprintItemWeapon);
			m_EndTurn = GetEndTurn(blueprintAbility);
			m_AttackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(blueprintAbility);
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				m_ShortDescriptionText = blueprintAbility.GetShortenedDescription();
				m_LongDescriptionText = blueprintAbility.Description;
			}
			m_UIAbilityData = UIUtilityItem.GetUIAbilityData(blueprintAbility, m_BlueprintItemWeapon, m_Caster);
			m_ScalingStats = blueprintAbility.GetScalingStats();
			m_HasCasterRestrictions = blueprintAbility.HasCasterRestrictions(m_Caster, out m_CasterRestrictionsPassed);
			if (m_BlueprintItemWeapon != null)
			{
				m_AttackRangeCells = m_BlueprintItemWeapon.AttackRange;
			}
			m_AppliedModifiers = blueprintAbility.GetAppliedModifiers(m_Caster, out m_ManualModifier);
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
				using (EvalContext.Build().Ability(abilityData).Push())
				{
					GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
					GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
					m_Name = abilityData.Name;
					m_Icon = abilityData.Icon;
					m_Type = GetAbilityType(abilityData.Blueprint.OriginalBlueprint);
					m_Tags = (from t in abilityData.Blueprint.Tags
						where !t.Blueprint.Name.IsEmpty()
						select t.Blueprint.Name.Text).ToList();
					m_ActionPointsCost = (m_ShowModifiedAPCost ? abilityData.CalculateActionPointCost() : abilityData.GetBaseActionPointCost());
					m_Veil = GetVeil(abilityData);
					m_EndTurn = GetEndTurn(abilityData.Blueprint.OriginalBlueprint);
					m_AttackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(abilityData.Blueprint.OriginalBlueprint);
					m_Target = UIUtilityAbilities.GetAbilityTarget(abilityData);
					m_TargetIcon = UIUtilityAbilities.GetTargetImage(abilityData.Blueprint.OriginalBlueprint);
					m_ShortDescriptionText = abilityData.ShortenedDescription;
					m_LongDescriptionText = abilityData.Description;
					m_UIAbilityData = UIUtilityItem.GetUIAbilityData(abilityData.Blueprint.OriginalBlueprint, abilityData.Weapon);
					m_AbilitiesWillBeLost = UIUtilityAbilities.TryGetAbilitiesWillBeLost(abilityData);
					m_AttackCount = abilityData.BurstAttacksCount;
					m_ScalingStats = abilityData.Blueprint.OriginalBlueprint.GetScalingStats();
					m_HasCasterRestrictions = abilityData.Blueprint.OriginalBlueprint.HasCasterRestrictions(m_Caster, out m_CasterRestrictionsPassed);
					if (abilityData.Weapon != null)
					{
						m_BlueprintItemWeapon = abilityData.Weapon.Blueprint;
						m_AttackRangeCells = m_BlueprintItemWeapon.AttackRange;
					}
					m_AppliedModifiers = abilityData.Blueprint.OriginalBlueprint.GetAppliedModifiers(m_Caster, out m_ManualModifier);
				}
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {abilityData?.Blueprint?.name}: {arg}");
		}
	}

	private void AddAppliedModifiers(List<ITooltipBrick> bricks)
	{
		if (m_AppliedModifiers != null && m_AppliedModifiers.Count >= 1)
		{
			bricks.Add(new BrickSpaceVM(25f));
			bricks.Add(new BrickAbilityModifiersVM(m_AppliedModifiers, m_ManualModifier, m_Caster));
		}
	}

	private void AddAbilitiesWillBeLost(List<ITooltipBrick> bricks)
	{
		if (m_AbilitiesWillBeLost == null || m_AbilitiesWillBeLost.Count < 1)
		{
			return;
		}
		LocalizedString abilitiesWillBeLostDescription = UIStrings.Instance.Tooltips.AbilitiesWillBeLostDescription;
		Sprite abilityRestrictionIcon = UIConfig.Instance.AbilityTooltipConfig.AbilityRestrictionIcon;
		Color restrictionColor = UIConfig.Instance.AbilityTooltipConfig.RestrictionColor;
		bricks.Add(new BrickSpaceVM(25f));
		bricks.Add(new BrickAbilityDescriptionVM(abilitiesWillBeLostDescription, abilityRestrictionIcon, restrictionColor));
		bricks.Add(new BrickSpaceVM(10f));
		foreach (Ability item in m_AbilitiesWillBeLost)
		{
			bricks.Add(new BrickAbilityFeatureVM(item, m_Caster));
		}
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Tooltip && !string.IsNullOrEmpty(m_AutoCastHint))
		{
			yield return new BrickSeparatorVM(TooltipBrickElementType.Medium);
			yield return new BrickTextVM(m_AutoCastHint, TooltipTextType.Italic);
		}
	}

	private ITooltipBrick GetAbilityHeaderBrick()
	{
		if (m_AbilityData != null)
		{
			return new BrickAbilityHeaderVM((!string.IsNullOrEmpty(m_Name)) ? m_Name : (m_UIAbilityData?.Weapon?.Name ?? string.Empty), m_Type, m_Icon, m_ActionPointsCost).SetModifierIcon(m_ManualModifier?.Tags?.FirstOrDefault()?.AbilityIcon);
		}
		return GetAbilityBlueprintHeader();
	}

	private ITooltipBrick GetAbilityBlueprintHeader()
	{
		TextEntity title = new TextEntity(BlueprintAbility.Name, TextFieldParams.Bold);
		List<string> values = BlueprintAbility.Tags.Select((BpRef<BlueprintAbilityTag> tag) => tag.Blueprint.Reference().Blueprint.Name.Text).ToList();
		TextValueElement secondaryValuesElement = new TextValueElement(string.Join(", ", values));
		TextValueElement tertiaryValuesElement = null;
		if (m_UIAbilityData.BurstAttacksCount > 1)
		{
			tertiaryValuesElement = new TextValueElement(string.Format(UIStrings.Instance.Tooltips.ShotsCount, m_UIAbilityData.BurstAttacksCount.ToString()));
		}
		return new BrickIconPatternVM(BlueprintAbility.Icon, null, title, secondaryValuesElement, tertiaryValuesElement);
	}

	private int GetVeilDamage(AbilityData abilityData)
	{
		return abilityData.GetPredictedVeilDelta();
	}

	private string GetVeil(AbilityData abilityData)
	{
		int veilDamage = GetVeilDamage(abilityData);
		if (veilDamage == 0)
		{
			return string.Empty;
		}
		GameLogContext.Count = veilDamage;
		return UIStrings.Instance.Tooltips.VeilDegradation;
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

	private void AddTargetInfo(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_Target) || !(m_TargetIcon == null) || m_UIAbilityData.PatternData != null)
		{
			bricks.Add(new BrickAbilityPatternVM(m_TargetIcon, m_Target, m_UIAbilityData.PatternData));
		}
	}

	private void AddScalingCharacteristics(List<ITooltipBrick> bricks)
	{
		if (m_Caster != null && m_ScalingStats != null && m_ScalingStats.Any())
		{
			bricks.Add(new BrickAbilityScalingStatsVM(m_ScalingStats, m_Caster));
		}
	}

	private void AddDamageInfo(List<ITooltipBrick> bricks)
	{
		if (m_BlueprintItemWeapon != null)
		{
			BrickAbilityWeaponDamageVM item = new BrickAbilityWeaponDamageVM(m_BlueprintItemWeapon, m_UIAbilityData.MinDamage, m_UIAbilityData.MaxDamage, m_Weapon);
			bricks.Add(item);
		}
	}

	private void AddAttacksCount(List<ITooltipBrick> bricks)
	{
		if (m_Weapon != null && m_AttackCount > 0)
		{
			bool num = !m_Weapon.Blueprint.IsRanged;
			string statName = (num ? UIUtilityTooltip.GetTooltipElementLabel(TooltipElement.RateOfFireMelee) : UIUtilityTooltip.GetTooltipElementLabel(TooltipElement.RateOfFire));
			Sprite statIcon = (num ? ConfigRoot.Instance.UIConfig.AbilityTooltipConfig.MeleeAttackRateIcon : ConfigRoot.Instance.UIConfig.AbilityTooltipConfig.RangedAttackRateIcon);
			bricks.Add(new BrickAbilityWeaponStatVM(statName, statIcon, m_AttackCount.ToString()));
		}
	}

	private void AddAbilityRange(List<ITooltipBrick> bricks)
	{
		if (!(m_AbilityData == null) && m_AttackRangeCells >= 1)
		{
			Sprite attackDistanceIcon = ConfigRoot.Instance.UIConfig.AbilityTooltipConfig.AttackDistanceIcon;
			LocalizedString abilityDistance = UIStrings.Instance.Tooltips.AbilityDistance;
			bricks.Add(new BrickAbilityWeaponStatVM(abilityDistance, attackDistanceIcon, m_AttackRangeCells.ToString()));
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
			bricks.Add(new BrickSpaceVM(25f));
			bricks.Add(new BrickFormattedDescriptionVM(description, m_Caster));
		}
	}

	private void AddEndTurnInfo(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_EndTurn))
		{
			Sprite abilityRestrictionIcon = UIConfig.Instance.AbilityTooltipConfig.AbilityRestrictionIcon;
			Color restrictionColor = UIConfig.Instance.AbilityTooltipConfig.RestrictionColor;
			bricks.Add(new BrickSpaceVM(25f));
			bricks.Add(new BrickAbilityDescriptionVM(m_EndTurn, abilityRestrictionIcon, restrictionColor));
		}
	}

	private void AddAttackAbilityGroupCooldown(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_AttackAbilityGroupCooldown))
		{
			Sprite abilityRestrictionIcon = UIConfig.Instance.AbilityTooltipConfig.AbilityRestrictionIcon;
			Color restrictionColor = UIConfig.Instance.AbilityTooltipConfig.RestrictionColor;
			bricks.Add(new BrickSpaceVM(25f));
			bricks.Add(new BrickAbilityDescriptionVM(m_AttackAbilityGroupCooldown, abilityRestrictionIcon, restrictionColor));
		}
	}

	private void AddVeilDegradation(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_Veil))
		{
			Sprite abilityPsykerIcon = UIConfig.Instance.AbilityTooltipConfig.AbilityPsykerIcon;
			Color psykerColor = UIConfig.Instance.AbilityTooltipConfig.PsykerColor;
			bricks.Add(new BrickSpaceVM(25f));
			bricks.Add(new BrickAbilityDescriptionVM(m_Veil, abilityPsykerIcon, psykerColor));
		}
	}

	private void AddBodyWeaponTags(List<ITooltipBrick> bricks)
	{
		if (!UIConfig.Instance.FeatureTagsConfig.ShowTagsDescriptions || m_BlueprintItemWeapon == null)
		{
			return;
		}
		foreach (WeaponTagUISettings weaponTag in m_BlueprintItemWeapon.WeaponTags)
		{
			if (!weaponTag.IsBodyIgnoreTag())
			{
				Sprite weaponTagIcon = UIConfig.Instance.FeatureTagsConfig.GetWeaponTagIcon(weaponTag);
				Color weaponMountColor = UIConfig.Instance.FeatureTagsConfig.GetWeaponMountColor(weaponTag);
				using (GameLogContext.Scope)
				{
					GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)(m_Weapon?.Owner ?? m_Caster);
					string tagName = UIUtilityItem.GetTagName(weaponTag);
					string tagDescription = UIUtilityItem.GetTagDescription(weaponTag);
					bricks.Add(new BrickSpaceVM(25f));
					bricks.Add(new BrickTagDescriptionVM(weaponTagIcon, weaponMountColor, tagName, tagDescription));
				}
			}
		}
	}
}
