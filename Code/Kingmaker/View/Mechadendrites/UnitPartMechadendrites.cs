using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.Equipment;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechadendrites;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartMechadendrites : AbstractUnitPart, IHashable, IOwlPackable<UnitPartMechadendrites>
{
	public readonly Dictionary<MechadendritesType, MechadendriteSettings> Mechadendrites = new Dictionary<MechadendritesType, MechadendriteSettings>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartMechadendrites",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	public void RegisterMechadendrite(MechadendriteSettings settings)
	{
		if (Mechadendrites.ContainsKey(settings.MechadendritesType))
		{
			Mechadendrites[settings.MechadendritesType] = settings;
		}
		else
		{
			Mechadendrites.Add(settings.MechadendritesType, settings);
		}
		UnitViewHandsEquipment unitViewHandsEquipment = (base.Owner.View as UnitEntityView)?.HandsEquipment;
		if (unitViewHandsEquipment?.Sets == null)
		{
			return;
		}
		foreach (var (_, weaponSet2) in unitViewHandsEquipment?.Sets)
		{
			weaponSet2.MainHand?.MatchVisuals();
			weaponSet2.OffHand?.MatchVisuals();
			weaponSet2.OffHand?.AttachModel(toHand: true);
		}
	}

	public void UnregisterMechadendrite(MechadendriteSettings settings)
	{
		if (Mechadendrites.ContainsKey(settings.MechadendritesType) && Mechadendrites[settings.MechadendritesType] == settings)
		{
			Mechadendrites.Remove(settings.MechadendritesType);
		}
		UnitViewHandsEquipment unitViewHandsEquipment = (base.Owner.View as UnitEntityView)?.HandsEquipment;
		if (unitViewHandsEquipment?.Sets == null)
		{
			return;
		}
		foreach (var (_, weaponSet2) in unitViewHandsEquipment?.Sets)
		{
			weaponSet2.MainHand?.MatchVisuals();
			weaponSet2.OffHand?.MatchVisuals();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartMechadendrites source = new UnitPartMechadendrites();
		result = Unsafe.As<UnitPartMechadendrites, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<UnitPartMechadendrites>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartMechadendrites>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
