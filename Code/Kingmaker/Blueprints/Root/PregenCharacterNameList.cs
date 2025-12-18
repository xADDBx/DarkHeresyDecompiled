using System;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class PregenCharacterNameList
{
	public Race Race;

	public Gender Gender;

	public CharGenMode CharGenMode;

	public LocalizedString NameList;
}
