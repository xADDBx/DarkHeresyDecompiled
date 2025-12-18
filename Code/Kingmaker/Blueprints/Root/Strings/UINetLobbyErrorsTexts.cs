using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UINetLobbyErrorsTexts
{
	public LocalizedString StoreNotInitializedErrorMessage;

	public LocalizedString GetAuthDataTimeoutMessage;

	public LocalizedString GetAuthDataErrorMessage;

	public LocalizedString ChangeRegionErrorMessage;

	public LocalizedString LobbyNotFoundErrorMessage;

	public LocalizedString LobbyFullErrorMessage;

	public LocalizedString JoinLobbyErrorMessage;

	public LocalizedString CreatingLobbyErrorMessage;

	public LocalizedString PhotonDisconnectedErrorMessage;

	public LocalizedString PhotonCustomAuthenticationFailedErrorMessage;

	public LocalizedString SaveSourceDisconnectedErrorMessage;

	public LocalizedString SaveReceiveErrorMessage;

	public LocalizedString SaveNotFoundErrorMessage;

	public LocalizedString SendMessageFailErrorMessage;

	public LocalizedString NoPlaystationPlusErrorMessage;

	public LocalizedString UnknownExceptionMessage;

	public string GetErrorMessage(NetLobbyErrorType type)
	{
		return type switch
		{
			NetLobbyErrorType.StoreNotInitializedError => StoreNotInitializedErrorMessage, 
			NetLobbyErrorType.GetAuthDataTimeout => GetAuthDataTimeoutMessage, 
			NetLobbyErrorType.GetAuthDataError => GetAuthDataErrorMessage, 
			NetLobbyErrorType.ChangeRegionError => ChangeRegionErrorMessage, 
			NetLobbyErrorType.LobbyNotFoundError => LobbyNotFoundErrorMessage, 
			NetLobbyErrorType.LobbyFullError => LobbyFullErrorMessage, 
			NetLobbyErrorType.JoinLobbyError => JoinLobbyErrorMessage, 
			NetLobbyErrorType.CreatingLobbyError => CreatingLobbyErrorMessage, 
			NetLobbyErrorType.PhotonCustomAuthenticationFailedError => PhotonCustomAuthenticationFailedErrorMessage, 
			NetLobbyErrorType.SaveSourceDisconnectedError => SaveSourceDisconnectedErrorMessage, 
			NetLobbyErrorType.SaveReceiveError => SaveReceiveErrorMessage, 
			NetLobbyErrorType.SaveNotFoundError => SaveNotFoundErrorMessage, 
			NetLobbyErrorType.SendMessageFailError => SendMessageFailErrorMessage, 
			NetLobbyErrorType.NoPlaystationPlusError => NoPlaystationPlusErrorMessage, 
			_ => UnknownExceptionMessage, 
		};
	}
}
