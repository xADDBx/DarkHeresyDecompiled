using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete("Defence")]
[TypeId("8d76c080808a4cfca3e4e6f96204777b")]
public class AddAutoDodgeBeforeAttack : UnitFactComponentDelegate
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool AutoDodge { get; set; }
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList ActionsOnAutoDodgeAttacker;

	public ActionList ActionsOnAutoDodgeDefender;
}
