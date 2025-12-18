using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.EntitySystem.Stats.Components;

[Serializable]
[AllowedOn(typeof(BlueprintUnit))]
[ComponentName("Stats/Unit/UnitHitPointsComponent")]
[TypeId("b474249fda574bffb7f9c353e917908b")]
public sealed class UnitHitPointsComponent : BlueprintComponent
{
	public int Value = 100;

	[InfoBox("Если true, то все модификаторы к стату будут проигнорированы и он будет равен указанному значению")]
	public bool Forced;
}
