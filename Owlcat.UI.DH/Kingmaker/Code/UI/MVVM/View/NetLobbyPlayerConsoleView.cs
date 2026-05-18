using Owlcat.UI;
using R3;

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

	public void AddPlayerInput()
	{
	}

	public void AddGamerTagInput()
	{
	}
}
