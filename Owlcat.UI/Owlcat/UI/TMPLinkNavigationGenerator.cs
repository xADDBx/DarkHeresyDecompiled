using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Owlcat.UI;

public static class TMPLinkNavigationGenerator
{
	public static List<IFloatConsoleNavigationEntity> GenerateEntityList(TextMeshProUGUI text, IConsoleNavigationEntity firstFocus, IConsoleNavigationEntity secondFocus, Action<string> onLinkClicked, Action<string> onLinkFocused, Func<string, TooltipBaseTemplate> getTooltipTemplate)
	{
		return (from glossaryEntity in GlossaryPointsUtils.GetLinksCoordinatesDictionary(text)
			select new TMPLinkNavigationEntity(text, firstFocus, secondFocus, glossaryEntity, onLinkClicked, onLinkFocused, getTooltipTemplate)).Cast<IFloatConsoleNavigationEntity>().ToList();
	}

	public static List<IFloatConsoleNavigationEntity> GenerateEntityList<TLinkData>(TextMeshProUGUI text, IConsoleNavigationEntity firstFocus, IConsoleNavigationEntity secondFocus, Action<string, TLinkData> onLinkClicked, Action<string, TLinkData> onLinkFocused, TLinkData linkData, Func<string, TLinkData, TooltipBaseTemplate> getTooltipTemplate)
	{
		return (from glossaryEntity in GlossaryPointsUtils.GetLinksCoordinatesDictionary(text)
			select new TMPLinkNavigationEntityAdvanced<TLinkData>(text, firstFocus, secondFocus, glossaryEntity, linkData, onLinkClicked, onLinkFocused, getTooltipTemplate)).Cast<IFloatConsoleNavigationEntity>().ToList();
	}
}
