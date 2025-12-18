using System;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("e60b053256f5cf545b5075ee4c00f616")]
public class DestroyStarship : ContextAction
{
	[SerializeField]
	private bool NoLog;

	[SerializeField]
	private bool NoExp;

	public override string GetCaption()
	{
		return "Destroy owner starship";
	}

	protected override void RunAction()
	{
	}
}
