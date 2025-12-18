using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[Serializable]
public class ParametrizedContextSetter
{
	public enum ParameterType
	{
		Unit,
		Locator,
		MapObject,
		Position,
		Blueprint,
		Float
	}

	public class ParamEvaluatorAttribute : PropertyAttribute
	{
	}

	[Serializable]
	public class ParameterEntry
	{
		public string Name;

		[InfoBox(Text = "Use float param for set rotation in cutscene")]
		public ParameterType Type;

		[ParamEvaluator]
		[SerializeReference]
		public Element Evaluator;

		public INamedParameterValue GetValue()
		{
			return Type switch
			{
				ParameterType.Unit => new NamedParameterValue_Unit((Evaluator is AbstractUnitEvaluator abstractUnitEvaluator) ? UnitReference.FromIAbstractUnitEntity(abstractUnitEvaluator.GetValue()) : default(UnitReference)), 
				ParameterType.Locator => new NamedParameterValue_Locator((Evaluator is LocatorEvaluator locatorEvaluator) ? locatorEvaluator.GetValue() : null), 
				ParameterType.MapObject => new NamedParameterValue_MapObject((Evaluator is MapObjectEvaluator mapObjectEvaluator) ? mapObjectEvaluator.GetValue() : null), 
				ParameterType.Position => new NamedParameterValue_Position((Evaluator is PositionEvaluator positionEvaluator) ? positionEvaluator.GetValue() : Vector3.zero), 
				ParameterType.Blueprint => new NamedParameterValue_Blueprint((Evaluator is BlueprintEvaluator blueprintEvaluator) ? BlueprintReference<BlueprintScriptableObject>.CreateTyped<BlueprintScriptableObjectReference>(blueprintEvaluator?.GetValue()) : null), 
				ParameterType.Float => new NamedParameterValue_Float((Evaluator is FloatEvaluator floatEvaluator) ? floatEvaluator.GetValue() : 0f), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	public ParameterEntry[] Parameters;

	[NonSerialized]
	public Dictionary<string, INamedParameterValue> AdditionalParams = new Dictionary<string, INamedParameterValue>();
}
