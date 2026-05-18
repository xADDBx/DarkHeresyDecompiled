using System;
using System.Collections.Generic;
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
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.UI.MVVM;

public sealed class TooltipTemplateBuff : TooltipBaseTemplate
{
	private EntityRef m_OverrideCaster;

	private string m_Name;

	private string m_OverrideName;

	private string m_Desc;

	private string m_FlavorText;

	private Sprite m_Icon;

	private Sprite m_OverrideIcon;

	private string m_OverrideSecondary;

	private readonly string m_Stacking;

	private readonly Buff m_Buff;

	private readonly BlueprintBuff m_BlueprintBuff;

	private string Name => m_OverrideName ?? m_Name;

	private Sprite Icon => m_OverrideIcon ?? m_Icon;

	public BlueprintBuff BlueprintBuff => m_Buff?.Blueprint ?? m_BlueprintBuff;

	private bool HasBuffWithContext
	{
		get
		{
			Buff buff = m_Buff;
			if (buff != null)
			{
				return buff.MaybeContext != null;
			}
			return false;
		}
	}

	private MechanicEntity Caster
	{
		get
		{
			object obj = m_OverrideCaster.Entity as MechanicEntity;
			if (obj == null)
			{
				Buff buff = m_Buff;
				if (buff == null)
				{
					return null;
				}
				MechanicsContext maybeContext = buff.MaybeContext;
				if (maybeContext == null)
				{
					return null;
				}
				obj = maybeContext.MaybeCaster;
			}
			return (MechanicEntity)obj;
		}
	}

	public TooltipTemplateBuff(Buff buff, IEntity overrideCaster = null, Sprite overrideIcon = null, string overrideName = null, string overrideSecondary = null)
	{
		m_Buff = buff;
		m_OverrideCaster = new EntityRef(overrideCaster);
		m_OverrideIcon = overrideIcon;
		m_OverrideName = overrideName;
		m_OverrideSecondary = overrideSecondary;
	}

	public TooltipTemplateBuff(BlueprintBuff blueprintBuff, MechanicEntity caster = null)
	{
		m_BlueprintBuff = blueprintBuff;
		m_OverrideCaster = new EntityRef(caster);
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
		AddBuffHeader(list);
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDOT(list);
		AddSource(list);
		AddDuration(list);
		AddStacking(list);
		AddFlavorText(list);
		AddDescription(list);
		AddNonStackBonus(list);
		if (BlueprintBuff.IsCriticalEffect)
		{
			list.Add(new BrickSeparatorVM());
			BrickTextVM item = new BrickTextVM(UIStrings.Instance.Tooltips.CriticalEffectHint, TooltipTextType.Italic | TooltipTextType.BrightColor);
			list.Add(item);
		}
		return list;
	}

	private void FillBlueprintAbilityData(BlueprintBuff blueprintBuff)
	{
		try
		{
			if (blueprintBuff == null)
			{
				return;
			}
			using (GameLogContext.Scope)
			{
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Caster;
				m_Name = blueprintBuff.Name;
				m_Desc = blueprintBuff.Description;
				m_Icon = blueprintBuff.Icon;
				m_FlavorText = blueprintBuff.FlavorText;
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
				if (HasBuffWithContext)
				{
					GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)((IBuff)m_Buff).Caster;
					GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Caster;
				}
				m_Name = buff.Name;
				m_Desc = buff.Description;
				m_Icon = buff.Icon;
				m_FlavorText = buff.FlavorText;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {buff?.Blueprint?.name}: {arg}");
		}
	}

	private void AddBuffHeader(List<ITooltipBrick> bricks)
	{
		TextEntity title = new TextEntity(Name, new TextFieldParams(TMPro.FontStyles.Bold));
		TextValueElement secondaryValuesElement = null;
		if (!string.IsNullOrEmpty(m_OverrideSecondary))
		{
			secondaryValuesElement = new TextValueElement(new TextEntity(m_OverrideSecondary, TextFieldParams.Italic));
		}
		else if (m_Buff != null && m_Buff.Blueprint.HasRanks && m_Buff.Rank > 0)
		{
			secondaryValuesElement = new TextValueElement(string.Format(UIStrings.Instance.CommonTexts.BuffStacks, m_Buff.Rank, m_Buff.Blueprint.MaxRank));
		}
		bricks.Add(new BrickIconPatternVM(Icon, null, title, secondaryValuesElement));
	}

	private void AddDuration(List<ITooltipBrick> bricks)
	{
		if (m_Buff != null)
		{
			string durationText = m_Buff.GetDurationText();
			if (!string.IsNullOrEmpty(durationText))
			{
				TextEntity title = new TextEntity(UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Duration));
				TextValueElement secondaryValuesElement = new TextValueElement(UIUtilityText.WrapWithWeight(durationText, TextFontWeight.SemiBold));
				bricks.Add(new BrickIconPatternVM(UIConfig.Instance.UIIcons.TooltipIcons.Duration, null, title, secondaryValuesElement));
			}
		}
	}

	private void AddStacking(List<ITooltipBrick> bricks)
	{
		if (m_Buff != null && m_Stacking != null)
		{
			bricks.Add(new BrickTextVM(m_Stacking));
		}
	}

	private void AddFlavorText(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_FlavorText))
		{
			bricks.Add(new BrickTextVM(m_FlavorText, TooltipTextType.Italic | TooltipTextType.BrightColor));
		}
	}

	private void AddDescription(List<ITooltipBrick> bricks)
	{
		bricks.Add(new BrickTextVM(m_Desc, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, Caster));
	}

	private void AddNonStackBonus(List<ITooltipBrick> bricks)
	{
		UnitPartNonStackBonuses unitPartNonStackBonuses = m_Buff?.Owner?.GetOptional<UnitPartNonStackBonuses>();
		if (unitPartNonStackBonuses != null && unitPartNonStackBonuses.ShouldShowWarning(m_Buff))
		{
			bricks.Add(new BrickNonStackVM(unitPartNonStackBonuses));
		}
	}

	private void AddSource(List<ITooltipBrick> bricks)
	{
		if (m_Buff != null)
		{
			ITooltipBrick tooltipBrick = null;
			if (m_Buff?.SourceAbilityBlueprint != null)
			{
				tooltipBrick = new BrickIconPatternVM(m_Buff.SourceAbilityBlueprint.Icon, null, UIStrings.Instance.Tooltips.Source, m_Buff.SourceAbilityBlueprint.Name);
			}
			if (m_Buff?.SourceFact != null && m_Buff.SourceFact.Blueprint is BlueprintBuff { IsHiddenInUI: false })
			{
				tooltipBrick = new BrickIconPatternVM(m_Buff.SourceFact.Icon, null, UIStrings.Instance.Tooltips.Source, m_Buff.SourceFact.Name);
			}
			if (m_Buff?.SourceItem != null)
			{
				tooltipBrick = new BrickIconPatternVM(m_Buff.SourceItem.ToItemEntity().Icon, null, UIStrings.Instance.Tooltips.Source, m_Buff.SourceItem.ToItemEntity().Name);
			}
			if (tooltipBrick == null && m_OverrideCaster.Entity is BaseUnitEntity baseUnitEntity)
			{
				tooltipBrick = new BrickIconPatternVM(UIConfig.Instance.UIIcons.TooltipIcons.Source, null, UIStrings.Instance.Tooltips.Source, baseUnitEntity.CharacterName);
			}
			if (tooltipBrick == null && m_Buff?.MaybeContext?.MaybeCaster is BaseUnitEntity baseUnitEntity2)
			{
				tooltipBrick = new BrickIconPatternVM(UIConfig.Instance.UIIcons.TooltipIcons.Source, null, UIStrings.Instance.Tooltips.Source, baseUnitEntity2.CharacterName);
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
		bricks.Add(new BrickDamageRangeVM(tooltipBrickStrings.Damage.Text, resultDamage.AverageValue, resultDamage.MinValue, resultDamage.MaxValue, 1, isResultValue: false, null, CombatLogIcon.TargetHit, BrickElementPalette.Negative));
		string value = resultDamage.MinValueBase + " — " + resultDamage.MaxValueBase;
		BrickTextValueVM item = new BrickTextValueVM(tooltipBrickStrings.BaseModifier.Text, value, 2, isResultValue: true);
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
