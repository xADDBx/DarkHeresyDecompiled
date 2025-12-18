using System;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("d89d9a7a51e048c680a95ebd444879b7")]
public class TargetHealModifier : UnitFactComponentDelegate
{
	public ContextValue FlatBonus;

	public ContextValue PercentBonus;

	public bool OnlyAgainstTargetsWithHalfOrLessHealth;
}
