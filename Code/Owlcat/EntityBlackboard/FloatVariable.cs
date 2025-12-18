using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OwlPack.Runtime;

namespace Owlcat.EntityBlackboard;

[OwlPackable(OwlPackableMode.Generate)]
public class FloatVariable : RuntimeVariable<float>, IOwlPackable<FloatVariable>
{
	[OwlPackInclude]
	private float m_Value;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "FloatVariable",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Key", typeof(string)),
			new FieldInfo("m_Value", typeof(float))
		}
	};

	public override float Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		FloatVariable source = new FloatVariable();
		result = Unsafe.As<FloatVariable, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<FloatVariable>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.StringField(0, "Key", ref Key, state);
		formatter.UnmanagedField(1, "m_Value", ref m_Value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<FloatVariable>();
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
			case 1:
				m_Value = formatter.ReadUnmanaged<float>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
