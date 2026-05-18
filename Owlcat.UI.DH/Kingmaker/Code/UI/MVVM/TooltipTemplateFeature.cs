using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateFeature : TooltipBaseTemplate
{
	public readonly BlueprintFeatureBase BlueprintFeatureBase;

	private readonly Feature m_Feature;

	private readonly MechanicEntity m_Caster;

	private readonly bool m_WithVariants;

	private string m_Description;

	private (BlueprintAbility HeroicAct, BlueprintAbility DesperateMeasure) m_Abilities;

	public override void Prepare(TooltipTemplateType type)
	{
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)(m_Feature?.Owner ?? m_Caster);
				if (BlueprintFeatureBase == null)
				{
					throw new ArgumentNullException("BlueprintFeature is NUll");
				}
				m_Description = TooltipTemplateUtils.GetFullDescription(BlueprintFeatureBase);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {BlueprintFeatureBase?.name}: {arg}");
		}
	}

	public TooltipTemplateFeature([NotNull] Feature feature, bool withVariants = false)
		: this(feature.Blueprint, withVariants, feature.Owner)
	{
		m_Feature = feature;
	}

	public TooltipTemplateFeature(BlueprintFeatureBase feature, bool withVariants = false, MechanicEntity caster = null)
	{
		BlueprintFeatureBase = feature;
		m_WithVariants = withVariants;
		m_Caster = caster;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (!m_WithVariants)
		{
			string acronym = ((BlueprintFeatureBase.Icon != null) ? "" : UIUtilityAbilities.GetAbilityAcronym(BlueprintFeatureBase.Name));
			Sprite icon = ((BlueprintFeatureBase.Icon != null) ? BlueprintFeatureBase.Icon : UIUtilityText.GetIconByText(BlueprintFeatureBase.NameForAcronym));
			TextEntity title = new TextEntity(BlueprintFeatureBase.Name, TextFieldParams.Bold);
			yield return new BrickIconPatternVM(icon, null, title, null, null, null, IconPatternMode.SkillMode, acronym);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (m_Abilities.DesperateMeasure != null || m_Abilities.HeroicAct != null)
		{
			if (m_Abilities.HeroicAct != null)
			{
				yield return new BrickTextVM(UIStrings.Instance.Tooltips.HeroicActAbility, TooltipTextType.Bold, TooltipTextAlignment.Midl, m_Caster);
				yield return new BrickFeatureVM(m_Abilities.HeroicAct);
			}
			if (m_Abilities.DesperateMeasure != null)
			{
				yield return new BrickTextVM(UIStrings.Instance.Tooltips.DesperateMeasureAbility, TooltipTextType.Bold, TooltipTextAlignment.Midl, m_Caster);
				yield return new BrickFeatureVM(m_Abilities.DesperateMeasure);
			}
			yield break;
		}
		yield return new BrickTextVM(m_Description, TooltipTextType.Paragraph, TooltipTextAlignment.Midl, m_Caster);
		ITooltipBrick sourceBrick = null;
		if ((bool)m_Feature?.SourceAbilityBlueprint)
		{
			sourceBrick = new BrickFeatureVM(m_Feature?.SourceAbilityBlueprint);
		}
		if (m_Feature?.SourceFact != null && !string.IsNullOrEmpty(m_Feature?.SourceFact.Name))
		{
			sourceBrick = new BrickFeatureVM(m_Feature.SourceFact);
		}
		if (m_Feature?.SourceItem != null)
		{
			ItemEntity itemEntity = (ItemEntity)m_Feature.SourceItem;
			sourceBrick = new BrickIconAndNameVM(itemEntity.Name, itemEntity.Icon);
		}
		if ((bool)m_Feature?.SourceRace)
		{
			sourceBrick = new BrickFeatureVM(m_Feature?.SourceRace);
		}
		if (sourceBrick != null)
		{
			yield return new BrickSeparatorVM();
			yield return new BrickTitleVM(UIStrings.Instance.Tooltips.Source, TooltipTitleType.H2);
			yield return sourceBrick;
		}
	}
}
