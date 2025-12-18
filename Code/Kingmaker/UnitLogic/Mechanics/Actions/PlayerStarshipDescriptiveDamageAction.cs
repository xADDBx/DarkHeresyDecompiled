using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[ComponentName("Actions/PlayerStarshipDescriptiveDamageAction")]
[AllowMultipleComponents]
[TypeId("3ed9f542a3b83084ab7fc16a9d94d9c2")]
public class PlayerStarshipDescriptiveDamageAction : GameAction
{
	private enum DamageValue
	{
		Tiny,
		VeryLow,
		Low,
		Average,
		High
	}

	[SerializeField]
	private DamageValue damageValue;

	[SerializeField]
	private bool IsWarp;

	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		return $"Do \"{damageValue}\" damage to player starship";
	}
}
