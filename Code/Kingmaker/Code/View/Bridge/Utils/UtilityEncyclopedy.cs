using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.Localization;

namespace Kingmaker.Code.View.Bridge.Utils;

public static class UtilityEncyclopedy
{
	public static BlueprintEncyclopediaGlossaryEntry GetGlossaryEntry(string key)
	{
		BlueprintEncyclopediaPage page = ChapterList.GetPage(key);
		if (!(page is BlueprintEncyclopediaGlossaryEntry result))
		{
			if (page != null)
			{
				return (BlueprintEncyclopediaGlossaryEntry)page.GlossaryEntry;
			}
			return null;
		}
		return result;
	}

	public static BlueprintEncyclopediaEntry GetEncyclopediaEntry(string key)
	{
		BlueprintEncyclopediaEntry entry = ChapterList.GetEntry(key);
		if (!entry)
		{
			return null;
		}
		return entry;
	}

	public static string GetGlossaryEntryName(string key)
	{
		BlueprintEncyclopediaEntry encyclopediaEntry = GetEncyclopediaEntry(key);
		if (encyclopediaEntry != null)
		{
			return encyclopediaEntry.Title;
		}
		LocalizedString localizedString = GetGlossaryEntry(key)?.Title;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}

	public static string GetFactionEncyclopediaKey(FactionType factionType)
	{
		return $"{factionType}Faction";
	}
}
