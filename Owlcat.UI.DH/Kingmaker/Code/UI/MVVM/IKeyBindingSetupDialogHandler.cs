using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface IKeyBindingSetupDialogHandler : ISubscriber
{
	void OpenKeyBindingSetupDialog(UISettingsEntityKeyBinding uiSettingsEntity, int bindingIndex);
}
