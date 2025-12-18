using System;
using Kingmaker.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Encounter;

[Serializable]
[TypeId("92c9c13e446b47f88aeb5ec7ab274f55")]
public sealed class BlueprintEncounterRoot : BlueprintScriptableObject
{
	public BpRef<BlueprintEncounter> DefaultEncounter = new BpRef<BlueprintEncounter>();
}
