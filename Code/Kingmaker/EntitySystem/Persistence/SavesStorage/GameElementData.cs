using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Newtonsoft.Json;
using OwlPack.Runtime;

namespace Kingmaker.EntitySystem.Persistence.SavesStorage;

[OwlPackable(OwlPackableMode.Generate)]
public class GameElementData : IOwlPackable, IOwlPackable<GameElementData>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "GameElementData",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	[JsonProperty]
	public GameElementType Type { get; set; }

	[JsonProperty]
	public string BlueprintGuid { get; set; }

	[JsonProperty]
	public string BlueprintName { get; set; }

	[JsonProperty]
	public string Value { get; set; }

	public GameElementData()
	{
	}

	public GameElementData(GameElementType type, BlueprintScriptableObject blueprint, string value)
	{
		Type = type;
		Value = value;
		if (blueprint != null)
		{
			BlueprintName = blueprint.name;
			BlueprintGuid = blueprint.AssetGuid;
		}
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		GameElementData source = new GameElementData();
		result = Unsafe.As<GameElementData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<GameElementData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<GameElementData>();
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
