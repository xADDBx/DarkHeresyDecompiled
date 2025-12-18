using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class NetLobbyInvitePlayerDifferentPlatformsVM : ViewModel
{
	private readonly Action m_CloseAction;

	private readonly Action m_InvitePlayer;

	private readonly Action m_InviteEpicGamesPlayer;

	public NetLobbyInvitePlayerDifferentPlatformsVM(Action closeAction, Action invitePlayerAction, Action inviteEpicGamesPlayerAction)
	{
		m_CloseAction = closeAction;
		m_InvitePlayer = invitePlayerAction;
		m_InviteEpicGamesPlayer = inviteEpicGamesPlayerAction;
	}

	public void OnClose()
	{
		m_CloseAction();
	}

	public void OnInvitePlayer()
	{
		m_InvitePlayer();
	}

	public void OnInviteEpicGamesPlayer()
	{
		m_InviteEpicGamesPlayer();
	}
}
