using System;
using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateMoraleUnit : TooltipBaseTemplate
{
	private readonly MechanicEntityUIState m_UnitUIState;

	private int m_MinValue = -10;

	private int m_MaxValue = 10;

	private int m_CurrentValue;

	private string m_CurrentPhase;

	private string m_CurrentPhaseDescription;

	private string m_MoraleDescription;

	private bool m_WillBecomeHeroic;

	private bool m_WillBecomeBroken;

	private bool m_IsMoraleLeader;

	private string m_MoraleLeaderDescription;

	public TooltipTemplateMoraleUnit(MechanicEntityUIState unitUIState)
	{
		m_UnitUIState = unitUIState;
	}

	public override void Prepare(TooltipTemplateType type)
	{
		if (m_UnitUIState == null)
		{
			return;
		}
		try
		{
			m_CurrentValue = m_UnitUIState.Morale.CurrentValue?.Morale ?? 0;
			m_CurrentPhase = LocalizedTexts.Instance.MoralePhases.GetText(m_UnitUIState.Morale.CurrentValue.MoralePhase);
			m_CurrentPhaseDescription = GetGlossaryEntry(m_UnitUIState.Morale.CurrentValue.MoralePhase).GetDescription();
			m_MoraleDescription = UIUtilityEncyclopedy.GetGlossaryEntry("Morale").GetDescription();
			m_WillBecomeHeroic = UIUtilityUnit.MoraleWillBecomeHeroic(m_UnitUIState.Morale.CurrentValue, m_UnitUIState.MoralePrediction.CurrentValue);
			m_WillBecomeBroken = UIUtilityUnit.MoraleWillBecomeBroken(m_UnitUIState.Morale.CurrentValue, m_UnitUIState.MoralePrediction.CurrentValue);
			m_IsMoraleLeader = m_UnitUIState.IsMoraleLeader.CurrentValue;
		}
		catch (Exception ex)
		{
			PFLog.UI.Error(ex);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(UIStrings.Instance.HUDTexts.MoraleTitle);
		yield return new TooltipBrickTitle(m_UnitUIState.Name.CurrentValue, TooltipTitleType.H3);
		if (m_IsMoraleLeader)
		{
			yield return new TooltipBrickTitle(UIStrings.Instance.HUDTexts.MoraleLeader, TooltipTitleType.H4);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickTextValue(UIStrings.Instance.HUDTexts.MoraleCurrentValueTitle, m_CurrentValue.ToString());
		yield return new TooltipBrickTextValue(UIStrings.Instance.HUDTexts.MoraleCurrentPhaseTitle, m_CurrentPhase);
		yield return new TooltipBrickSlider(m_MinValue, m_MaxValue, m_CurrentValue, new List<BrickSliderValueVM>
		{
			new BrickSliderValueVM(m_MinValue, m_MaxValue, 0, null, needColor: true, UIConfig.Instance.TooltipColors.MoraleBroken, null, isValueOnBottom: false)
		}, showValue: true, 50, UIConfig.Instance.TooltipColors.MoraleHeroic);
		if (m_WillBecomeHeroic)
		{
			yield return new TooltipBrickHint(UIStrings.Instance.HUDTexts.MoraleBecomeHeroicSoonHint);
		}
		if (m_WillBecomeBroken)
		{
			yield return new TooltipBrickHint(UIStrings.Instance.HUDTexts.MoraleBecomeBrokenSoonHint);
		}
		if (m_IsMoraleLeader)
		{
			yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
			yield return new TooltipBrickTitle(UIStrings.Instance.HUDTexts.MoraleLeader, TooltipTitleType.H4);
			yield return new TooltipBrickText(UIStrings.Instance.HUDTexts.MoraleLeaderDescription);
		}
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
		yield return new TooltipBrickTitle(m_CurrentPhase, TooltipTitleType.H4);
		yield return new TooltipBrickText(m_CurrentPhaseDescription);
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
		yield return new TooltipBrickTitle(UIStrings.Instance.HUDTexts.MoraleTitle, TooltipTitleType.H4);
		yield return new TooltipBrickText(m_MoraleDescription);
	}

	private BlueprintEncyclopediaGlossaryEntry GetGlossaryEntry(MoralePhaseType moralePhase)
	{
		return moralePhase switch
		{
			MoralePhaseType.Regular => UIUtilityEncyclopedy.GetGlossaryEntry("MoraleRegular"), 
			MoralePhaseType.Heroic => UIUtilityEncyclopedy.GetGlossaryEntry("MoraleHeroic"), 
			MoralePhaseType.Broken => UIUtilityEncyclopedy.GetGlossaryEntry("MoraleBroken"), 
			_ => throw new ArgumentOutOfRangeException("moralePhase", moralePhase, null), 
		};
	}
}
