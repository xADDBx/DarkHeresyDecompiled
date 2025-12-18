using System;
using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.Framework.GameLog;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Mechanics.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateBuff : TooltipBaseTemplate
{
	private EntityRef m_OverrideCaster;

	private bool m_IsConcentration;

	private string m_Name;

	private string m_OverrideName;

	private string m_Desc;

	private Sprite m_Icon;

	private Sprite m_OverrideIcon;

	private string m_OverrideSecondary;

	private readonly string m_Stacking;

	private readonly Buff m_Buff;

	private readonly BlueprintBuff m_BlueprintBuff;

	private string Name => m_OverrideName ?? m_Name;

	private Sprite Icon => m_OverrideIcon ?? m_Icon;

	public BlueprintBuff BlueprintBuff => m_Buff?.Blueprint ?? m_BlueprintBuff;

	public TooltipTemplateBuff(Buff buff, IEntity overrideCaster = null, bool isConcentration = false, Sprite overrideIcon = null, string overrideName = null, string overrideSecondary = null)
	{
		m_Buff = buff;
		m_IsConcentration = isConcentration;
		m_OverrideCaster = new EntityRef(overrideCaster);
		m_OverrideIcon = overrideIcon;
		m_OverrideName = overrideName;
		m_OverrideSecondary = overrideSecondary;
	}

	public TooltipTemplateBuff(BlueprintBuff blueprintBuff)
	{
		m_BlueprintBuff = blueprintBuff;
	}

	public override void Prepare(TooltipTemplateType type)
	{
		if (m_Buff != null)
		{
			FillAbilityDataInfo(m_Buff);
		}
		else if (m_BlueprintBuff != null)
		{
			FillBlueprintAbilityData(m_BlueprintBuff);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_IsConcentration)
		{
			list.Add(new TooltipBrickTitle(UIStrings.Instance.HUDTexts.ConcentrationHint, TooltipTitleType.H3));
			return list;
		}
		AddBuffHeader(list);
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_IsConcentration)
		{
			AddConcentration(list);
			list.Add(new TooltipBrickSeparator());
			AddBuffHeader(list);
		}
		AddDOT(list);
		AddSource(list);
		AddDuration(list);
		AddStacking(list);
		AddDescription(list);
		AddNonStackBonus(list);
		if (BlueprintBuff.IsCriticalEffect)
		{
			list.Add(new TooltipBrickSeparator());
			TooltipBrickText item = new TooltipBrickText(UIStrings.Instance.Tooltips.CriticalEffectHint, TooltipTextType.Italic | TooltipTextType.BrightColor);
			list.Add(item);
		}
		return list;
	}

	private void FillBlueprintAbilityData(BlueprintBuff blueprintBuff)
	{
		try
		{
			if (blueprintBuff != null)
			{
				m_Name = blueprintBuff.Name;
				m_Desc = blueprintBuff.Description;
				m_Icon = blueprintBuff.Icon;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {blueprintBuff?.name}: {arg}");
		}
	}

	private void FillAbilityDataInfo(Buff buff)
	{
		try
		{
			if (buff == null)
			{
				return;
			}
			using (GameLogContext.Scope)
			{
				GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)((IBuff)m_Buff).Caster;
				m_Name = buff.Name;
				m_Desc = buff.Description;
				m_Icon = buff.Icon;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {buff?.Blueprint?.name}: {arg}");
		}
	}

	private void AddBuffHeader(List<ITooltipBrick> bricks)
	{
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = Name,
			TextParams = new TextFieldParams
			{
				FontStyles = TMPro.FontStyles.Bold
			}
		};
		TooltipBrickIconPattern.TextFieldValues secondaryValues = null;
		if (!string.IsNullOrEmpty(m_OverrideSecondary))
		{
			secondaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = m_OverrideSecondary,
				TextParams = new TextFieldParams
				{
					FontStyles = TMPro.FontStyles.Italic
				}
			};
		}
		else if (m_Buff != null && m_Buff.Blueprint.HasRanks && m_Buff.Rank > 0)
		{
			secondaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = string.Format(UIStrings.Instance.CommonTexts.BuffStacks, m_Buff.Rank, m_Buff.Blueprint.MaxRank),
				TextParams = new TextFieldParams()
			};
		}
		bricks.Add(new TooltipBrickIconPattern(Icon, null, titleValues, secondaryValues));
	}

	private void AddConcentration(List<ITooltipBrick> bricks)
	{
		bricks.Add(new TooltipBrickText(UIUtilityEncyclopedy.GetGlossaryEntry("Concentration").GetDescription()));
		bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
	}

	private void AddDuration(List<ITooltipBrick> bricks)
	{
		if (m_Buff != null)
		{
			string duration = BuffTooltipUtils.GetDuration(m_Buff);
			if (!string.IsNullOrEmpty(duration))
			{
				TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
				{
					Text = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Duration)
				};
				TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
				{
					Text = UIUtilityText.WrapWithWeight(duration, TextFontWeight.SemiBold)
				};
				bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Duration, null, titleValues, secondaryValues));
			}
		}
	}

	private void AddStacking(List<ITooltipBrick> bricks)
	{
		if (m_Buff != null && m_Stacking != null)
		{
			bricks.Add(new TooltipBrickText(m_Stacking));
		}
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		if (m_Buff == null)
		{
			bricks.Add(new TooltipBrickText(UIUtilityText.UpdateDescriptionWithUIProperties(m_Desc, null), TooltipTextType.Paragraph));
		}
		else
		{
			bricks.Add(new TooltipBrickText(UIUtilityText.UpdateDescriptionWithUIProperties(m_Desc, ((IBuff)m_Buff).Caster), TooltipTextType.Paragraph));
		}
	}

	private void AddNonStackBonus(List<ITooltipBrick> bricks)
	{
		UnitPartNonStackBonuses unitPartNonStackBonuses = m_Buff?.Owner?.GetOptional<UnitPartNonStackBonuses>();
		if (unitPartNonStackBonuses != null && unitPartNonStackBonuses.ShouldShowWarning(m_Buff))
		{
			bricks.Add(new TooltipBrickNonStack(unitPartNonStackBonuses));
		}
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
		if (m_Buff != null)
		{
			ITooltipBrick tooltipBrick = null;
			if (m_Buff?.SourceAbilityBlueprint != null)
			{
				tooltipBrick = new TooltipBrickIconPattern(m_Buff.SourceAbilityBlueprint.Icon, null, UIStrings.Instance.Tooltips.Source, m_Buff.SourceAbilityBlueprint.Name);
			}
			if (m_Buff?.SourceFact != null && m_Buff.SourceFact.Blueprint is BlueprintBuff { IsHiddenInUI: false })
			{
				tooltipBrick = new TooltipBrickIconPattern(m_Buff.SourceFact.Icon, null, UIStrings.Instance.Tooltips.Source, m_Buff.SourceFact.Name);
			}
			if (m_Buff?.SourceItem != null)
			{
				tooltipBrick = new TooltipBrickIconPattern(m_Buff.SourceItem.ToItemEntity().Icon, null, UIStrings.Instance.Tooltips.Source, m_Buff.SourceItem.ToItemEntity().Name);
			}
			if (tooltipBrick == null && m_OverrideCaster.Entity is BaseUnitEntity baseUnitEntity)
			{
				tooltipBrick = new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Source, null, UIStrings.Instance.Tooltips.Source, baseUnitEntity.CharacterName);
			}
			if (tooltipBrick == null && m_Buff?.MaybeContext?.MaybeCaster is BaseUnitEntity baseUnitEntity2)
			{
				tooltipBrick = new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Source, null, UIStrings.Instance.Tooltips.Source, baseUnitEntity2.CharacterName);
			}
			if (tooltipBrick != null)
			{
				bricks.Add(tooltipBrick);
			}
		}
	}

	private void AddDOT(List<ITooltipBrick> bricks)
	{
		DOTLogicVisual dOTLogicVisual = m_Buff?.Blueprint?.GetComponent<DOTLogicVisual>();
		if (dOTLogicVisual == null)
		{
			return;
		}
		Buff buff = null;
		DOTLogic dOTLogic = null;
		foreach (Buff item2 in m_Buff.Owner.Buffs.Enumerable)
		{
			dOTLogic = item2.Blueprint?.GetComponent<DOTLogic>();
			if (dOTLogic != null && dOTLogic.Type == dOTLogicVisual.Type)
			{
				buff = item2;
				break;
			}
		}
		if (dOTLogic == null || buff == null)
		{
			return;
		}
		BaseUnitEntity owner = buff.Owner;
		MechanicEntity initiator = buff.Context.MaybeCaster ?? owner;
		TooltipBrickStrings tooltipBrickStrings = GameLogStrings.Instance.TooltipBrickStrings;
		IntermediateDamage resultDamage = Rulebook.Trigger(new RuleCalculateDamage(initiator, owner, buff.Context.SourceAbility, null, dOTLogic.DamageType.CreateDamage(m_Buff.Rank))
		{
			Reason = buff
		}).ResultDamage;
		bricks.Add(new TooltipBrickDamageRange(tooltipBrickStrings.Damage.Text, resultDamage.AverageValue, resultDamage.MinValue, resultDamage.MaxValue, 1, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: true, isBorderChanceIcon: false, isGrayBackground: false, isBeigeBackground: false, isRedBackground: true));
		string value = resultDamage.MinValueBase + " — " + resultDamage.MaxValueBase;
		TooltipBrickTextValue item = new TooltipBrickTextValue(tooltipBrickStrings.BaseModifier.Text, value, 2, isResultValue: true);
		bricks.Add(item);
		foreach (ITooltipBrick minMaxDamageModifier in LogThreadBase.GetMinMaxDamageModifiers(resultDamage.MinValueModifiers, resultDamage.MaxValueModifiers, 2))
		{
			bricks.Add(minMaxDamageModifier);
		}
		foreach (ITooltipBrick commonDamageModifier in LogThreadBase.GetCommonDamageModifiers(resultDamage.Modifiers, 2))
		{
			bricks.Add(commonDamageModifier);
		}
	}
}
