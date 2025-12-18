using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISettingsDescriptionUIHandler : ISubscriber
{
	void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null);

	void HandleHideSettingsDescription();
}
