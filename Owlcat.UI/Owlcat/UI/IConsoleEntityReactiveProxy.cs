using System;

namespace Owlcat.UI;

public interface IConsoleEntityReactiveProxy : IConsoleEntityProxy, IConsoleEntity
{
	IDisposable Subscribe(Action<IConsoleEntity> onNext);
}
