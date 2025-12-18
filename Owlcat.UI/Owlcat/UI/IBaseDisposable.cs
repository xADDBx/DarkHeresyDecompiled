using System;

namespace Owlcat.UI;

[Obsolete]
public interface IBaseDisposable : IDisposable
{
	event Action OnDispose;
}
