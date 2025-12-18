using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("08e1e72d39371a242b5c5c88db0f4f75")]
public class StarshipProbabilisticAction : ContextAction
{
	[Range(0f, 1f)]
	public float probability = 1f;

	public bool modifyWithOwnerHPPercent;

	[ShowIf("modifyWithOwnerHPPercent")]
	[Range(0f, 1f)]
	public float minHpPercent;

	[ShowIf("modifyWithOwnerHPPercent")]
	[Range(0f, 1f)]
	public float maxHpPercent = 1f;

	[ShowIf("modifyWithOwnerHPPercent")]
	[Range(0f, 1f)]
	public float probModAtMin = 1f;

	[ShowIf("modifyWithOwnerHPPercent")]
	[Range(0f, 1f)]
	public float probModAtMax = 1f;

	public ActionList Actions;

	public ActionList FailActions;

	public override string GetCaption()
	{
		string text = $"{probability * 100f}% chances";
		if (modifyWithOwnerHPPercent)
		{
			text += ", modified with owner HP left,";
		}
		if (Actions.Actions.Length != 0)
		{
			text = text + " to " + Actions.Actions[0].GetCaption();
		}
		return text;
	}

	protected override void RunAction()
	{
	}
}
