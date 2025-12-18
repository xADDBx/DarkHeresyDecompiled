using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OwlPack.Runtime;

namespace Owlcat.EntityBlackboard;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public class RuntimeVariable : IOwlPackable, IOwlPackable<RuntimeVariable>
{
	[OwlPackInclude]
	public string Key;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "RuntimeVariable",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Key", typeof(string))
		}
	};

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		RuntimeVariable source = new RuntimeVariable();
		result = Unsafe.As<RuntimeVariable, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<RuntimeVariable>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "Key", ref Key, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<RuntimeVariable>();
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
				Key = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
public class RuntimeVariable<T> : RuntimeVariable
{
	public virtual T Value { get; set; }

	public override string ToString()
	{
		return $"{Key}: {Value}";
	}
}
