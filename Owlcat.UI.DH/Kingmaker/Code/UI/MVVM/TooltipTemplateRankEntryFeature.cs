using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.UIUtils;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateRankEntryFeature : TooltipTemplateUIFeature
{
	private readonly FeatureSelectionItem m_SelectionItem;

	private readonly ReadOnlyReactiveProperty<SelectionStateFeature> m_SelectionState;

	private readonly BaseUnitEntity m_Caster;

	private readonly RankEntrySelectionVM m_Owner;

	private CalculatedPrerequisite Prerequisite => m_SelectionState.CurrentValue?.GetCalculatedPrerequisite(m_SelectionItem) ?? CalculatedPrerequisite.Calculate(null, m_SelectionItem, m_Caster);

	public TooltipTemplateRankEntryFeature(UIFeature uiFeature, FeatureSelectionItem featureSelectionItem, ReadOnlyReactiveProperty<SelectionStateFeature> selectionState, RankEntrySelectionVM owner, BaseUnitEntity caster = null)
		: base(uiFeature)
	{
		m_SelectionItem = featureSelectionItem;
		m_SelectionState = selectionState;
		m_Caster = caster;
		m_Owner = owner;
	}

	protected override void AddDescription(List<ITooltipBrick> bricks)
	{
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				GameLogContext.DescriptionOwner = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				string fullDescription = TooltipTemplateUtils.GetFullDescription(UIFeature.Feature);
				bricks.Add(new BrickTextVM(fullDescription, TooltipTextType.Simple, TooltipTextAlignment.Midl, m_Caster));
			}
		}
		catch (Exception arg)
		{
			bricks.Add(new BrickTextVM(UIFeature.Description, TooltipTextType.Simple, TooltipTextAlignment.Midl, m_Caster));
			Debug.LogError($"Can't create TooltipTemplate for: {UIFeature.Feature.name}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (RankEntrySelectionFeaturesUtils.IsCommonSelectionItem(m_SelectionItem))
		{
			list.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.CommonFeatureDesc, TooltipTitleType.H4));
		}
		list.AddRange(base.GetHeader(type));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.AddRange(base.GetBody(type));
		if (Prerequisite != null)
		{
			list.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
			AddPrerequisiteGroup(UIUtilityAbilities.GetPrerequisiteEntries(Prerequisite), list);
		}
		return list;
	}

	private static void AddPrerequisiteGroup(List<PrerequisiteEntryVM> allPrerequisites, List<ITooltipBrick> result, bool isOr = false)
	{
		List<PrerequisiteEntryVM> list = allPrerequisites.Where((PrerequisiteEntryVM p) => !p.Inverted && !p.IsGroup).ToList();
		List<PrerequisiteEntryVM> list2 = allPrerequisites.Where((PrerequisiteEntryVM p) => p.Inverted && !p.IsGroup).ToList();
		List<PrerequisiteEntryVM> list3 = allPrerequisites.Where((PrerequisiteEntryVM p) => p.IsGroup).ToList();
		if (isOr && list.Any())
		{
			AddOr(result);
		}
		result.Add(new BrickPrerequisiteVM(list));
		if (list2.Any())
		{
			if (isOr)
			{
				AddOr(result);
			}
			result.Add(new BrickTitleVM(UIStrings.Instance.Tooltips.NoFeature, TooltipTitleType.H3));
			result.Add(new BrickPrerequisiteVM(list2));
		}
		if (isOr && list3.Any())
		{
			AddOr(result);
		}
		for (int i = 0; i < list3.Count; i++)
		{
			if (isOr && i > 0)
			{
				AddOr(result);
			}
			PrerequisiteEntryVM prerequisiteEntryVM = list3[i];
			AddPrerequisiteGroup(prerequisiteEntryVM.Prerequisites, result, prerequisiteEntryVM.IsOrComposition);
		}
	}

	private static void AddOr(List<ITooltipBrick> result)
	{
		result.Add(new BrickItemHeaderVM($"<size={UIConfig.Instance.SubTextPercentSize}%>{UIStrings.Instance.Tooltips.or.Text}</size>"));
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (type != 0 && Game.Instance.IsControllerMouse && RankEntrySelectionFeaturesUtils.HasPrerequisiteFooter(Prerequisite, m_Owner))
		{
			yield return new BrickTitleVM(UIStrings.Instance.Tooltips.PrerequisitesFooter, TooltipTitleType.H6);
		}
	}
}
