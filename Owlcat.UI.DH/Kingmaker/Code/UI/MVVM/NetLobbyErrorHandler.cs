using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public class NetLobbyErrorHandler : INetLobbyErrorHandler, ISubscriber, INetGameStartHandler, IDisposable
{
	private static UINetLobbyErrorsTexts ErrorsTexts => UIStrings.Instance.NetLobbyErrorsTexts;

	public NetLobbyErrorHandler()
	{
		EventBus.Subscribe(this);
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleStoreNotInitializedError()
	{
		UtilityNet.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.StoreNotInitializedError));
	}

	public void HandleGetAuthDataTimeout()
	{
		UtilityNet.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.GetAuthDataTimeout));
	}

	public void HandleGetAuthDataError(string errorMessage)
	{
		UtilityNet.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.GetAuthDataError) + " " + errorMessage);
	}

	public void HandleChangeRegionError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.ChangeRegionError));
	}

	public void HandleNoPlayStationPlusError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.NoPlaystationPlusError));
	}

	public void HandleLobbyNotFoundError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.LobbyNotFoundError));
	}

	public void HandleLobbyFullError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.LobbyFullError));
	}

	public void HandleJoinLobbyError(int returnCode)
	{
		Show($"{ErrorsTexts.GetErrorMessage(NetLobbyErrorType.JoinLobbyError)} [{returnCode}]");
	}

	public void HandleCreatingLobbyError(short returnCode)
	{
		Show($"{ErrorsTexts.GetErrorMessage(NetLobbyErrorType.CreatingLobbyError)} [{returnCode}]");
	}

	public void HandlePhotonCustomAuthenticationFailedError()
	{
		UtilityNet.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.PhotonCustomAuthenticationFailedError));
	}

	public void HandleUnknownException()
	{
		UtilityNet.ShowReconnectDialog(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.UnknownException));
	}

	public void HandleStartGameFailed()
	{
		Show(UIStrings.Instance.NetLobbyTexts.IsNotEnoughPlayersForGame);
	}

	private static void Show(string text, Action onCloseAction = null, string yesLabel = null)
	{
		UtilityMessageBox.ShowMessageBox(text, DialogMessageBoxType.Message, delegate
		{
			onCloseAction?.Invoke();
		}, null, yesLabel);
	}
}
