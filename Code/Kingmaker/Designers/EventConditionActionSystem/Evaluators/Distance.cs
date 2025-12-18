using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("d7b66301d89037e49a7d1e6478b75bcb")]
public class Distance : FloatEvaluator
{
	[SerializeReference]
	public PositionEvaluator FirstPoint;

	[SerializeReference]
	public PositionEvaluator SecondPoint;

	public override string GetDescription()
	{
		return "Возвращает дистанцию между двумя точками:\n" + $"{FirstPoint}\n" + $"{SecondPoint}";
	}

	protected override float GetValueInternal()
	{
		return Vector3.Distance(FirstPoint.GetValue(), SecondPoint.GetValue());
	}

	public override string GetCaption()
	{
		return $"Distance ({FirstPoint};{SecondPoint})";
	}
}
