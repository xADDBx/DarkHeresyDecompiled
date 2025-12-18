using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Exploration;

[Obsolete]
[AllowedOn(typeof(BlueprintAnomaly))]
[TypeId("25be4538ef4f4576be7f775ab5f04eca")]
public abstract class AnomalyInteraction : BlueprintComponent
{
}
