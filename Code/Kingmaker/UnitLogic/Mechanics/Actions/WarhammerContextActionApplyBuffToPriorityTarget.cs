using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("2de0227f2d0b4e8994f31f8401c579aa")]
public class WarhammerContextActionApplyBuffToPriorityTarget : ContextAction
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public bool Permanent;

	public ContextDurationValue DurationValue;

	[SerializeField]
	private BlueprintBuffReference m_TargetBuff;

	public BlueprintBuff Buff => m_Buff?.Get();

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		string text = "Apply Buff to priority target: " + (Buff.NameSafe() ?? "???");
		if (Permanent)
		{
			return text + " (permanent)";
		}
		string text2 = DurationValue.ToString();
		return text + " (for " + text2 + ")";
	}
}
