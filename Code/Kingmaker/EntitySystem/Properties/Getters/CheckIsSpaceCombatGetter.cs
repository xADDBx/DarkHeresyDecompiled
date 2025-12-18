using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("4515e962e6b6e6e43a2206ad6fcd0caa")]
public class CheckIsSpaceCombatGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "State is SpaceCombat";
	}
}
