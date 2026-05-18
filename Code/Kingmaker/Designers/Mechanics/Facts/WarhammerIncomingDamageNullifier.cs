using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b28e1f5ac7e12074e9457c0f862f1ae8")]
public class WarhammerIncomingDamageNullifier : UnitFactComponentDelegate
{
	[SerializeField]
	private RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ContextValue m_NullifyChances;

	[SerializeField]
	private bool m_OnlyCritical;
}
