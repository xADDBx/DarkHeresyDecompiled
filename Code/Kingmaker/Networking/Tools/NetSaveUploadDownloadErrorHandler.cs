using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Networking.Tools;

public class NetSaveUploadDownloadErrorHandler : INetSaveUploadDownloadErrorHandler, ISubscriber
{
	private UINetLobbyErrorsTexts ErrorsTexts => UIStrings.Instance.NetLobbyErrorsTexts;

	public NetSaveUploadDownloadErrorHandler()
	{
		EventBus.Subscribe(this);
	}

	public void HandleSaveSourceDisconnectedError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.SaveSourceDisconnectedError));
	}

	public void HandleSaveReceiveError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.SaveReceiveError));
	}

	public void HandleSaveNotFoundError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.SaveNotFoundError));
	}

	public void HandleSendMessageFailError()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.SendMessageFailError));
	}

	public void HandleUnknownException()
	{
		Show(ErrorsTexts.GetErrorMessage(NetLobbyErrorType.UnknownException));
	}

	private void Show(string text)
	{
		UtilityMessageBox.ShowMessageBox(text, DialogMessageBoxType.Message, delegate
		{
		});
	}
}
