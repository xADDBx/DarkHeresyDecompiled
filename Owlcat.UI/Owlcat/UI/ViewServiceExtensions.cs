using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using R3;

namespace Owlcat.UI;

public static class ViewServiceExtensions
{
	public static IDisposable Subscribe<T>(this ViewService service, ReactiveProperty<T> source)
	{
		return source.Scan((default(T), default(T)), ((T, T) accumulate, T current) => (accumulate.Item2, current)).Subscribe(delegate((T, T) value)
		{
			var (val, _) = value;
			if (val != null)
			{
				service.Remove(val, dispose: true);
			}
			T item = value.Item2;
			if (item != null)
			{
				service.Add(item);
			}
		});
	}

	public static IDisposable SubscribeAll<T>(this ViewService service, T source)
	{
		DisposableBag disposableBag = default(DisposableBag);
		MethodInfo method = typeof(ViewServiceExtensions).GetMethod("Subscribe");
		object[] array = new object[2];
		foreach (FieldInfo field in GetFields(typeof(T)))
		{
			object value = field.GetValue(source);
			Type type = field.FieldType.GetGenericArguments()[0];
			array[0] = service;
			array[1] = value;
			IDisposable disposable = (IDisposable)method.MakeGenericMethod(type).Invoke(null, array);
			disposableBag.Add(disposable);
		}
		return disposableBag;
	}

	public static IEnumerable<FieldInfo> GetFields(Type type)
	{
		Type reactiveType = typeof(ReactiveProperty<>);
		return from x in type.GetFields(BindingFlags.Instance | BindingFlags.Public)
			where x.FieldType.IsGenericType
			where x.FieldType.GetGenericTypeDefinition() == reactiveType
			select x;
	}
}
