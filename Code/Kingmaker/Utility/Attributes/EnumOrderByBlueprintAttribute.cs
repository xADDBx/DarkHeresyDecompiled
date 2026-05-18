using System;
using Kingmaker.Blueprints;

namespace Kingmaker.Utility.Attributes;

public abstract class EnumOrderByBlueprintAttribute : EnumOrderAttribute
{
	public abstract System.Enum[] GetOrder(BlueprintScriptableObject blueprint);
}
