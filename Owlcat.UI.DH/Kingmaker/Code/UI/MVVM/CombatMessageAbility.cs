using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public sealed class CombatMessageAbility : CombatMessageBase
{
	public string Name;

	public Sprite Sprite;

	public Sprite BigSprite;

	public Color? Color;

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

	public override Sprite GetBigSprite()
	{
		return BigSprite;
	}
}
