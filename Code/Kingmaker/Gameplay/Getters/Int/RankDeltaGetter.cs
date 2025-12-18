using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Getters.Int;

[Serializable]
[TypeId("e96c19b95fab431a9b7561d03feb6b77")]
public sealed class RankDeltaGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Rank delta of buff (for BuffTrigger only)";
	}

	protected override int GetBaseValue()
	{
		return SimpleContextData<int, Buff.Scope.RankDelta>.Current;
	}
}
