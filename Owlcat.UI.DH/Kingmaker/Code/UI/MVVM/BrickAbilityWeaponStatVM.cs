using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityWeaponStatVM : TooltipBrickVM
{
	public readonly string StatName;

	public readonly string StatValue;

	public readonly Sprite StatIcon;

	public BrickAbilityWeaponStatVM(string statName, Sprite statIcon, string statValue)
	{
		StatName = statName;
		StatIcon = statIcon;
		StatValue = statValue;
	}
}
