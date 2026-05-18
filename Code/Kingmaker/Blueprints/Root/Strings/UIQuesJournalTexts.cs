using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIQuesJournalTexts
{
	public LocalizedString NoData;

	public LocalizedString ShowCompletedQuests;

	public LocalizedString HideCompletedQuests;

	public LocalizedString Quests;

	public LocalizedString OpenCase;

	public string GetActiveTabLabel(JournalTab tab)
	{
		if (tab == JournalTab.Quests)
		{
			return Quests;
		}
		return string.Empty;
	}
}
