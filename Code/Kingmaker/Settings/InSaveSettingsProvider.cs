using System.Collections.Generic;

namespace Kingmaker.Settings;

public class InSaveSettingsProvider : DictionarySettingsProvider
{
	protected override Dictionary<string, object> Dictionary => Game.Instance.State.InGameSettings.List;

	public InSaveSettingsProvider()
	{
		IsEmpty = Dictionary.Count == 0;
	}

	public override void SaveAll()
	{
	}
}
