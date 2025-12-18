using System;
using Kingmaker.EntitySystem.Stats.Base;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.DodgeChance;

[Obsolete]
[TypeId("0b4684d3bfe5495bb8e279792b7f961b")]
public class WarhammerDodgeChanceStatReplacement : WarhammerDodgeChanceModifier
{
	public StatType AgilityReplacementStat = StatType.Agility;
}
