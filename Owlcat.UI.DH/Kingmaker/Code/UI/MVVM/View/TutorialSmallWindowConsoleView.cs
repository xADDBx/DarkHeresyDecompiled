using System.Collections;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TutorialSmallWindowConsoleView : TutorialWindowConsoleView<TutorialHintWindowVM>
{
	[Space]
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private int m_ViewPortHeight = 350;

	[SerializeField]
	private LayoutElement m_ViewPort;

	[SerializeField]
	private RectTransform m_Content;

	[SerializeField]
	private HintView m_CloseWindowHint;

	[SerializeField]
	private HintView m_OptionsCloseHint;

	[SerializeField]
	private HintView m_EnterSmallTutorHintsHint;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private float m_Offset;

	private bool m_IsHintsSetup;

	private readonly ReactiveProperty<bool> m_IsInSmallTutor = new ReactiveProperty<bool>();

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	protected override bool IsBigTutorial => false;

	protected override void OnBind()
	{
		base.OnBind();
		SetContent();
		m_IsInSmallTutor.Value = false;
		m_ScrollRect.ScrollToTop();
		Rebind();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_Disposable.Clear();
		ExitTutorialNavigation();
	}

	private void EnterTutorialNavigation()
	{
		if (!m_IsHintsSetup)
		{
			m_IsHintsSetup = true;
			m_IsInSmallTutor.Value = true;
			CreateInput();
			base.ViewModel.ChangeExpandState();
		}
	}

	private void ExitTutorialNavigation()
	{
		m_IsInSmallTutor.Value = false;
		CloseGlossary();
		m_IsHintsSetup = false;
		base.ViewModel.ChangeExpandState();
	}

	private void Rebind()
	{
		if (!m_IsHintsSetup)
		{
			CloseGlossary();
		}
	}

	private void CreateInput()
	{
	}

	protected override void OnFocusLink(string key)
	{
		IsPossibleGoToEncyclopedia.Value = TooltipHelper.GetLinkTooltipTemplate(key) is TooltipTemplateGlossary;
		LinkKey = key;
		if (!(this == null))
		{
			StartCoroutine(DelayedEnsureVisible());
		}
	}

	protected override void Focus()
	{
		if (IsGlossaryMode.Value)
		{
			m_FirstGlossaryFocus.ShowTooltip(TooltipHelper.GetLinkTooltipTemplate(LinkKey), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace));
		}
	}

	private void DelayedFocus()
	{
		Observable.NextFrame().Subscribe(Focus).AddTo(this);
	}

	protected override void OnShow()
	{
		base.OnShow();
		ModalWindowsSounds.Instance.Tutorial.ShowSmallTutorial.Play();
		StartCoroutine(SetSizeDelayed());
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_IsHintsSetup = false;
		CloseGlossary();
		ModalWindowsSounds.Instance.Tutorial.HideSmallTutorial.Play();
		base.ViewModel.ChangeExpandState();
	}

	private void SetContent()
	{
		SetPage(base.ViewModel.Pages.FirstOrDefault());
	}

	private void SetWindowSize()
	{
		m_ViewPort.preferredHeight = Mathf.Min(m_ViewPortHeight, m_Content.sizeDelta.y + m_Offset);
	}

	private IEnumerator SetSizeDelayed()
	{
		yield return new WaitForEndOfFrame();
		SetWindowSize();
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
}
