using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("de19bd4eb40decb419caec8194765ed0")]
public class ContextActionSkillCheck_Obsolete : ContextAction
{
	[Serializable]
	private struct ConditionalDCIncrease
	{
		public ConditionsChecker Condition;

		public ContextValue Value;
	}

	public StatType Stat;

	[SerializeField]
	private ConditionalDCIncrease[] m_ConditionalDCIncrease = new ConditionalDCIncrease[0];

	public bool UseCustomDC;

	[ShowIf("UseCustomDC")]
	public ContextValue CustomDC;

	public bool CalculateDCDifference;

	public ActionList Success;

	[HideIf("CalculateDCDifference")]
	public ActionList Failure;

	[ShowIf("CalculateDCDifference")]
	public ActionList FailureDiffMoreOrEqual5Less10;

	[ShowIf("CalculateDCDifference")]
	public ActionList FailureDiffMoreOrEqual10;

	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		return string.Format("Skill check {0} {1}", Stat, UseCustomDC ? $"(DC: {CustomDC})" : "");
	}
}
