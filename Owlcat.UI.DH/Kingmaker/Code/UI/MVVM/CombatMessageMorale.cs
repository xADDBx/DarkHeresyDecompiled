using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageMorale : CombatMessageBase
{
	public int Amount;

	public bool HasCriticalEffect;

	public override string GetText()
	{
		if (!HasCriticalEffect)
		{
			return Amount.ToString("+0;-#");
		}
		return $"{Amount:+0;-#} [{UIStrings.Instance.CombatTexts.CriticallyInjured.Text}]";
	}

	public override Sprite GetSprite()
	{
		return UIConfig.Instance.UIIcons.CombatMessageMorale;
	}

	public override bool GetAttention()
	{
		return true;
	}
}
