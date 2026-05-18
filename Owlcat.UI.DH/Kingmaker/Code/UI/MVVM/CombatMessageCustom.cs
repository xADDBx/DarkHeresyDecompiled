using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CombatMessageCustom : CombatMessageBase
{
	public string Text;

	public Color? Color;

	public override string GetText()
	{
		return Text;
	}

	public override Color? GetColor()
	{
		return Color;
	}
}
