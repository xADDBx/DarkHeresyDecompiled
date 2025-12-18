using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[ComponentName("UI/AddStringToFactDescription")]
[TypeId("2f54dfbdcdce4761bcfd711b2409c34b")]
public class AddStringToFactDescription : AddStringToFact
{
}
