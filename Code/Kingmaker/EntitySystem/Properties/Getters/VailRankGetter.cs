using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("718e0c48c4a64be8a64a127f85f40511")]
public class VailRankGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Veil value (number)";
	}

	protected override int GetBaseValue()
	{
		return Game.Instance.LoadedArea.Veil.Damage;
	}
}
