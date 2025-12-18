using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Blueprints.Root;

[Serializable]
[ComponentName("Root/SORoot")]
[TypeId("8a3a79e56f9b411db39b12fadbebbabd")]
public class SORoot : BlueprintScriptableObject
{
	public UISettingsRoot UISettingsRoot;

	public CursorRoot Cursors;

	public SettingsValues SettingsValues;

	public DifficultyPresetsList DifficultyList;

	public CalendarRoot Calendar;
}
