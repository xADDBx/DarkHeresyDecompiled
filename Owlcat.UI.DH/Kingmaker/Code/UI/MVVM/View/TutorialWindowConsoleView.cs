using System.Collections.Generic;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class TutorialWindowConsoleView<TViewModel> : TutorialWindowBaseView<TViewModel> where TViewModel : TutorialWindowVM
{
	[SerializeField]
	protected HintView m_ToggleHint;

	[SerializeField]
	protected HintView m_GlossaryHint;

	[SerializeField]
	protected HintView m_CloseGlossaryHint;

	[SerializeField]
	protected HintView m_EncyclopediaHint;

	[SerializeField]
	protected OwlcatMultiButton m_FirstGlossaryFocus;

	[SerializeField]
	protected OwlcatMultiButton m_SecondGlossaryFocus;

	[SerializeField]
	private float m_TitleDefaultConsoleFontSize = 28f;

	[SerializeField]
	private float m_TriggerDefaultSize = 24f;

	[SerializeField]
	private float m_MainTextsDefaultConsoleFontSize = 24f;

	private IConsoleEntity m_FirstEnt;

	protected string LinkKey;

	protected readonly ReactiveProperty<bool> IsGlossaryMode = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> HasGlossaryPoints = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> IsPossibleGoToEncyclopedia = new ReactiveProperty<bool>();

	private readonly List<IFloatConsoleNavigationEntity> m_Entities = new List<IFloatConsoleNavigationEntity>();

	protected abstract void OnFocusLink(string key);

	protected abstract void Focus();

	protected virtual void GoToEncyclopedia()
	{
		CloseGlossary();
		if (TooltipHelper.GetLinkTooltipTemplate(LinkKey) is TooltipTemplateGlossary tooltipTemplateGlossary)
		{
			tooltipTemplateGlossary.EncyclopediaCallback();
		}
		TooltipHelper.HideTooltip();
		base.ViewModel?.TemporarilyHide();
	}

	protected void SelectDeselectToggle()
	{
		ModalWindowsSounds.Instance.Tutorial.BanTutorialType.Play();
		m_DontShowToggle.Set(!m_DontShowToggle.IsOn.CurrentValue);
	}

	protected void DelayedGlossaryCalculation()
	{
		DelayedInvoker.InvokeInFrames(CalculateGlossary, 5);
	}

	private void CalculateGlossary()
	{
	}

	protected void ShowGlossary()
	{
		IsGlossaryMode.Value = true;
	}

	protected void CloseGlossary()
	{
		IsGlossaryMode.Value = false;
		TooltipHelper.HideTooltip();
	}

	public override void Show()
	{
		base.Show();
		m_DontShowToggle.Set(value: false);
	}

	protected override void SetTextsSize(float multiplier)
	{
		m_Title.fontSize = m_TitleDefaultConsoleFontSize * multiplier;
		m_TriggerText.fontSize = m_TriggerDefaultSize * multiplier;
		m_TutorialText.fontSize = m_MainTextsDefaultConsoleFontSize * multiplier;
		m_SolutionText.fontSize = m_MainTextsDefaultConsoleFontSize * multiplier;
		base.SetTextsSize(multiplier);
	}
}
