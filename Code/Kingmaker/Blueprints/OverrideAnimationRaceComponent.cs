using System;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[Obsolete]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("d5b50a89d394e7442888820691e1025a")]
public class OverrideAnimationRaceComponent : BlueprintComponent
{
	public BlueprintRaceReference BlueprintRace;

	public override string ToString()
	{
		return "Переопределить рассу для Variant анимаций";
	}
}
