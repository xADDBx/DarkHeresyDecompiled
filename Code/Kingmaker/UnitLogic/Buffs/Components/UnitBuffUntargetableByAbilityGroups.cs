using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[ComponentName("Target Restriction/UnitBuffUntargetableByAbilityGroups")]
[TypeId("4804457c2d8b4eac8e9fa5a6d9de486b")]
[ClassInfoBox("Unit is untargetable by single target abilities having one of blocked ability groups")]
public class UnitBuffUntargetableByAbilityGroups : UnitBuffComponentDelegate
{
	[SerializeField]
	private List<BlueprintAbilityGroupReference> m_BlockedGroups;

	public List<BlueprintAbilityGroupReference> BlockedGroups => m_BlockedGroups;
}
