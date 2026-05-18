using System.Collections;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TutorialHintWindowPCView : TutorialWindowPCView<TutorialHintWindowVM>
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
	private OwlcatMultiButton m_EncyclopediaButton;

	protected override bool IsBigTutorial => false;

	protected override void OnBind()
	{
		base.OnBind();
		m_ConfirmButtonText.text = UIStrings.Instance.Tutorial.GotIt.Text;
		ObservableSubscribeExtensions.Subscribe(m_ConfirmButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Hide();
		}).AddTo(this);
		bool isPossibleGoToEncyclopedia = UIUtilityEncyclopedy.IsPossibleGoToEncyclopedia;
		bool currentValue = base.ViewModel.EncyclopediaLinkExist.CurrentValue;
		m_EncyclopediaButton.gameObject.SetActive(isPossibleGoToEncyclopedia && currentValue);
		if (isPossibleGoToEncyclopedia && currentValue)
		{
			m_EncyclopediaButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.GoToEncyclopedia).AddTo(this);
			m_EncyclopediaButton.SetHint(UIStrings.Instance.Tutorial.Encyclopedia.Text).AddTo(this);
		}
		SetContent();
		m_ScrollRect.ScrollToTop();
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
		ModalWindowsSounds.Instance.Tutorial.HideSmallTutorial.Play();
	}

	private void SetContent()
	{
		SetPage(base.ViewModel.Pages.FirstOrDefault());
	}

	private void SetWindowSize()
	{
		m_ViewPort.preferredHeight = Mathf.Min(m_ViewPortHeight, m_Content.sizeDelta.y);
	}

	private IEnumerator SetSizeDelayed()
	{
		yield return new WaitForEndOfFrame();
		SetWindowSize();
	}
}
