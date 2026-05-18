using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[Obsolete("Use OverrideStat instead")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[TypeId("abc6ff48520749c0b4d161484dd6486f")]
public class ReplaceStatAttribute : ReplaceStat
{
	protected override void OnActivateOrPostLoad()
	{
	}

	protected override void OnDeactivate()
	{
	}
}
