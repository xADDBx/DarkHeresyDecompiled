using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickPortraitFeaturesVM : TooltipBrickVM
{
	public readonly string Name;

	public readonly bool Available;

	public readonly string AvailableText;

	public readonly Sprite Portrait;

	public readonly List<Ability> DesperateMeasureAbilities;

	public readonly List<Ability> HeroicActAbilities;

	public BrickPortraitFeaturesVM(string name, bool available, string availableText, Sprite portrait, List<Ability> desperateMeasureAbilities, List<Ability> heroicActAbilities)
	{
		Name = name;
		Available = available;
		AvailableText = availableText;
		Portrait = portrait;
		DesperateMeasureAbilities = desperateMeasureAbilities;
		HeroicActAbilities = heroicActAbilities;
	}
}
