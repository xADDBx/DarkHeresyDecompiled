using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickHistoryManagementConsoleView : TooltipBrickHistoryManagementView, IConsoleInputHandler
{
	[SerializeField]
	private ConsoleHint m_PreviousHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

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

	public void AddInputTo(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, GridConsoleNavigationBehaviour ownerBehaviour)
	{
		m_PreviousHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnPreviousButtonClick(ownerBehaviour);
		}, 14, base.ViewModel.PreviousButtonInteractable)).AddTo(this);
		m_NextHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OnNextButtonClick(ownerBehaviour);
		}, 15, base.ViewModel.NextButtonInteractable)).AddTo(this);
	}

	public void UpdateTooltipBrick()
	{
		base.ViewModel.CheckDirectionButtons();
	}
}
