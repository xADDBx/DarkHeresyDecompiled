using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class WarhammerUnitPartChooseWeapon : BaseUnitPart, IHashable, IOwlPackable<WarhammerUnitPartChooseWeapon>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityRef<ItemEntityWeapon> m_WeaponRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "WarhammerUnitPartChooseWeapon",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_WeaponRef", typeof(EntityRef<ItemEntityWeapon>))
		}
	};

	public ItemEntityWeapon ChosenWeapon => m_WeaponRef.Entity;

	public void ChooseWeapon(ItemEntityWeapon weapon)
	{
		m_WeaponRef = weapon;
	}

	public void RemoveWeapon()
	{
		m_WeaponRef = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<ItemEntityWeapon> obj = m_WeaponRef;
		Hash128 val2 = StructHasher<EntityRef<ItemEntityWeapon>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		WarhammerUnitPartChooseWeapon source = new WarhammerUnitPartChooseWeapon();
		result = Unsafe.As<WarhammerUnitPartChooseWeapon, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<WarhammerUnitPartChooseWeapon>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_WeaponRef", ref m_WeaponRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<WarhammerUnitPartChooseWeapon>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				m_WeaponRef = formatter.ReadPackable<EntityRef<ItemEntityWeapon>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
