using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageDamage : CombatMessageBase
{
	public int Amount;

	public Sprite Sprite;

	public bool IsVital;

	public bool IsImmune;

	public bool IsEnemy;

	public bool HasHPDamageBonus;

	public bool HasArmorDamageBonus;

	public bool HasCriticalEffect;

	public Vector3 SourcePosition;

	public Vector3 TargetPosition;

	public override string GetText()
	{
		string text = Amount.ToString();
		if (IsVital)
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
		return IsVital;
	}
}
