using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace OwlPack.Runtime;

public class FieldInfo : IOwlPackable<FieldInfo>, IOwlPackable
{
	public string Name;

	public Type BaseType;

	public string[] OldNames;

	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(FieldInfo).FullName,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Name", typeof(string)),
			new FieldInfo("Type", typeof(ushort))
		}
	};

	public FieldInfo()
	{
	}

	public FieldInfo(string name, Type type, string[] oldNames = null)
	{
		Name = name;
		BaseType = type;
		OldNames = oldNames;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FieldInfo source = new FieldInfo();
		result = Unsafe.As<FieldInfo, TPossiblyBase>(ref source);
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
		ushort typeID = state.TypeLibrary.GetTypeID<FieldInfo>();
		formatter.StartObject(typeID, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "Name", ref Name, state);
		string assemblyQualifiedName = BaseType.AssemblyQualifiedName;
		assemblyQualifiedName = TypeLibrary.SerializedTypeInfo.ShortTypeNameRegex.Replace(assemblyQualifiedName, "");
		formatter.StringField(1, "Type", ref assemblyQualifiedName, state);
		formatter.EndObject();
	}

	public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FieldInfo>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		state.References.Register(objectId, this);
		formatter.EnterObject();
		int num = ((state.Version == 1) ? 1 : typeInfo.Fields.Length);
		for (int i = 0; i < num; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case 0:
				Name = formatter.ReadString(state);
				break;
			case 1:
			{
				string typeName = formatter.ReadString(state);
				BaseType = Type.GetType(typeName, throwOnError: false);
				break;
			}
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
