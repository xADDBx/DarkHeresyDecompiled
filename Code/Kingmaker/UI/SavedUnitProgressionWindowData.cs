using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Progression.Paths;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[JsonObject]
[OwlPackable(OwlPackableMode.Generate)]
public struct SavedUnitProgressionWindowData : IHashable, IOwlPackable, IOwlPackable<SavedUnitProgressionWindowData>
{
	[JsonProperty]
	[OwlPackInclude]
	public BlueprintCareerPath.Reference CareerPath;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SavedUnitProgressionWindowData",
		Fields = new FieldInfo[1]
		{
			new FieldInfo("CareerPath", typeof(BlueprintCareerPath.Reference))
		}
	};

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = BlueprintReferenceHasher.GetHash128(CareerPath);
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SavedUnitProgressionWindowData source = default(SavedUnitProgressionWindowData);
		result = Unsafe.As<SavedUnitProgressionWindowData, TPossiblyBase>(ref source);
	}

	public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<SavedUnitProgressionWindowData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "CareerPath", ref CareerPath, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SavedUnitProgressionWindowData>();
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
				CareerPath = formatter.ReadPackable<BlueprintCareerPath.Reference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
