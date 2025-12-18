using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[Obsolete]
[TypeId("cae5fcf97ef49da4eb7a70a0e181d81c")]
public class StarshipUltimateAbilityBuff : ContextAction
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetCaption()
	{
		return "Apply " + Buff.NameSafe() + " with post officer skill dependant duration";
	}

	protected override void RunAction()
	{
	}
}
