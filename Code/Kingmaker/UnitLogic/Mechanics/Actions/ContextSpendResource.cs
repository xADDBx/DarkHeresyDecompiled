using System;
using Kingmaker.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("b9fdbab5064d5434c97f18f2550d3741")]
public class ContextSpendResource : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Resource")]
	private BlueprintAbilityResourceReference m_Resource;

	public bool ContextValueSpendure;

	[ShowIf("ContextValueSpendure")]
	public ContextValue Value;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	public override string GetCaption()
	{
		return "Spend resourse";
	}

	protected override void RunAction()
	{
	}
}
