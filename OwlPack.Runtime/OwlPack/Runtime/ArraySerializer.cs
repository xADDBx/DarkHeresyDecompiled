using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace OwlPack.Runtime;

public class ArraySerializer : IObjectSerializer, ISerializer
{
	protected interface IImpl
	{
		void Serialize<TFormatter, TArray>(TFormatter formatter, ref TArray value, SerializerState state) where TFormatter : IOutputFormatter;

		void Deserialize<TFormatter, TArray>(TFormatter formatter, ref TArray value, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct Impl<TElement> : IImpl
	{
		private static ThreadLocal<IOutputFormatter.ElementDelegate<TElement>> s_SerializationDelegate = new ThreadLocal<IOutputFormatter.ElementDelegate<TElement>>();

		private static ThreadLocal<IInputFormatter.ElementDelegate<TElement>> s_DeserializationDelegate = new ThreadLocal<IInputFormatter.ElementDelegate<TElement>>();

		public void Serialize<TFormatter, TArray>(TFormatter formatter, ref TArray valueBase, SerializerState state) where TFormatter : IOutputFormatter
		{
			TElement[] array = Unsafe.As<TArray, TElement[]>(ref valueBase);
			if (array == null)
			{
				formatter.NullObject();
				return;
			}
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(array);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort typeID = state.TypeLibrary.GetTypeID<Array>();
			ref TFormatter reference = ref formatter;
			TFormatter val = default(TFormatter);
			if (val == null)
			{
				val = reference;
				reference = ref val;
			}
			reference.StartArray(typeID, null, "Array", objectId, array.Length);
			OutputFormatter.CreateElementDelegate(formatter, ref s_SerializationDelegate);
			TElement[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				TElement value = array2[i];
				s_SerializationDelegate.Value(ref value, state);
			}
			formatter.EndArray();
		}

		public void Deserialize<TFormatter, TArray>(TFormatter formatter, ref TArray value, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			formatter.EnterArray(out var count);
			TElement[] source = new TElement[count];
			state.References.Register(objectId, source);
			InputFormatter.CreateElementDelegate(formatter, ref s_DeserializationDelegate);
			for (int i = 0; i < count; i++)
			{
				formatter.NextArrayElement();
				source[i] = s_DeserializationDelegate.Value(state);
			}
			formatter.LeaveArray();
			value = Unsafe.As<TElement[], TArray>(ref source);
		}
	}

	public static TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Array",
		Fields = null,
		Flags = TypeFlags.IsExternal
	};

	protected Dictionary<Type, IImpl> m_Implementations = new Dictionary<Type, IImpl>();

	public TypeInfo TypeInfo => OwlPackTypeInfo;

	private IImpl GetOrCreate(Type t)
	{
		if (m_Implementations.TryGetValue(t, out var value))
		{
			return value;
		}
		value = (IImpl)Activator.CreateInstance(typeof(Impl<>).MakeGenericType(t.GetElementType()));
		m_Implementations[t] = value;
		return value;
	}

	public void Deserialize<TFormatter, TPossiblyBaseCollection>(TFormatter formatter, ref TPossiblyBaseCollection value, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		GetOrCreate(typeof(TPossiblyBaseCollection)).Deserialize(formatter, ref value, objectId, state);
	}

	public void Serialize<TFormatter, TPossiblyBaseCollection>(TFormatter formatter, ref TPossiblyBaseCollection valueBase, SerializerState state) where TFormatter : IOutputFormatter
	{
		GetOrCreate(valueBase.GetType()).Serialize(formatter, ref valueBase, state);
	}
}
