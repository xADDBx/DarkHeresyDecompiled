using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickHistoryManagementConsoleView : BrickHistoryManagementView
{
	[SerializeField]
	private HintView m_PreviousHint;

	[SerializeField]
	private HintView m_NextHint;

	protected override void OnBind()
	{
		m_TitleLabel.text = base.ViewModel.Title;
		base.ViewModel.PreviousButtonInteractable.Subscribe(delegate(bool value)
		{
			m_PreviousButton.Interactable = value;
		}).AddTo(this);
		base.ViewModel.NextButtonInteractable.Subscribe(delegate(bool value)
		{
			m_NextButton.Interactable = value;
		}).AddTo(this);
	}

	public void AddInputTo()
	{
	}

	public void UpdateTooltipBrick()
	{
		base.ViewModel.CheckDirectionButtons();
	}
}
