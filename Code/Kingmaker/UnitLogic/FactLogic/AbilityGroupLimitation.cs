using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete("AddAbilityRestriction")]
[AllowMultipleComponents]
[TypeId("17b7e5e565e0db44d972ec8999f57106")]
public class AbilityGroupLimitation : UnitFactComponentDelegate
{
	[SerializeField]
	private BlueprintAbilityGroupReference m_Group;
}
