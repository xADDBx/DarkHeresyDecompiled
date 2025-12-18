using System;

namespace Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;

[Flags]
public enum EquipmentFeatureFlag
{
	None = 1,
	IsHiddenWithHelmet = 2,
	IgnoreLayer = 4,
	IsHiddenInPeacefulMode = 8,
	IsVisibleOnlyInDollRoom = 0x10,
	IsBackpack = 0x20,
	IsCloak = 0x40,
	IsCloakSquashed = 0x80
}
