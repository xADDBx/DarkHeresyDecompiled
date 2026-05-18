using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using R3;
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
	private HintView m_ConfirmHint;

	[SerializeField]
	private HintView m_PreviousHint;

	[SerializeField]
	private HintView m_CloseWindowHint;

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
	}

	private void OnNext()
	{
		if (base.ViewModel.CurrentPageIndex.CurrentValue < base.ViewModel.PageCount - 1)
		{
			m_PageNavigation.OnNextClick();
			ModalWindowsSounds.Instance.Tutorial.ChangeTutorialPage.Play();
		}
		else
		{
			base.ViewModel.Hide();
		}
	}

	private void OnPrev()
	{
		m_PageNavigation.OnPreviousClick();
		ModalWindowsSounds.Instance.Tutorial.ChangeTutorialPage.Play();
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
			m_FirstGlossaryFocus.ShowTooltip(TooltipHelper.GetLinkTooltipTemplate(LinkKey));
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

	public void Scroll(float x)
	{
		if (!(m_ScrollRect == null))
		{
			PointerEventData data = new PointerEventData(EventSystem.current)
			{
				scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity)
			};
			m_ScrollRect.OnSmoothlyScroll(data);
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		TooltipHelper.HideTooltip();
		ModalWindowsSounds.Instance.Tutorial.ShowBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		CloseGlossary();
		ModalWindowsSounds.Instance.Tutorial.HideBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: false);
	}

	private void OnCurrentInputLayerChanged()
	{
	}
}
