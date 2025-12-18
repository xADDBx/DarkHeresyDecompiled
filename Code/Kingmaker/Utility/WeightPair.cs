using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;

namespace Kingmaker.Utility;

[Serializable]
[OwlPackable(OwlPackableMode.Generate)]
public class WeightPair<T> : IOwlPackable, IOwlPackable<WeightPair<T>> where T : BlueprintReferenceBase
{
	[JsonProperty]
	[HasherCustom(Type = typeof(BlueprintReferenceHasher))]
	[OwlPackInclude]
	public T Object;

	[JsonProperty]
	[Weights]
	[OwlPackInclude]
	public int Weight;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "WeightPair",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("Object", typeof(T)),
			new FieldInfo("Weight", typeof(int))
		}
	};

	private static IOutputFormatter.FieldDelegate<T> m_Serializer_T_ = null;

	private static IInputFormatter.FieldDelegate<T> m_Deserializer_T_ = null;

	[JsonConstructor]
	public WeightPair()
	{
	}

	public WeightPair(T obj, int weight)
	{
		Object = obj;
		Weight = weight;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		WeightPair<T> source = new WeightPair<T>();
		result = Unsafe.As<WeightPair<T>, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<WeightPair<T>>(OwlPackTypeInfo);
		OutputFormatter.CreateFieldDelegate(formatter, ref m_Serializer_T_);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		m_Serializer_T_(0, "Object", ref Object, state);
		formatter.UnmanagedField(1, "Weight", ref Weight, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		InputFormatter.CreateFieldDelegate(formatter, ref m_Deserializer_T_);
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<WeightPair<T>>();
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
				Object = m_Deserializer_T_(state);
				break;
			case 1:
				Weight = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
