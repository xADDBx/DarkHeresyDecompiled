using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.Traps;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Obsolete]
[TypeId("c893f3c91a569bd4cbc08f766c6b0655")]
public class TrapTrigger : EntityFactComponentDelegate, IDisarmTrapHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, ITrapActivationHandler
{
	[SerializeReference]
	public MapObjectEvaluator Trap;

	public ActionList OnActivation;

	public ActionList OnDisarm;

	public void HandleDisarmTrapSuccess(TrapObjectData trap)
	{
		if (EqualsToReferenced(trap))
		{
			OnDisarm.Run();
		}
	}

	public void HandleDisarmTrapFail(TrapObjectData trap)
	{
	}

	public void HandleDisarmTrapCriticalFail(TrapObjectData trap)
	{
	}

	public void HandleTrapActivation(TrapObjectData trap)
	{
		if (EqualsToReferenced(trap))
		{
			OnActivation.Run();
		}
	}

	private bool EqualsToReferenced(TrapObjectData trap)
	{
		if (Trap.TryGetValue(out var value))
		{
			return value.UniqueId == trap.UniqueId;
		}
		return false;
	}
}
