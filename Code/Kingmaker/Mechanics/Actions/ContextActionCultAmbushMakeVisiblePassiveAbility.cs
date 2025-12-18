using System;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[Obsolete]
[TypeId("f39fb0dcf8a742c28e1187843821e7f2")]
public class ContextActionCultAmbushMakeVisiblePassiveAbility : ContextAction
{
	public enum Actors
	{
		Owner,
		Caster
	}

	[SerializeField]
	private Actors m_Actor;

	public override string GetCaption()
	{
		return "CultAmbushMakeVisiblePassiveAbility";
	}

	protected override void RunAction()
	{
	}
}
