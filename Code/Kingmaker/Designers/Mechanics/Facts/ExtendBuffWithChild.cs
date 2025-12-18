using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete("Unused")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("4a6f362c4d6bff24c83ef48f157dcab7")]
public class ExtendBuffWithChild : UnitFactComponentDelegate
{
	[SerializeField]
	private BlueprintBuffReference m_parenttBuff;

	[SerializeField]
	private BlueprintBuffReference m_childBuff;

	public BlueprintBuff ParentBuff => m_parenttBuff?.Get();

	public BlueprintBuff ChildBuff => m_childBuff?.Get();
}
