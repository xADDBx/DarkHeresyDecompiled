using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageDamage : CombatMessageBase
{
	public enum DamageBonus
	{
		None,
		Hp,
		Armor,
		Vital,
		Critical
	}

	public int Amount;

	public Sprite Sprite;

	public bool IsImmune;

	public DamageBonus Bonus;

	public Vector3 SourcePosition;

	public Vector3 TargetPosition;

	public override string GetText()
	{
		string text = Amount.ToString();
		if (Bonus == DamageBonus.Vital)
		{
			return text + "!";
		}
		if (IsImmune)
		{
			return text + " " + GameLogStrings.Instance.Damage.DamageImmune.Text;
		}
		return text;
	}

	public override Color? GetColor()
	{
		return ConfigRoot.Instance.UIConfig.CombatTextColors.DamageColor;
	}

	public override Sprite GetSprite()
	{
		return Sprite;
	}

	public override bool GetAttention()
	{
		return Bonus == DamageBonus.Vital;
	}
}
