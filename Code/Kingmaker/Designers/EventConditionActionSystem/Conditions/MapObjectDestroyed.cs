using System;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete("Unused mechanic")]
[TypeId("10d3102b1c93ce842add52534433ceba")]
public class MapObjectDestroyed : Condition
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	public override string GetDescription()
	{
		return "Is map object destroyed or not?";
	}

	protected override string GetConditionCaption()
	{
		return $"MapObject {MapObject} is destroyed";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
