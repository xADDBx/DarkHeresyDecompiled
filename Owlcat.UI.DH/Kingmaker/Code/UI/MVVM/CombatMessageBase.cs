using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class CombatMessageBase
{
	public abstract string GetText();

	public virtual Color? GetColor()
	{
		return null;
	}

	public virtual bool GetAttention()
	{
		return false;
	}

	public virtual Sprite GetSprite()
	{
		return null;
	}

	public virtual Sprite GetBigSprite()
	{
		return null;
	}

	public virtual bool GetStrikethrough()
	{
		return false;
	}
}
