using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete]
[TypeId("a6fb48de4771436fa5d6db25ba686fd1")]
public class InitiatorHealModifier : UnitFactComponentDelegate
{
	public int FlatBonus;

	public int PercentBonus;

	public bool OnlyAgainstTargetsWithHalfOrLessHealth;
}
