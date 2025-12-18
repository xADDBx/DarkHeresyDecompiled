using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.Net;

[JsonObject(IsReference = false)]
[OwlPackable(OwlPackableMode.Generate)]
public class LeftStickData : IHashable, IOwlPackable, IOwlPackable<LeftStickData>
{
	[JsonProperty(PropertyName = "v")]
	[OwlPackInclude]
	public byte version;

	[JsonProperty(PropertyName = "u")]
	[OwlPackInclude]
	public UnitReference unit;

	[JsonProperty(PropertyName = "x")]
	[OwlPackInclude]
	public sbyte moveDirectionX;

	[JsonProperty(PropertyName = "y")]
	[OwlPackInclude]
	public sbyte moveDirectionY;

	[JsonProperty(PropertyName = "s")]
	[OwlPackInclude]
	public UnitReference[] selectedUnits;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "LeftStickData",
		OldNames = null,
		Fields = new FieldInfo[5]
		{
			new FieldInfo("version", typeof(byte)),
			new FieldInfo("unit", typeof(UnitReference)),
			new FieldInfo("moveDirectionX", typeof(sbyte)),
			new FieldInfo("moveDirectionY", typeof(sbyte)),
			new FieldInfo("selectedUnits", typeof(UnitReference[]))
		}
	};

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref version);
		UnitReference obj = unit;
		Hash128 val = UnitReferenceHasher.GetHash128(ref obj);
		result.Append(ref val);
		result.Append(ref moveDirectionX);
		result.Append(ref moveDirectionY);
		UnitReference[] array = selectedUnits;
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				UnitReference obj2 = array[i];
				Hash128 val2 = UnitReferenceHasher.GetHash128(ref obj2);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		LeftStickData source = new LeftStickData();
		result = Unsafe.As<LeftStickData, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<LeftStickData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "version", ref version, state);
		formatter.Field(1, "unit", ref unit, state);
		formatter.UnmanagedField(2, "moveDirectionX", ref moveDirectionX, state);
		formatter.UnmanagedField(3, "moveDirectionY", ref moveDirectionY, state);
		formatter.Field(4, "selectedUnits", ref selectedUnits, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<LeftStickData>();
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
				version = formatter.ReadUnmanaged<byte>(state);
				break;
			case 1:
				unit = formatter.ReadPackable<UnitReference>(state);
				break;
			case 2:
				moveDirectionX = formatter.ReadUnmanaged<sbyte>(state);
				break;
			case 3:
				moveDirectionY = formatter.ReadUnmanaged<sbyte>(state);
				break;
			case 4:
				selectedUnits = formatter.ReadPackable<UnitReference[]>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
