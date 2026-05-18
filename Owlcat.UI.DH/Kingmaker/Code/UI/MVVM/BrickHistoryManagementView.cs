using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickHistoryManagementView : BrickBaseView<BrickHistoryManagementVM>
{
	[Header("Elements")]
	[SerializeField]
	protected TMP_Text m_TitleLabel;

	[SerializeField]
	protected OwlcatMultiButton m_PreviousButton;

	[SerializeField]
	protected OwlcatMultiButton m_NextButton;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_TitleLabel).AddTo(this);
		}
		base.OnBind();
		m_TitleLabel.text = base.ViewModel.Title;
		base.ViewModel.PreviousButtonInteractable.Subscribe(delegate(bool value)
		{
			m_PreviousButton.Interactable = value;
		}).AddTo(this);
		base.ViewModel.NextButtonInteractable.Subscribe(delegate(bool value)
		{
			m_NextButton.Interactable = value;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_PreviousButton.OnLeftClickAsObservable(), delegate
		{
			if (base.ViewModel.PreviousButtonInteractable.CurrentValue)
			{
				base.ViewModel.OnPreviousButtonClick();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_NextButton.OnLeftClickAsObservable(), delegate
		{
			if (base.ViewModel.NextButtonInteractable.CurrentValue)
			{
				base.ViewModel.OnNextButtonClick();
			}
		}).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
