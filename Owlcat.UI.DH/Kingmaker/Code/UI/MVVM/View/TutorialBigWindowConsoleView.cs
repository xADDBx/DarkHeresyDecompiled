using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View;

public class TutorialBigWindowConsoleView : TutorialWindowConsoleView<TutorialModalWindowVM>
{
	[Space]
	[SerializeField]
	private GameObject m_PagesBlock;

	[SerializeField]
	private PageNavigationConsole m_PageNavigation;

	[SerializeField]
	private TextMeshProUGUI m_PageNavigationText;

	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	protected TextMeshProUGUI m_ConfirmButtonText;

	[Space]
	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private ConsoleHint m_PreviousHint;

	[SerializeField]
	private ConsoleHint m_CloseWindowHint;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	public static readonly string InputLayerContextName = "BigTutorialWindow";

	public static readonly string GlossaryContextName = "BigTutorGlossary";

	protected override bool IsShowDefaultSprite => true;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.CurrentPage.Subscribe(base.SetPage).AddTo(this);
		m_PagesBlock.SetActive(base.ViewModel.MultiplePages);
		m_PageNavigation.Initialize(base.ViewModel.PageCount, base.ViewModel.CurrentPageIndex, base.ViewModel.SetCurrentPage);
		m_PageNavigation.AddTo(this);
		CreateInput();
		base.ViewModel.CurrentPage.Subscribe(delegate
		{
			DelayedGlossaryCalculation();
		}).AddTo(this);
		base.ViewModel.CurrentPageIndex.Subscribe(delegate
		{
			m_PageNavigationText.text = base.ViewModel.CurrentPageIndex.CurrentValue + 1 + "/" + base.ViewModel.PageCount;
			m_ConfirmButtonText.text = ((base.ViewModel.CurrentPageIndex.CurrentValue + 1 < base.ViewModel.PageCount) ? UIStrings.Instance.Tutorial.Next.Text : UIStrings.Instance.Tutorial.Complete.Text);
		}).AddTo(this);
		TooltipHelper.HideTooltip();
	}

	private void CreateInput()
	{
		GamePad.Instance.BaseLayer?.Unbind();
		NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_NavigationParameters).AddTo(this);
		InputLayer = new InputLayer
		{
			ContextName = InputLayerContextName
		};
		GlossaryInputLayer = NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = GlossaryContextName
		});
		m_PageNavigation.AddInput(InputLayer, IsGlossaryMode.Not().ToReadOnlyReactiveProperty(initialValue: false), addDpad: true, showHints: false);
		InputBindStruct inputBindStruct = InputLayer.AddButton(base.SelectDeselectToggle, 10, IsGlossaryMode.Not().ToReadOnlyReactiveProperty(initialValue: false));
		m_ToggleHint.Bind(inputBindStruct).AddTo(this);
		inputBindStruct.AddTo(this);
		InputBindStruct inputBindStruct2 = InputLayer.AddButton(delegate
		{
			OnPrev();
		}, 9, m_PageNavigation.HasPrevious, InputActionEventType.ButtonJustReleased);
		m_PreviousHint.Bind(inputBindStruct2).AddTo(this);
		inputBindStruct2.AddTo(this);
		m_PreviousHint.SetLabel(UIStrings.Instance.Tutorial.Previous.Text);
		InputBindStruct inputBindStruct3 = InputLayer.AddButton(delegate
		{
			OnNext();
		}, 8, IsGlossaryMode.Not().ToReadOnlyReactiveProperty(initialValue: false));
		m_ConfirmHint.Bind(inputBindStruct3).AddTo(this);
		inputBindStruct3.AddTo(this);
		InputBindStruct inputBindStruct4 = InputLayer.AddButton(delegate
		{
			base.ViewModel.Hide();
		}, 9, IsGlossaryMode.Not().ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustLongPressed);
		m_CloseWindowHint.Bind(inputBindStruct4).AddTo(this);
		inputBindStruct4.AddTo(this);
		m_CloseWindowHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow.Text);
		InputBindStruct inputBindStruct5 = InputLayer.AddButton(delegate
		{
			ShowGlossary();
		}, 11, IsGlossaryMode.Not().And(HasGlossaryPoints).ToReadOnlyReactiveProperty(initialValue: false));
		m_GlossaryHint.Bind(inputBindStruct5).AddTo(this);
		inputBindStruct5.AddTo(this);
		m_GlossaryHint.SetLabel(UIStrings.Instance.Dialog.OpenGlossary.Text);
		InputBindStruct inputBindStruct6 = GlossaryInputLayer.AddButton(delegate
		{
			CloseGlossary();
		}, 9, IsGlossaryMode, InputActionEventType.ButtonJustReleased);
		m_CloseGlossaryHint.Bind(inputBindStruct6).AddTo(this);
		inputBindStruct6.AddTo(this);
		m_CloseGlossaryHint.SetLabel(UIStrings.Instance.Dialog.CloseGlossary.Text);
		InputBindStruct inputBindStruct7 = GlossaryInputLayer.AddButton(delegate
		{
			GoToEncyclopedia();
		}, 10);
		m_EncyclopediaHint.Bind(inputBindStruct7).AddTo(this);
		inputBindStruct7.AddTo(this);
		m_EncyclopediaHint.SetLabel(UIStrings.Instance.EncyclopediaTexts.EncyclopediaGlossaryButton.Text);
		ReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = IsGlossaryMode.Not().ToReadOnlyReactiveProperty(initialValue: false);
		InputLayer.AddAxis(Scroll, 3, readOnlyReactiveProperty).AddTo(this);
		GamePad.Instance.PushLayer(InputLayer).AddTo(this);
		GamePad.Instance.OnLayerPushed.Subscribe(OnCurrentInputLayerChanged).AddTo(this);
	}

	private void OnNext()
	{
		if (base.ViewModel.CurrentPageIndex.CurrentValue < base.ViewModel.PageCount - 1)
		{
			m_PageNavigation.OnNextClick();
			UISounds.Instance.Sounds.Tutorial.ChangeTutorialPage.Play();
		}
		else
		{
			base.ViewModel.Hide();
		}
	}

	private void OnPrev()
	{
		m_PageNavigation.OnPreviousClick();
		UISounds.Instance.Sounds.Tutorial.ChangeTutorialPage.Play();
	}

	protected override void OnFocusLink(string key)
	{
		LinkKey = key;
		StartCoroutine(DelayedEnsureVisible());
	}

	protected override void GoToEncyclopedia()
	{
		if (IsGlossaryMode.Value)
		{
			base.GoToEncyclopedia();
		}
		else
		{
			base.ViewModel.GoToEncyclopedia();
		}
	}

	protected override void Focus()
	{
		if (IsGlossaryMode.Value)
		{
			OwlcatMultiButton firstGlossaryFocus = m_FirstGlossaryFocus;
			TooltipBaseTemplate linkTooltipTemplate = TooltipHelper.GetLinkTooltipTemplate(LinkKey);
			ConsoleNavigationBehaviour navigationBehaviour = NavigationBehaviour;
			firstGlossaryFocus.ShowTooltip(linkTooltipTemplate, default(TooltipConfig), null, navigationBehaviour);
		}
	}

	private void DelayedFocus()
	{
		Observable.NextFrame().Subscribe(Focus).AddTo(this);
	}

	private IEnumerator DelayedEnsureVisible()
	{
		yield return new WaitForEndOfFrame();
		FocusAndEnsureVisible();
	}

	private void FocusAndEnsureVisible()
	{
		DelayedFocus();
		EnsureVisible();
	}

	private void EnsureVisible()
	{
		m_ScrollRect.EnsureVisibleVertical(m_FirstGlossaryFocus.transform as RectTransform);
	}

	public void Scroll(InputActionEventData data, float x)
	{
		if (!(m_ScrollRect == null))
		{
			PointerEventData data2 = new PointerEventData(EventSystem.current)
			{
				scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity)
			};
			m_ScrollRect.OnSmoothlyScroll(data2);
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		TooltipHelper.HideTooltip();
		UISounds.Instance.Sounds.Tutorial.ShowBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		CloseGlossary();
		GamePad.Instance.BaseLayer?.Bind();
		UISounds.Instance.Sounds.Tutorial.HideBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: false);
	}

	private void OnCurrentInputLayerChanged()
	{
		GamePad instance = GamePad.Instance;
		if (instance.CurrentInputLayer != InputLayer && instance.CurrentInputLayer != GlossaryInputLayer && !(instance.CurrentInputLayer.ContextName == InfoWindowConsoleView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == "SaveLoad") && !(instance.CurrentInputLayer.ContextName == "SaveFullScreenshotConsoleView") && !(instance.CurrentInputLayer.ContextName == MessageBoxConsoleView.InputLayerName) && !(instance.CurrentInputLayer.ContextName == ContextMenuConsoleView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == OwlcatDropdown.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == OwlcatInputField.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == CrossPlatformConsoleVirtualKeyboard.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == BugReportDrawingView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == EscMenuBaseView.InputLayerContextName) && !(instance.CurrentInputLayer.ContextName == SettingsConsoleView.SettingsInputLayerName) && !(instance.CurrentInputLayer.ContextName == SettingsConsoleView.GlossarySettingsInputLayerName) && !(instance.CurrentInputLayer.ContextName == NetLobbyConsoleView.InputLayerName))
		{
			CloseGlossary();
			instance.PopLayer(InputLayer);
			instance.PushLayer(InputLayer);
		}
	}
}
