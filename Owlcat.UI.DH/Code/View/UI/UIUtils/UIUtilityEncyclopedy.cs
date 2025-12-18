using Kingmaker;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.Utils;
using Kingmaker.Gameplay.Features.Reputation;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;

namespace Code.View.UI.UIUtils;

public class UIUtilityEncyclopedy
{
	public static bool IsPossibleGoToEncyclopedia
	{
		get
		{
			if (GameUIState.Instance.IsInMainMenu)
			{
				return false;
			}
			return true;
		}
	}

	public static void ShowEncyclopediaPage(string pageKey)
	{
		ShowEncyclopediaPage(ChapterList.GetPage(pageKey));
	}

	public static void ShowEncyclopediaPage(INode page)
	{
		if (!IsPossibleGoToEncyclopedia)
		{
			return;
		}
		Game.Instance.Player.UISettings.CurrentEncyclopediaPage = page as IPage;
		if (RootUIContext.Instance.CurrentServiceWindow != ServiceWindowsType.Encyclopedia)
		{
			EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
			{
				h.HandleOpenEncyclopedia(page);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IEncyclopediaHandler h)
			{
				h.HandleEncyclopediaPage(page);
			});
		}
	}

	public static BlueprintEncyclopediaGlossaryEntry GetGlossaryEntry(string key)
	{
		return UtilityEncyclopedy.GetGlossaryEntry(key);
	}

	public static BlueprintEncyclopediaEntry GetEncyclopediaEntry(string key)
	{
		return UtilityEncyclopedy.GetEncyclopediaEntry(key);
	}

	public static string GetGlossaryEntryName(string key)
	{
		return UtilityEncyclopedy.GetGlossaryEntryName(key);
	}

	public static string GetFactionEncyclopediaKey(FactionType factionType)
	{
		return UtilityEncyclopedy.GetFactionEncyclopediaKey(factionType);
	}
}
