using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickHistoryManagementView : TooltipBaseBrickView<TooltipBrickHistoryManagementVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_TitleLabel;

	[SerializeField]
	protected OwlcatMultiButton m_PreviousButton;

	[SerializeField]
	protected OwlcatMultiButton m_NextButton;

	[SerializeField]
	private float m_DefaultFontSize = 23f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 23f;

	protected override void OnBind()
	{
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
				base.ViewModel.OnPreviousButtonClick(null);
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_NextButton.OnLeftClickAsObservable(), delegate
		{
			if (base.ViewModel.NextButtonInteractable.CurrentValue)
			{
				base.ViewModel.OnNextButtonClick(null);
			}
		}).AddTo(this);
		m_TitleLabel.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * m_FontMultiplier;
	}
}
