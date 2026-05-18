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

	private string m_CurrentPhaseTitle;

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
			IUIUnitMoraleData currentValue = m_UnitUIState.Morale.CurrentValue;
			m_CurrentValue = currentValue?.Morale ?? 0;
			if (currentValue != null)
			{
				m_CurrentPhase = LocalizedTexts.Instance.MoralePhases.GetText(currentValue.MoralePhase);
				m_CurrentPhaseTitle = LocalizedTexts.Instance.MoralePhaseTitles.GetText(currentValue.MoralePhase);
				m_CurrentPhaseDescription = GetGlossaryEntry(currentValue.MoralePhase).GetDescription();
				m_MoraleDescription = UIUtilityEncyclopedy.GetGlossaryEntry("Morale").GetDescription();
				m_WillBecomeHeroic = UIUtilityUnit.MoraleWillBecomeHeroic(currentValue, m_UnitUIState.MoralePrediction.CurrentValue);
				m_WillBecomeBroken = UIUtilityUnit.MoraleWillBecomeBroken(currentValue, m_UnitUIState.MoralePrediction.CurrentValue);
				m_IsMoraleLeader = m_UnitUIState.IsMoraleLeader.CurrentValue;
			}
		}
		catch (Exception ex)
		{
			PFLog.UI.Error(ex);
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(UIStrings.Instance.HUDTexts.MoraleTitle);
		yield return new BrickTitleVM(m_UnitUIState.Name.CurrentValue, TooltipTitleType.H3);
		if (m_IsMoraleLeader)
		{
			yield return new BrickTitleVM(UIStrings.Instance.HUDTexts.MoraleLeader, TooltipTitleType.H4);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new BrickTextValueVM(UIStrings.Instance.HUDTexts.MoraleCurrentValueTitle, m_CurrentValue.ToString());
		yield return new BrickTextValueVM(UIStrings.Instance.HUDTexts.MoraleCurrentPhaseTitle, m_CurrentPhase);
		yield return new BrickSliderVM(m_MinValue, m_MaxValue, m_CurrentValue, new List<SliderValuesVM>
		{
			new SliderValuesVM(m_MinValue, m_MaxValue, 0, null, needColor: true, UIConfig.Instance.TooltipColors.MoraleBroken, null, isValueOnBottom: false)
		}, showValue: true, UIConfig.Instance.TooltipColors.MoraleHeroic);
		if (m_WillBecomeHeroic)
		{
			yield return new BrickHintVM(UIStrings.Instance.HUDTexts.MoraleBecomeHeroicSoonHint);
		}
		if (m_WillBecomeBroken)
		{
			yield return new BrickHintVM(UIStrings.Instance.HUDTexts.MoraleBecomeBrokenSoonHint);
		}
		if (m_IsMoraleLeader)
		{
			yield return new BrickSeparatorVM(TooltipBrickElementType.Medium);
			yield return new BrickTitleVM(UIStrings.Instance.HUDTexts.MoraleLeader, TooltipTitleType.H4);
			yield return new BrickTextVM(UIStrings.Instance.HUDTexts.MoraleLeaderDescription);
		}
		yield return new BrickSeparatorVM(TooltipBrickElementType.Medium);
		yield return new BrickTitleVM(m_CurrentPhaseTitle, TooltipTitleType.H4);
		yield return new BrickTextVM(m_CurrentPhaseDescription);
		yield return new BrickSeparatorVM(TooltipBrickElementType.Medium);
		yield return new BrickTitleVM(UIStrings.Instance.HUDTexts.MoraleTitle, TooltipTitleType.H4);
		yield return new BrickTextVM(m_MoraleDescription);
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
