using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIServiceWindows
{
	public LocalizedString Inventory;

	public LocalizedString Character;

	public LocalizedString Journal;

	public LocalizedString Reputation;

	public LocalizedString DetectiveJournal;

	public LocalizedString LocalMap;

	public LocalizedString Encyclopedia;

	public LocalizedString Unknown;

	public LocalizedString GetTitle(ServiceWindowsType type)
	{
		return type switch
		{
			ServiceWindowsType.Inventory => Inventory, 
			ServiceWindowsType.CharacterInfo => Character, 
			ServiceWindowsType.Journal => Journal, 
			ServiceWindowsType.Reputation => Reputation, 
			ServiceWindowsType.DetectiveJournal => DetectiveJournal, 
			ServiceWindowsType.LocalMap => LocalMap, 
			ServiceWindowsType.Encyclopedia => Encyclopedia, 
			_ => Unknown, 
		};
	}
}
