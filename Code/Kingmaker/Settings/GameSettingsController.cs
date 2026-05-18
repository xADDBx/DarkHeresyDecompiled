using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Settings;

public class GameSettingsController
{
	public GameSettingsController()
	{
		SettingsRoot.Game.Main.SendGameStatistic.OnValueChanged += SendStatisticOnChanged;
		SettingsRoot.Game.Main.BloodOnCharacters.OnValueChanged += delegate
		{
			EventBus.RaiseEvent(delegate(IBloodSettingsHandler h)
			{
				h.HandleBloodSettingChanged();
			});
		};
	}

	private void SendStatisticOnChanged(bool sendStatistics)
	{
		if (sendStatistics)
		{
			Metrics.StartDataCollection();
		}
		else
		{
			Metrics.StopDataCollection();
		}
	}
}
