using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TutorialModalWindowPCView : TutorialWindowPCView<TutorialModalWindowVM>
{
	[Space]
	[SerializeField]
	private GameObject m_EncyclopediaBlock;

	[SerializeField]
	private OwlcatMultiButton m_EncyclopediaButton;

	[SerializeField]
	private GameObject m_PagesBlock;

	[SerializeField]
	private PageNavigationPC m_PageNavigation;

	[SerializeField]
	private TextMeshProUGUI m_PageNavigationText;

	[SerializeField]
	private float m_EncyclopediaButtonDefaultSize = 18f;

	protected override bool IsShowDefaultSprite => true;

	protected override void OnBind()
	{
		base.OnBind();
		m_EncyclopediaButton.SetHint(UIStrings.Instance.Tutorial.Encyclopedia.Text).AddTo(this);
		base.ViewModel.CurrentPage.Subscribe(base.SetPage).AddTo(this);
		bool isPossibleGoToEncyclopedia = UIUtilityEncyclopedy.IsPossibleGoToEncyclopedia;
		bool currentValue = base.ViewModel.EncyclopediaLinkExist.CurrentValue;
		m_EncyclopediaBlock.SetActive(isPossibleGoToEncyclopedia && currentValue);
		if (isPossibleGoToEncyclopedia && currentValue)
		{
			m_EncyclopediaButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.GoToEncyclopedia).AddTo(this);
		}
		m_PagesBlock.SetActive(base.ViewModel.MultiplePages);
		m_PageNavigation.Initialize(base.ViewModel.PageCount, base.ViewModel.CurrentPageIndex, base.ViewModel.SetCurrentPage);
		base.ViewModel.CurrentPageIndex.Subscribe(delegate
		{
			m_PageNavigationText.text = base.ViewModel.CurrentPageIndex.CurrentValue + 1 + "/" + base.ViewModel.PageCount;
			m_ConfirmButtonText.text = ((base.ViewModel.CurrentPageIndex.CurrentValue + 1 < base.ViewModel.PageCount) ? UIStrings.Instance.Tutorial.Next.Text : UIStrings.Instance.Tutorial.Complete.Text);
		}).AddTo(this);
		m_ConfirmButton.OnLeftClickAsObservable().Subscribe(OnNext).AddTo(this);
		m_PageNavigation.AddTo(this);
		TooltipHelper.HideTooltip();
	}

	private void OnNext()
	{
		if (base.ViewModel.CurrentPageIndex.CurrentValue + 1 < base.ViewModel.PageCount)
		{
			base.ViewModel.SetCurrentPage(base.ViewModel.CurrentPageIndex.CurrentValue + 1);
			UISounds.Instance.Sounds.Tutorial.ChangeTutorialPage.Play();
		}
		else
		{
			base.ViewModel.Hide();
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		UISounds.Instance.Sounds.Tutorial.ShowBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		UISounds.Instance.Sounds.Tutorial.HideBigTutorial.Play();
		Game.Instance.RequestPauseUi(isPaused: false);
	}

	protected override void SetTextsSize(float multiplier)
	{
		base.SetTextsSize(multiplier);
	}
}
