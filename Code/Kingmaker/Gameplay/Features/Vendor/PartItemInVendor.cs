using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Vendor;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class PartItemInVendor : EntityPart<ItemEntity>, IHashable, IOwlPackable<PartItemInVendor>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartItemInVendor",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("VendorTable", typeof(BlueprintSharedVendorTable)),
			new FieldInfo("IdInVendorTable", typeof(int)),
			new FieldInfo("SoldByPlayer", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	[CanBeNull]
	public BlueprintSharedVendorTable VendorTable { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int IdInVendorTable { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool SoldByPlayer { get; set; }

	public void CopyFrom(PartItemInVendor other)
	{
		VendorTable = other.VendorTable;
		IdInVendorTable = other.IdInVendorTable;
		SoldByPlayer = other.SoldByPlayer;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(VendorTable);
		result.Append(ref val2);
		int val3 = IdInVendorTable;
		result.Append(ref val3);
		bool val4 = SoldByPlayer;
		result.Append(ref val4);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartItemInVendor source = new PartItemInVendor();
		result = Unsafe.As<PartItemInVendor, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartItemInVendor>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintSharedVendorTable value = VendorTable;
		formatter.Field(0, "VendorTable", ref value, state);
		int value2 = IdInVendorTable;
		formatter.UnmanagedField(1, "IdInVendorTable", ref value2, state);
		bool value3 = SoldByPlayer;
		formatter.UnmanagedField(2, "SoldByPlayer", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartItemInVendor>();
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
				VendorTable = formatter.ReadPackable<BlueprintSharedVendorTable>(state);
				break;
			case 1:
				IdInVendorTable = formatter.ReadUnmanaged<int>(state);
				break;
			case 2:
				SoldByPlayer = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
