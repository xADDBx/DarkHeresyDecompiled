using JetBrains.Annotations;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class MainMenuButtonsView<TContextMenuEntityView> : View<MainMenuButtonsVM> where TContextMenuEntityView : ContextMenuEntityView
{
	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_ContinueView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_NewGameView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_LoadView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_NetView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_AddonsView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_OptionsView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_CreditView;

	[SerializeField]
	[UsedImplicitly]
	protected TContextMenuEntityView m_ExitView;

	protected override void OnBind()
	{
		m_ContinueView.Bind(base.ViewModel.ContinueVm);
		m_NewGameView.Bind(base.ViewModel.NewGameVm);
		m_LoadView.Bind(base.ViewModel.LoadVm);
		m_OptionsView.Bind(base.ViewModel.OptionsVm);
		m_ExitView.gameObject.SetActive(base.ViewModel.ExitEnabled);
		if (base.ViewModel.ExitEnabled)
		{
			m_ExitView.Bind(base.ViewModel.ExitVm);
		}
	}
}
