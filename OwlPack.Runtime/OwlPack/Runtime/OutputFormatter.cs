using System;
using System.Reflection;
using System.Threading;

namespace OwlPack.Runtime;

public static class OutputFormatter
{
	private static IOutputFormatter.FieldDelegate<TField> CreateFieldDelegateInternal<TField>(IOutputFormatter formatter)
	{
		Type typeFromHandle = typeof(TField);
		Type underlyingType = Nullable.GetUnderlyingType(typeFromHandle);
		if (typeFromHandle.IsPrimitive)
		{
			if (underlyingType != null)
			{
				return (IOutputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("UnmanagedNullableField", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
					.CreateDelegate(typeof(IOutputFormatter.FieldDelegate<TField>), formatter);
			}
			return (IOutputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("UnmanagedField", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
				.CreateDelegate(typeof(IOutputFormatter.FieldDelegate<TField>), formatter);
		}
		if (typeFromHandle.IsEnum)
		{
			if (underlyingType != null)
			{
				return (IOutputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("EnumNullableField", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
					.CreateDelegate(typeof(IOutputFormatter.FieldDelegate<TField>), formatter);
			}
			return (IOutputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("EnumField", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
				.CreateDelegate(typeof(IOutputFormatter.FieldDelegate<TField>), formatter);
		}
		if (typeFromHandle == typeof(string))
		{
			return (IOutputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("StringField", BindingFlags.Instance | BindingFlags.Public).CreateDelegate(typeof(IOutputFormatter.FieldDelegate<TField>), formatter);
		}
		if (underlyingType != null)
		{
			return (IOutputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("NullableField", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
				.CreateDelegate(typeof(IOutputFormatter.FieldDelegate<TField>), formatter);
		}
		return (IOutputFormatter.FieldDelegate<TField>)formatter.GetType().GetMethod("Field", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
			.CreateDelegate(typeof(IOutputFormatter.FieldDelegate<TField>), formatter);
	}

	public static void CreateFieldDelegate<TField>(IOutputFormatter formatter, ref IOutputFormatter.FieldDelegate<TField> result)
	{
		if (result == null || result.Target != formatter)
		{
			result = CreateFieldDelegateInternal<TField>(formatter);
		}
	}

	private static IOutputFormatter.ElementDelegate<TElement> CreateElementDelegateInternal<TElement>(IOutputFormatter formatter)
	{
		Type typeFromHandle = typeof(TElement);
		Type underlyingType = Nullable.GetUnderlyingType(typeFromHandle);
		if (typeFromHandle.IsPrimitive)
		{
			if (underlyingType != null)
			{
				return (IOutputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("UnmanagedNullableArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
					.CreateDelegate(typeof(IOutputFormatter.ElementDelegate<TElement>), formatter);
			}
			return (IOutputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("UnmanagedArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
				.CreateDelegate(typeof(IOutputFormatter.ElementDelegate<TElement>), formatter);
		}
		if (typeFromHandle.IsEnum)
		{
			if (underlyingType != null)
			{
				return (IOutputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("EnumNullableArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
					.CreateDelegate(typeof(IOutputFormatter.ElementDelegate<TElement>), formatter);
			}
			return (IOutputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("EnumArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
				.CreateDelegate(typeof(IOutputFormatter.ElementDelegate<TElement>), formatter);
		}
		if (typeFromHandle == typeof(string))
		{
			return (IOutputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("StringArrayElement", BindingFlags.Instance | BindingFlags.Public).CreateDelegate(typeof(IOutputFormatter.ElementDelegate<TElement>), formatter);
		}
		if (underlyingType != null)
		{
			return (IOutputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("NullableArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(underlyingType)
				.CreateDelegate(typeof(IOutputFormatter.ElementDelegate<TElement>), formatter);
		}
		return (IOutputFormatter.ElementDelegate<TElement>)formatter.GetType().GetMethod("ArrayElement", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeFromHandle)
			.CreateDelegate(typeof(IOutputFormatter.ElementDelegate<TElement>), formatter);
	}

	public static void CreateElementDelegate<TElement>(IOutputFormatter formatter, ref IOutputFormatter.ElementDelegate<TElement> result)
	{
		if (result == null || result.Target != formatter)
		{
			result = CreateElementDelegateInternal<TElement>(formatter);
		}
	}

	public static void CreateElementDelegate<TElement>(IOutputFormatter formatter, ref ThreadLocal<IOutputFormatter.ElementDelegate<TElement>> result)
	{
		if (!result.IsValueCreated || result.Value.Target != formatter)
		{
			result.Value = CreateElementDelegateInternal<TElement>(formatter);
		}
	}
}
