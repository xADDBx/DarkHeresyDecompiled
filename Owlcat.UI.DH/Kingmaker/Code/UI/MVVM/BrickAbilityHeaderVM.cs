using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic.Levelup.Selections;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityHeaderVM : TooltipBrickVM
{
	public enum AbilityToggleState : byte
	{
		None,
		On,
		Off
	}

	public readonly string AbilityName;

	public readonly string AbilityType;

	public readonly Sprite AbilityIcon;

	public readonly string Acronym;

	public readonly string APText;

	public readonly int APCount;

	public readonly TalentIconInfo TalentIconInfo;

	public Sprite ModifierIcon { get; private set; }

	public AbilityToggleState ToggleState { get; private set; }

	public BrickAbilityHeaderVM(string abilityName, string abilityType, Sprite abilityIcon, int apCount = 0, string acronym = null, TalentIconInfo talentIconInfo = null)
	{
		AbilityName = abilityName;
		AbilityType = abilityType;
		AbilityIcon = abilityIcon;
		Acronym = acronym;
		APCount = apCount;
		APText = UIStrings.Instance.Tooltips.AP;
		TalentIconInfo = talentIconInfo;
	}

	public BrickAbilityHeaderVM SetToggleState(bool isOn)
	{
		ToggleState = (isOn ? AbilityToggleState.On : AbilityToggleState.Off);
		return this;
	}

	public BrickAbilityHeaderVM SetModifierIcon(Sprite modifierIcon)
	{
		ModifierIcon = modifierIcon;
		return this;
	}
}
