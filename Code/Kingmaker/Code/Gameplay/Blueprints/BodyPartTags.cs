using System;

namespace Kingmaker.Code.Gameplay.Blueprints;

[Flags]
public enum BodyPartTags
{
	None = 0,
	Default = 1,
	Torso = 2,
	Legs = 4,
	Arms = 8,
	Head = 0x10,
	Neck = 0x20,
	Eyes = 0x40,
	Vital = 0x400,
	NonVital = 0x800,
	PreciseIgnore = 0x1000
}
