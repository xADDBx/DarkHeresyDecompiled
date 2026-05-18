using System;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Code.View.UI.MVVM.Titles;

public class TitlesContext : ViewModel, IEndGameTitlesUIHandler, ISubscriber, ICreditsWindowUIHandler
{
	private readonly ReactiveProperty<TitlesVM> m_TitlesVM;

	private readonly ReactiveProperty<CreditsVM> m_CreditsVM;

	private readonly Action m_ReturnToMainMenu;

	public TitlesContext(ReactiveProperty<TitlesVM> titlesVM, ReactiveProperty<CreditsVM> creditsVM, Action returnToMainMenu)
	{
		m_TitlesVM = titlesVM;
		m_CreditsVM = creditsVM;
		m_ReturnToMainMenu = returnToMainMenu;
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_TitlesVM.ClearDisposableValue();
		m_CreditsVM.ClearDisposableValue();
	}

	void IEndGameTitlesUIHandler.HandleShowEndGameTitles(bool returnToMainMenu)
	{
		m_TitlesVM.ClearDisposableValue();
		m_TitlesVM.Value = new TitlesVM(delegate
		{
			HandleOnTitlesClosed(returnToMainMenu);
		});
	}

	void ICreditsWindowUIHandler.HandleOpenCredits()
	{
		m_CreditsVM.ClearDisposableValue();
		m_CreditsVM.Value = new CreditsVM(delegate
		{
			m_CreditsVM.ClearDisposableValue();
		});
	}

	private void HandleOnTitlesClosed(bool returnToMainMenu)
	{
		m_TitlesVM.ClearDisposableValue();
		if (returnToMainMenu)
		{
			m_ReturnToMainMenu();
		}
	}
}
