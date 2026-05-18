using System;

namespace Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;

[Flags]
public enum EquipmentFeatureFlag
{
	None = 1,
	IgnoreLayer = 2,
	IsHiddenInPeacefulMode = 4,
	IsVisibleOnlyInDollRoom = 8,
	IsHelmet = 0x10,
	IsArmor = 0x20,
	IsBackpack = 0x40,
	IsCloak = 0x80,
	IsGloves = 0x100,
	IsBoots = 0x200,
	IsMechadendrites = 0x400
}
