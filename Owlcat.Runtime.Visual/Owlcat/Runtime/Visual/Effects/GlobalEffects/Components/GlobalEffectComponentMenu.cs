using System;

namespace Owlcat.Runtime.Visual.Effects.GlobalEffects.Components;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GlobalEffectComponentMenu : Attribute
{
	public readonly string Menu;

	public GlobalEffectComponentMenu(string menu)
	{
		Menu = menu;
	}
}
