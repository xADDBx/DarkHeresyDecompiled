using System;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Code.View.UI.MVVM.Titles;

public class TitlesContext : ViewModel, IEndGameTitlesUIHandler, ISubscriber
{
	private readonly ReactiveProperty<TitlesVM> m_TitlesVM;

	private readonly Action m_ReturnToMainMenu;

	public TitlesContext(ReactiveProperty<TitlesVM> vm, Action returnToMainMenu)
	{
		m_TitlesVM = vm;
		m_ReturnToMainMenu = returnToMainMenu;
		EventBus.Subscribe(this);
	}

	protected override void OnDispose()
	{
		EventBus.Unsubscribe(this);
		DisposeVM();
	}

	void IEndGameTitlesUIHandler.HandleShowEndGameTitles(bool returnToMainMenu)
	{
		DisposeVM();
		CreateVM(returnToMainMenu);
	}

	private void CreateVM(bool returnToMainMenu)
	{
		m_TitlesVM.Value = new TitlesVM(delegate
		{
			HandleOnTitlesClosed(returnToMainMenu);
		});
	}

	private void DisposeVM()
	{
		m_TitlesVM.Value?.Dispose();
		m_TitlesVM.Value = null;
	}

	private void HandleOnTitlesClosed(bool returnToMainMenu)
	{
		DisposeVM();
		if (returnToMainMenu)
		{
			m_ReturnToMainMenu();
		}
	}
}
