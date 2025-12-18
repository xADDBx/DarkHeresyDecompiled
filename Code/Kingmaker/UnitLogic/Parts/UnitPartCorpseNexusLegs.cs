using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartCorpseNexusLegs : BaseUnitPart, IHashable, IOwlPackable<UnitPartCorpseNexusLegs>
{
	[JsonProperty]
	[OwlPackInclude]
	public List<CorpseNexusLegData> Legs = new List<CorpseNexusLegData>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartCorpseNexusLegs",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Legs", typeof(List<CorpseNexusLegData>))
		}
	};

	public void NewLeg(BaseUnitEntity newLeg, CorpseNexusLegType legType)
	{
		if (!Legs.Any((CorpseNexusLegData p) => p.Unit == newLeg))
		{
			CorpseNexusLegData corpseNexusLegData = new CorpseNexusLegData();
			corpseNexusLegData.Unit = newLeg;
			corpseNexusLegData.LegType = legType;
			Legs.Add(corpseNexusLegData);
		}
	}

	public void RemoveLeg(BaseUnitEntity leg)
	{
		CorpseNexusLegData corpseNexusLegData = Legs.Find((CorpseNexusLegData p) => p.Unit == leg);
		if (corpseNexusLegData != null)
		{
			Legs.Remove(corpseNexusLegData);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<CorpseNexusLegData> legs = Legs;
		if (legs != null)
		{
			for (int i = 0; i < legs.Count; i++)
			{
				Hash128 val2 = ClassHasher<CorpseNexusLegData>.GetHash128(legs[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartCorpseNexusLegs source = new UnitPartCorpseNexusLegs();
		result = Unsafe.As<UnitPartCorpseNexusLegs, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartCorpseNexusLegs>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "Legs", ref Legs, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartCorpseNexusLegs>();
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
				Legs = formatter.ReadPackable<List<CorpseNexusLegData>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
