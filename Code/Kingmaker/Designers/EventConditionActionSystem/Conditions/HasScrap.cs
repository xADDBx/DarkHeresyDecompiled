using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("d6b0a46b654c71945a3a53f1e98347a4")]
public class HasScrap : Condition
{
	[SerializeField]
	private int scrapRequired;

	protected override string GetConditionCaption()
	{
		return $"Player has at least {scrapRequired} scrap";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
