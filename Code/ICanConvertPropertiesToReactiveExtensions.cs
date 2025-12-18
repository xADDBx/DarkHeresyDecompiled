using System;
using R3;

[Obsolete]
public static class ICanConvertPropertiesToReactiveExtensions
{
	public static Observable<TResult> GetReactiveProperty<T, TResult>(this T source, Func<T, TResult> selector) where T : ICanConvertPropertiesToReactive
	{
		return source.UpdateCommand.Select((Unit x) => selector(source)).Prepend(selector(source)).ToReadOnlyReactiveProperty();
	}
}
