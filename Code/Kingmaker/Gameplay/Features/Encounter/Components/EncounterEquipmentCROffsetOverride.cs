using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Gameplay.Features.Experience;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Encounter.Components;

[Serializable]
[AllowedOn(typeof(BlueprintEncounter))]
[ComponentName("Combat/EncounterEquipmentCROffsetOverride")]
[TypeId("6cabbbfa9e4a6a048921092fe42767d9")]
public sealed class EncounterEquipmentCROffsetOverride : BlueprintComponent
{
	[Serializable]
	public sealed class Entry
	{
		public UnitDifficultyType DifficultyType;

		public int Offset;
	}

	public Entry[] Entries = Array.Empty<Entry>();

	public int? GetOffset(UnitDifficultyType type)
	{
		Entry[] entries = Entries;
		foreach (Entry entry in entries)
		{
			if (entry.DifficultyType == type)
			{
				return entry.Offset;
			}
		}
		return null;
	}
}
