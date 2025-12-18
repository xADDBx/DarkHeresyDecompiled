using System;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("a3d0709d57d248f1a751e9bd62ce5f0b")]
public class ContextActionMoraleSetLeader : ContextAction
{
	[SerializeField]
	private bool m_RemoveLeader;

	public override string GetCaption()
	{
		if (m_RemoveLeader)
		{
			return $"Remove leader status from {base.Target}";
		}
		return $"Make {base.Target} leader";
	}

	protected override void RunAction()
	{
		if (base.Target.Entity is BaseUnitEntity unit)
		{
			if (m_RemoveLeader)
			{
				Game.Instance.Controllers.MoraleController.UnregisterLeader(unit);
			}
			else
			{
				Game.Instance.Controllers.MoraleController.RegisterLeader(unit);
			}
		}
	}
}
