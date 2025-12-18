using System;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[TypeId("2d16a086cc16415a8016057b2c28a39e")]
public class MultiplyIncomingDamageBonus : UnitBuffComponentDelegate
{
	public float PercentIncreaseMultiplier = 1f;
}
