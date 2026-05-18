using Core.Cheats;

namespace Kingmaker.Localization;

public static class VOTextHelper
{
	public static bool IsActiveExportToVoiceOver;

	[Cheat(Name = "set_export_to_voice_over", Description = "Включить тест работы текстов в режиме ВО")]
	public static void SetExportToVoiceOver(bool isOn)
	{
		IsActiveExportToVoiceOver = isOn;
	}
}
