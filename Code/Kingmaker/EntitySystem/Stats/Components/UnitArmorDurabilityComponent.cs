using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Stats.Components;

[Serializable]
[AllowedOn(typeof(BlueprintUnit))]
[ComponentName("Stats/Unit/UnitArmorDurabilityComponent")]
[TypeId("0ba7b674ab334c8d81776f25f2c4c3bb")]
public sealed class UnitArmorDurabilityComponent : BlueprintComponent
{
	public int Value = 100;
}
