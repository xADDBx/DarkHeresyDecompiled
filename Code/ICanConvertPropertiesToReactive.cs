using System;
using R3;

[Obsolete]
public interface ICanConvertPropertiesToReactive
{
	ReactiveCommand<Unit> UpdateCommand { get; }
}
