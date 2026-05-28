using System.Collections.Generic;
using System.Threading;

namespace OwlPack.Runtime;

public class QueueSerializer<TConcreteQueue, TElement> : AObjectSerializer<TConcreteQueue> where TConcreteQueue : Queue<TElement>, new()
{
	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = typeof(TConcreteQueue).FullName,
		Fields = null,
		Flags = TypeFlags.IsExternal
	};

	private static ThreadLocal<IOutputFormatter.ElementDelegate<TElement>> m_ElementSerializer = new ThreadLocal<IOutputFormatter.ElementDelegate<TElement>>();

	private static ThreadLocal<IInputFormatter.ElementDelegate<TElement>> m_ElementDeserializer = new ThreadLocal<IInputFormatter.ElementDelegate<TElement>>();

	public override TypeInfo TypeInfo => OwlPackTypeInfo;

	protected override void SerializeInternal<TFormatter>(TFormatter formatter, ref TConcreteQueue? value, SerializerState state)
	{
		if (value == null)
		{
			formatter.NullObject();
			return;
		}
		OutputFormatter.CreateElementDelegate(formatter, ref m_ElementSerializer);
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(value);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<TConcreteQueue>(OwlPackTypeInfo);
		ref TFormatter reference = ref formatter;
		TFormatter val = default(TFormatter);
		if (val == null)
		{
			val = reference;
			reference = ref val;
		}
		reference.StartArray(type, OwlPackTypeInfo.Name, typeof(TElement).FullName, objectId, value.Count);
		foreach (TElement item in value)
		{
			TElement value2 = item;
			m_ElementSerializer.Value(ref value2, state);
		}
		formatter.EndArray();
	}

	protected override void DeserializeInternal<TFormatter>(TFormatter formatter, ref TConcreteQueue? value, uint objectId, DeserializerState state)
	{
		InputFormatter.CreateElementDelegate(formatter, ref m_ElementDeserializer);
		value = new TConcreteQueue();
		state.References.Register(objectId, value);
		formatter.EnterArray(out var count);
		for (int i = 0; i < count; i++)
		{
			formatter.NextArrayElement();
			value.Enqueue(m_ElementDeserializer.Value(state));
		}
		formatter.LeaveArray();
	}
}
