using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Conditions;

[Obsolete]
[TypeId("cff81f02c313ef244b5a1b79c9068e83")]
public abstract class BuffCondition : Condition
{
	protected Buff Buff => null;
}
