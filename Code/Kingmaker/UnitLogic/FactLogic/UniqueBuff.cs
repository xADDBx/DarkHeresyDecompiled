using System;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[TypeId("810d12da0027ec94c87f00f0b7fb0b38")]
public class UniqueBuff : UnitBuffComponentDelegate
{
	protected override void OnActivate()
	{
		base.Buff.Context.MaybeCaster?.GetOrCreate<UnitPartUniqueBuffs>().NewBuff(base.Buff);
	}

	protected override void OnDeactivate()
	{
		base.Buff.Context.MaybeCaster?.GetOptional<UnitPartUniqueBuffs>()?.RemoveBuff(base.Buff);
	}
}
