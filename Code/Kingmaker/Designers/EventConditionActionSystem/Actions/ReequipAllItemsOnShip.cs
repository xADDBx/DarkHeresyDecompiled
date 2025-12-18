using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("7e7f555bf3ab41c69d10ea17764526df")]
public class ReequipAllItemsOnShip : PlayerUpgraderOnlyAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public override string GetCaption()
	{
		return "Unequip all items from the target starship";
	}

	protected override void RunActionOverride()
	{
	}
}
