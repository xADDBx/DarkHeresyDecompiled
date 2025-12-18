using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Visual;

[OwlPackable(OwlPackableMode.Generate)]
public class BoneTransformData : IHashable, IOwlPackable, IOwlPackable<BoneTransformData>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "BoneTransformData",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Position", typeof(Vector3)),
			new FieldInfo("Rotation", typeof(Quaternion))
		}
	};

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Vector3 Position { get; private set; }

	[JsonProperty(IsReference = false)]
	[OwlPackInclude]
	public Quaternion Rotation { get; private set; }

	[JsonConstructor]
	private BoneTransformData()
	{
	}

	public BoneTransformData(Vector3 position, Quaternion rotation)
	{
		Position = position;
		Rotation = rotation;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Vector3 val = Position;
		result.Append(ref val);
		Quaternion val2 = Rotation;
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		BoneTransformData source = new BoneTransformData();
		result = Unsafe.As<BoneTransformData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<BoneTransformData>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		Vector3 value = Position;
		formatter.Field(0, "Position", ref value, state);
		Quaternion value2 = Rotation;
		formatter.Field(1, "Rotation", ref value2, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<BoneTransformData>();
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
				Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 1:
				Rotation = formatter.ReadPackable<Quaternion>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
