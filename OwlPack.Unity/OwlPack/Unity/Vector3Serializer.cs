using System.Collections.Generic;
using OwlPack.Runtime;
using UnityEngine;

namespace OwlPack.Unity;

public class Vector3Serializer : AObjectSerializer<Vector3>
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Vector3",
		Fields = new FieldInfo[3]
		{
			new FieldInfo("x", typeof(float)),
			new FieldInfo("y", typeof(float)),
			new FieldInfo("z", typeof(float))
		},
		Flags = TypeFlags.IsExternal
	};

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	public static void Register()
	{
		ExternalTypeLibrary.Instance.RegisterType(typeof(Vector3), typeof(Vector3Serializer), OwlPackTypeInfo);
	}

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref Vector3 value, uint objectId, DeserializerState state)
	{
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Vector3>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		float x = 0f;
		float y = 0f;
		float z = 0f;
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
				x = formatter.ReadUnmanaged<float>(state);
				break;
			case 1:
				y = formatter.ReadUnmanaged<float>(state);
				break;
			case 2:
				z = formatter.ReadUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
		value = new Vector3(x, y, z);
	}

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref Vector3 value, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<Vector3>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.UnmanagedField(0, "x", ref value.x, state);
		formatter.UnmanagedField(1, "y", ref value.y, state);
		formatter.UnmanagedField(2, "z", ref value.z, state);
		formatter.EndObject();
	}
}
