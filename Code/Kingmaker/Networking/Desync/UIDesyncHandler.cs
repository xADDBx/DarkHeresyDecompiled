using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Networking.Hash;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Networking.Desync;

public class UIDesyncHandler : IDesyncHandler
{
	public void RaiseDesync(HashableState data, DesyncMeta meta)
	{
		UtilityMessageBox.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.DesyncWasDetected, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
		{
			if (button == DialogMessageBoxButton.Yes)
			{
				EventBus.RaiseEvent(delegate(INetLobbyRequest h)
				{
					h.HandleNetLobbyRequest();
				});
			}
		});
	}
}
