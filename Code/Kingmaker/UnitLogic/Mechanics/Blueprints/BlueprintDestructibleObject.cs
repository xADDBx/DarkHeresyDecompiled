using System;
using Kingmaker.Blueprints;
using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;

namespace Kingmaker.UnitLogic.Mechanics.Blueprints;

[TypeId("907fc3bb404843afa2393f7be56df153")]
[OwlPackable(OwlPackableMode.NoGenerate)]
public class BlueprintDestructibleObject : BlueprintMechanicEntityFact
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintDestructibleObject>
	{
	}

	public int HitPoints;

	public int Toughness;

	public int DamageReduction;

	public SurfaceType SurfaceType;
}
