using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[Obsolete]
[TypeId("daf0d5bfc550ca542884d8ebc63df72f")]
public class LastAttackPosition : PositionEvaluator
{
	[ValidateNotNull]
	public MechanicEntityEvaluator Entity;

	public override string GetCaption()
	{
		return $"Last attack position of {Entity}";
	}

	protected override Vector3 GetValueInternal()
	{
		return Entity.GetValue().GetLastAttackPosition() ?? Entity.GetValue().Position;
	}
}
