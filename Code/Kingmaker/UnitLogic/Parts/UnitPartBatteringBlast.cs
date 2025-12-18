using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartBatteringBlast : BaseUnitPart, IAreaHandler, ISubscriber, IHashable, IOwlPackable<UnitPartBatteringBlast>
{
	[JsonProperty]
	[OwlPackInclude]
	public List<AbilityData> Entries = new List<AbilityData>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartBatteringBlast",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Entries", typeof(List<AbilityData>))
		}
	};

	public void NewEntry(AbilityData entry)
	{
		Entries.Add(entry);
	}

	public void RemoveEntry(AbilityData entry)
	{
		Entries.RemoveAll((AbilityData p) => p == entry);
	}

	public void OnAreaBeginUnloading()
	{
		Entries.Clear();
	}

	public int CountEntries(AbilityData entry)
	{
		return Entries.FindAll((AbilityData p) => p == entry).Count;
	}

	public void OnAreaDidLoad()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<AbilityData> entries = Entries;
		if (entries != null)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				Hash128 val2 = ClassHasher<AbilityData>.GetHash128(entries[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartBatteringBlast source = new UnitPartBatteringBlast();
		result = Unsafe.As<UnitPartBatteringBlast, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartBatteringBlast>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "Entries", ref Entries, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartBatteringBlast>();
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
				Entries = formatter.ReadPackable<List<AbilityData>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
