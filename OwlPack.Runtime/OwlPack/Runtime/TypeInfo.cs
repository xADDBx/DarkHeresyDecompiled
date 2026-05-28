using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OwlPack.Runtime;

public class TypeInfo : IOwlPackable<TypeInfo>, IOwlPackable
{
	public string Name;

	public string[] OldNames;

	public FieldInfo[] Fields = Array.Empty<FieldInfo>();

	public TypeFlags Flags;

	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(TypeInfo).FullName,
		Fields = new FieldInfo[3]
		{
			new FieldInfo("Name", typeof(string)),
			new FieldInfo("Fields", typeof(FieldInfo[])),
			new FieldInfo("Flags", typeof(byte))
		}
	};

	public bool IsExternal => Flags.HasFlag(TypeFlags.IsExternal);

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TypeInfo source = new TypeInfo();
		result = Unsafe.As<TypeInfo, TPossiblyBase>(ref source);
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
		ushort typeID = state.TypeLibrary.GetTypeID<TypeInfo>();
		formatter.StartObject(typeID, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "Name", ref Name, state);
		formatter.Field(1, "Fields", ref Fields, state);
		byte value = (byte)Flags;
		formatter.UnmanagedField(2, "Flags", ref value, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TypeInfo>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		state.References.Register(objectId, this);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case 0:
				Name = formatter.ReadString(state);
				break;
			case 1:
				Fields = Serializer.DeserializeObject<FieldInfo[]>(formatter, state);
				break;
			case 2:
				Flags = (TypeFlags)formatter.ReadUnmanaged<byte>(state);
				break;
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
