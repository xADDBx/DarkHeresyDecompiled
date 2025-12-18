using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("88c33e2e171c44948b372a22e3250c43")]
public class NamedParameterBlueprint : BlueprintEvaluator
{
	public string Parameter;

	protected override BlueprintScriptableObject GetValueInternal()
	{
		NamedParametersContext.ContextData current = ContextData<NamedParametersContext.ContextData>.Current;
		if (current == null)
		{
			return null;
		}
		if (!current.Context.Params.TryGetValue(Parameter, out var value))
		{
			Element.LogError(this, "Cannot find blueprint {0} in context parameters", Parameter);
		}
		if (value.Value is string)
		{
			return ResourcesLibrary.TryGetBlueprint((string)value.Value) as BlueprintScriptableObject;
		}
		return (value.Value as BlueprintScriptableObjectReference)?.GetBlueprint();
	}

	public override string GetCaption()
	{
		return "P:" + Parameter;
	}
}
