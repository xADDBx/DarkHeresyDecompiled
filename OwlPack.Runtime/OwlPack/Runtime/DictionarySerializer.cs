using System.Collections.Generic;
using System.Threading;

namespace OwlPack.Runtime;

public class DictionarySerializer<TConcreteDictionary, TKey, TValue> : AObjectSerializer<TConcreteDictionary> where TConcreteDictionary : IDictionary<TKey, TValue>, new()
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(TConcreteDictionary).FullName,
		Fields = null,
		Flags = TypeFlags.IsExternal
	};

	private static ThreadLocal<IOutputFormatter.ElementDelegate<TKey>> m_KeySerializer = new ThreadLocal<IOutputFormatter.ElementDelegate<TKey>>();

	private static ThreadLocal<IInputFormatter.ElementDelegate<TKey>> m_KeyDeserializer = new ThreadLocal<IInputFormatter.ElementDelegate<TKey>>();

	private static ThreadLocal<IOutputFormatter.ElementDelegate<TValue>> m_ValueSerializer = new ThreadLocal<IOutputFormatter.ElementDelegate<TValue>>();

	private static ThreadLocal<IInputFormatter.ElementDelegate<TValue>> m_ValueDeserializer = new ThreadLocal<IInputFormatter.ElementDelegate<TValue>>();

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref TConcreteDictionary? value, SerializerState state)
	{
		if (value == null)
		{
			formatter.NullObject();
			return;
		}
		OutputFormatter.CreateElementDelegate(formatter, ref m_KeySerializer);
		OutputFormatter.CreateElementDelegate(formatter, ref m_ValueSerializer);
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort typeID = state.TypeLibrary.GetTypeID<TConcreteDictionary>();
		ref TFormatter reference = ref formatter;
		TFormatter val = default(TFormatter);
		if (val == null)
		{
			val = reference;
			reference = ref val;
		}
		reference.StartArray(typeID, OwlPackTypeInfo.Name, "(" + typeof(TKey).FullName + ", " + typeof(TValue).FullName + ")", objectId, value.Count);
		foreach (KeyValuePair<TKey, TValue> item in value)
		{
			TKey value2 = item.Key;
			TValue value3 = item.Value;
			formatter.StartTuple(2);
			m_KeySerializer.Value(ref value2, state);
			formatter.NextTupleElement();
			m_ValueSerializer.Value(ref value3, state);
			formatter.EndTuple();
		}
		formatter.EndArray();
	}

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref TConcreteDictionary? value, uint objectId, DeserializerState state)
	{
		InputFormatter.CreateElementDelegate(formatter, ref m_KeyDeserializer);
		InputFormatter.CreateElementDelegate(formatter, ref m_ValueDeserializer);
		value = new TConcreteDictionary();
		state.References.Register(objectId, value);
		formatter.EnterArray(out var count);
		for (int i = 0; i < count; i++)
		{
			formatter.NextArrayElement();
			formatter.EnterTuple(out var count2);
			if (count2 != 2)
			{
				throw new InputFormatterException($"Expecting 2 values per tuple when deserializing {typeof(TConcreteDictionary).FullName}, got {count2}");
			}
			KeyValuePair<TKey, TValue> item = new KeyValuePair<TKey, TValue>(m_KeyDeserializer.Value(state), m_ValueDeserializer.Value(state));
			value.Add(item);
			formatter.LeaveTuple();
		}
		formatter.LeaveArray();
	}
}
