using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.UI;

[Serializable]
[Obsolete]
[ComponentName("Items/UIPropertiesComponent")]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[AllowMultipleComponents]
[TypeId("95e248f24866439681c312b1bff0693c")]
public class UIPropertiesComponent : BlueprintComponent
{
	public UIPropertySettings[] Properties = new UIPropertySettings[0];
}
