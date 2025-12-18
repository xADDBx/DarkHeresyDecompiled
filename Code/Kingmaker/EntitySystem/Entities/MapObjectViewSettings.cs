using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.RandomEncounters.Settings;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public class MapObjectViewSettings : IHashable, IOwlPackable, IOwlPackable<MapObjectViewSettings>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MapObjectViewSettings",
		OldNames = null,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Blueprint", typeof(BlueprintSpawnableObject)),
			new FieldInfo("Position", typeof(Vector3)),
			new FieldInfo("Rotation", typeof(Quaternion))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintSpawnableObject Blueprint { get; private set; }

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Vector3 Position { get; private set; }

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Quaternion Rotation { get; private set; }

	[JsonConstructor]
	public MapObjectViewSettings(BlueprintSpawnableObject blueprint, Vector3 position, Quaternion rotation)
	{
		Blueprint = blueprint;
		Position = position;
		Rotation = rotation;
	}

	public MapObjectViewSettings()
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		Vector3 val2 = Position;
		result.Append(ref val2);
		Quaternion val3 = Rotation;
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MapObjectViewSettings source = new MapObjectViewSettings();
		result = Unsafe.As<MapObjectViewSettings, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MapObjectViewSettings>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintSpawnableObject value = Blueprint;
		formatter.Field(0, "Blueprint", ref value, state);
		Vector3 value2 = Position;
		formatter.Field(1, "Position", ref value2, state);
		Quaternion value3 = Rotation;
		formatter.Field(2, "Rotation", ref value3, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MapObjectViewSettings>();
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
				Blueprint = formatter.ReadPackable<BlueprintSpawnableObject>(state);
				break;
			case 1:
				Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 2:
				Rotation = formatter.ReadPackable<Quaternion>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
