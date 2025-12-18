using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[AllowMultipleComponents]
[TypeId("9e9202ec6aca44728f3dfff49ce23e20")]
public class SpawnBlood : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	protected override void RunAction()
	{
		throw new NotImplementedException();
	}

	public override string GetCaption()
	{
		return $"Spawn FX blood on ({Target})";
	}
}
