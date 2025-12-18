using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public sealed class CombatMessageExpiredAbility : CombatMessageBase
{
	public string Name;

	public Sprite Sprite;

	public Color? Color;

	public override bool GetStrikethrough()
	{
		return true;
	}

	public override string GetText()
	{
		return Name;
	}

	public override Color? GetColor()
	{
		return Color;
	}

	public override bool GetAttention()
	{
		return true;
	}

	public override Sprite GetSprite()
	{
		return Sprite;
	}
}
