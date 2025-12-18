using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[ComponentName("UI/AddStringToFactName")]
[TypeId("c8cf2b4638d44ca29565527b0d0b90c2")]
public class AddStringToFactName : AddStringToFact
{
}
