using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageHealing : CombatMessageBase
{
	public int Amount;

	public Sprite Sprite;

	public override string GetText()
	{
		return UIUtilityText.AddSign(Amount);
	}

	public override Sprite GetSprite()
	{
		return Sprite;
	}

	public override Color? GetColor()
	{
		return ConfigRoot.Instance.UIConfig.CombatTextColors.HealColor;
	}
}
