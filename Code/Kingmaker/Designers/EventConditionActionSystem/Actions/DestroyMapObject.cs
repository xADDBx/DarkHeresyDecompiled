using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete("Unused mechanic")]
[ComponentName("Actions/DestroyMapObject")]
[AllowMultipleComponents]
[TypeId("10b37ccc7a0511b4ba6c4cbf72b22f76")]
public class DestroyMapObject : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		return "Destroy map-object " + MapObject?.GetCaption();
	}
}
