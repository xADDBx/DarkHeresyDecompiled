using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.DodgeChance;

[Obsolete]
[TypeId("469780bd4d77420b8aa5ad36daca3561")]
public class WarhammerGlobalDodgeChanceModifier : WarhammerDodgeChanceModifier
{
	public bool OnlyOnEnemies;

	public bool OnlyOnAllies;

	public bool OnlyOnTargetsInLineOfSight;

	public bool OnlyAgainstAllies;
}
