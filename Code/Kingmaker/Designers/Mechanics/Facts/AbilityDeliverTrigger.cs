using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[Obsolete]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("26e7b647b85b4b8ebeaeff8e9b6395b9")]
public class AbilityDeliverTrigger : UnitFactComponentDelegate
{
	public enum FactionType
	{
		Any,
		Ally,
		Enemy
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ActionList m_ActionsToCaster;

	[SerializeField]
	private ActionList m_ActionsToTargets;

	[SerializeField]
	private FactionType m_Faction;
}
