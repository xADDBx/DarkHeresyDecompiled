using System;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyPlayerConsoleView : NetLobbyPlayerBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	private readonly ReactiveProperty<bool> m_IsFocused = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsFocused => m_IsFocused;

	public void SetFocus(bool value)
	{
		m_IsFocused.Value = value;
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		if (m_MainButton.Interactable)
		{
			return m_MainButton.IsActive();
		}
		return false;
	}

	public void InvitePlayer()
	{
		if (base.InviteButtonInteractable.CurrentValue)
		{
			base.ViewModel.InviteFromPrimaryStore();
		}
	}

	public void InviteEpicPlayer()
	{
		ReadOnlyReactiveProperty<bool> epicGamesAuthorized = base.ViewModel.EpicGamesAuthorized;
		if (epicGamesAuthorized == null || !epicGamesAuthorized.CurrentValue)
		{
			InvitePlayer();
		}
		else if (base.ViewModel.EpicGamesAuthorized.CurrentValue && base.InviteButtonInteractable.CurrentValue)
		{
			base.ViewModel.InviteFromSecondaryStore();
		}
	}

	public void KickPlayer()
	{
		if (base.KickButtonInteractable.CurrentValue && base.ViewModel.IsMeHost.CurrentValue)
		{
			base.ViewModel.Kick();
		}
	}

	public void AddPlayerInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, Action showGamersTagModeAction, ReadOnlyReactiveProperty<bool> canConfirmLaunch)
	{
		hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			InviteEpicPlayer();
		}, 10, base.InviteButtonInteractable.And(IsFocused).And(base.ViewModel.EpicGamesAuthorized).And(canConfirmLaunch.Not())
			.ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.InviteEpicGamesPlayer).AddTo(this);
		hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			InvitePlayer();
		}, 8, base.InviteButtonInteractable.And(IsFocused).And(canConfirmLaunch.Not()).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.InvitePlayer).AddTo(this);
		hintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			KickPlayer();
		}, 11, base.KickButtonInteractable.And(IsFocused).And(base.ViewModel.IsMeHost).And(canConfirmLaunch.Not())
			.ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased), UIStrings.Instance.NetLobbyTexts.KickPlayer);
	}

	public void AddGamerTagInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, Action hideGamersTagModeAction, ReadOnlyReactiveProperty<bool> canConfirmLaunch)
	{
		m_GamerTagAndName.AddGamerTagInput(inputLayer, hintsWidget, hideGamersTagModeAction, canConfirmLaunch);
	}
}
