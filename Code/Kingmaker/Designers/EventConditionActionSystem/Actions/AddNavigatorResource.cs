using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("126c1804a65a4391bdd281d1eb7fd260")]
[PlayerUpgraderAllowed(false)]
public class AddNavigatorResource : GameAction
{
	[SerializeField]
	public int AddCount;

	public override string GetCaption()
	{
		return "Add navigator resource";
	}

	protected override void RunAction()
	{
	}
}
