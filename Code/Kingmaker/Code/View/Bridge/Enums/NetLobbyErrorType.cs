using OwlPack.Runtime;

namespace Kingmaker.Code.View.Bridge.Enums;

[OwlPackOldName("Kingmaker.Code.UI.MVVM.NetLobbyErrorHandler+NetLobbyErrorType, Code")]
public enum NetLobbyErrorType
{
	StoreNotInitializedError,
	GetAuthDataTimeout,
	GetAuthDataError,
	ChangeRegionError,
	LobbyNotFoundError,
	LobbyFullError,
	JoinLobbyError,
	CreatingLobbyError,
	PhotonCustomAuthenticationFailedError,
	SaveSourceDisconnectedError,
	SaveReceiveError,
	SaveNotFoundError,
	SendMessageFailError,
	NoPlaystationPlusError,
	UnknownException
}
