using System;
using System.Reflection;
using System.Threading;

namespace OwlPack.Runtime;

public static class InputFormatter
{
	private static IInputFormatter.FieldDelegate<TField> CreateFieldDelegateInternal<TField>(IInputFormatter formatter)
	{
		Type typeFromHandle = typeof(TField);
		Type underlyingType = Nullable.GetUnderlyingType(typeFromHandle);
		if (typeFromHandle.IsPrimitive)
		{
			if (underlyingType != null)
			{
				return (IInputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("ReadNullableUnmanaged", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
					.CreateDelegate(typeof(IInputFormatter.FieldDelegate<TField>), formatter);
			}
			return (IInputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("ReadUnmanaged", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
				.CreateDelegate(typeof(IInputFormatter.FieldDelegate<TField>), formatter);
		}
		if (typeFromHandle.IsEnum)
		{
			if (underlyingType != null)
			{
				return (IInputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("ReadNullableEnum", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
					.CreateDelegate(typeof(IInputFormatter.FieldDelegate<TField>), formatter);
			}
			return (IInputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("ReadEnum", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
				.CreateDelegate(typeof(IInputFormatter.FieldDelegate<TField>), formatter);
		}
		if (typeFromHandle == typeof(string))
		{
			return (IInputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("ReadString", BindingFlags.Instance | BindingFlags.Public).CreateDelegate(typeof(IInputFormatter.FieldDelegate<TField>), formatter);
		}
		if (underlyingType != null)
		{
			return (IInputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("ReadNullablePackable", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
				.CreateDelegate(typeof(IInputFormatter.FieldDelegate<TField>), formatter);
		}
		return (IInputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("ReadPackable", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
			.CreateDelegate(typeof(IInputFormatter.FieldDelegate<TField>), formatter);
	}

	public static void CreateFieldDelegate<TField>(IInputFormatter formatter, ref IInputFormatter.FieldDelegate<TField> result)
	{
		if (result == null || result.Target != formatter)
		{
			result = CreateFieldDelegateInternal<TField>(formatter);
		}
	}

	private static IInputFormatter.ElementDelegate<TElement> CreateElementDelegateInternal<TElement>(IInputFormatter formatter)
	{
		Type typeFromHandle = typeof(TElement);
		Type underlyingType = Nullable.GetUnderlyingType(typeFromHandle);
		if (typeFromHandle.IsPrimitive)
		{
			if (underlyingType != null)
			{
				return (IInputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("ReadNullableUnmanagedArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
					.CreateDelegate(typeof(IInputFormatter.ElementDelegate<TElement>), formatter);
			}
			return (IInputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("ReadUnmanagedArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
				.CreateDelegate(typeof(IInputFormatter.ElementDelegate<TElement>), formatter);
		}
		if (typeFromHandle.IsEnum)
		{
			if (underlyingType != null)
			{
				return (IInputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("ReadNullableEnumArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
					.CreateDelegate(typeof(IInputFormatter.ElementDelegate<TElement>), formatter);
			}
			return (IInputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("ReadEnumArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
				.CreateDelegate(typeof(IInputFormatter.ElementDelegate<TElement>), formatter);
		}
		if (typeFromHandle == typeof(string))
		{
			return (IInputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("ReadStringArrayElement", BindingFlags.Instance | BindingFlags.Public).CreateDelegate(typeof(IInputFormatter.ElementDelegate<TElement>), formatter);
		}
		if (underlyingType != null)
		{
			return (IInputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("ReadNullablePackableArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
				.CreateDelegate(typeof(IInputFormatter.ElementDelegate<TElement>), formatter);
		}
		return (IInputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("ReadPackableArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
			.CreateDelegate(typeof(IInputFormatter.ElementDelegate<TElement>), formatter);
	}

	public static void CreateElementDelegate<TElement>(IInputFormatter formatter, ref IInputFormatter.ElementDelegate<TElement> result)
	{
		if (result == null || result.Target != formatter)
		{
			result = CreateElementDelegateInternal<TElement>(formatter);
		}
	}

	public static void CreateElementDelegate<TElement>(IInputFormatter formatter, ref ThreadLocal<IInputFormatter.ElementDelegate<TElement>> result)
	{
		if (!result.IsValueCreated || result.Value.Target != formatter)
		{
			result.Value = CreateElementDelegateInternal<TElement>(formatter);
		}
	}
}
