using System;

namespace Warhammer.SpaceCombat.Blueprints.Slots;

[Serializable]
[Obsolete]
public class AugerArraySlotData
{
	public AugerArraySlotType Type;

	public BlueprintItemAugerArray.Reference AugerArray;
}
