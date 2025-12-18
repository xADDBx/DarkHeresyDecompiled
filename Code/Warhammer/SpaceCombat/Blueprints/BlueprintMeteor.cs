using System;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.View.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Warhammer.SpaceCombat.Blueprints;

[Obsolete]
[TypeId("086ea1e7680449d5ab2a7006667f71c8")]
public class BlueprintMeteor : BlueprintMechanicEntityFact
{
	public int Damage;

	public MechanicEntityView Prefab;

	public Size MeteorSize = Size.Medium;
}
