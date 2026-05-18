using System;
using Kingmaker.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
[TypeId("bab71994eeaf407ebbafcb45b4c5c4fd")]
public class UnitEvaluatorHolder : ElementsScriptableObject
{
	[SerializeReference]
	public AbstractUnitEvaluator? Evaluator;

	public AbstractUnitEntity? Evaluate()
	{
		return Evaluator?.GetValue();
	}
}
