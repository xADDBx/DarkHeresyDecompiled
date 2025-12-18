using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public abstract class TutorialWindowConsoleView<TViewModel> : TutorialWindowBaseView<TViewModel> where TViewModel : TutorialWindowVM
{
	[SerializeField]
	protected ConsoleHint m_ToggleHint;

	[SerializeField]
	protected ConsoleHint m_GlossaryHint;

	[SerializeField]
	protected ConsoleHint m_CloseGlossaryHint;

	[SerializeField]
	protected ConsoleHint m_EncyclopediaHint;

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

	protected InputLayer GlossaryInputLayer;

	protected FloatConsoleNavigationBehaviour NavigationBehaviour;

	protected InputLayer InputLayer;

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

	protected void SelectDeselectToggle(InputActionEventData eventData)
	{
		UISounds.Instance.Sounds.Tutorial.BanTutorialType.Play();
		m_DontShowToggle.Set(!m_DontShowToggle.IsOn.CurrentValue);
	}

	protected void DelayedGlossaryCalculation()
	{
		DelayedInvoker.InvokeInFrames(CalculateGlossary, 5);
	}

	private void CalculateGlossary()
	{
		m_Entities.Clear();
		NavigationBehaviour.Clear();
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		List<IFloatConsoleNavigationEntity> list2 = new List<IFloatConsoleNavigationEntity>();
		list = TMPLinkNavigationGenerator.GenerateEntityList(m_TutorialText, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusLink, TooltipHelper.GetLinkTooltipTemplate);
		if (m_SolutionText.text != null)
		{
			list2 = TMPLinkNavigationGenerator.GenerateEntityList(m_SolutionText, m_FirstGlossaryFocus, m_SecondGlossaryFocus, null, OnFocusLink, TooltipHelper.GetLinkTooltipTemplate);
		}
		m_FirstEnt = list.FirstOrDefault();
		foreach (IFloatConsoleNavigationEntity item in list)
		{
			m_Entities.Add(item);
		}
		if (!list2.Empty())
		{
			foreach (IFloatConsoleNavigationEntity item2 in list2)
			{
				m_Entities.Add(item2);
			}
		}
		NavigationBehaviour.AddEntities(m_Entities);
		HasGlossaryPoints.Value = m_Entities.Any();
	}

	protected void ShowGlossary()
	{
		GamePad.Instance.PushLayer(GlossaryInputLayer).AddTo(this);
		IsGlossaryMode.Value = true;
		NavigationBehaviour.FocusOnEntityManual(m_FirstEnt);
	}

	protected void CloseGlossary()
	{
		if (GlossaryInputLayer != null)
		{
			GamePad.Instance.PopLayer(GlossaryInputLayer);
		}
		IsGlossaryMode.Value = false;
		TooltipHelper.HideTooltip();
		NavigationBehaviour?.UnFocusCurrentEntity();
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
