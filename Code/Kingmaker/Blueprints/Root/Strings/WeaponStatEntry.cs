using System;
using Kingmaker.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class WeaponStatEntry
{
	public WeaponStat Stat;

	public LocalizedString Text;
}
