using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartBirthday : EntityPart, IHashable, IOwlPackable<PartBirthday>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartBirthday",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Day", typeof(int)),
			new FieldInfo("Month", typeof(int))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public int Day { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int Month { get; private set; }

	public void Set(int day, int month)
	{
		Day = day;
		Month = month;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		int val2 = Day;
		result.Append(ref val2);
		int val3 = Month;
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartBirthday source = new PartBirthday();
		result = Unsafe.As<PartBirthday, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartBirthday>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		int value = Day;
		formatter.UnmanagedField(0, "Day", ref value, state);
		int value2 = Month;
		formatter.UnmanagedField(1, "Month", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartBirthday>();
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
				Day = formatter.ReadUnmanaged<int>(state);
				break;
			case 1:
				Month = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
