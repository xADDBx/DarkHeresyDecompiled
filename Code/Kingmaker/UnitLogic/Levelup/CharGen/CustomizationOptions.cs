using System;
using JetBrains.Annotations;
using Kingmaker.ResourceLinks;

namespace Kingmaker.UnitLogic.Levelup.CharGen;

[Serializable]
public class CustomizationOptions
{
	[NotNull]
	public EquipmentEntityLink[] Head = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Eyebrows = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Hair = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Facial = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Eyes = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Scars = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Tatoo = new EquipmentEntityLink[0];

	[NotNull]
	public EquipmentEntityLink[] Augmentic = new EquipmentEntityLink[0];
}
