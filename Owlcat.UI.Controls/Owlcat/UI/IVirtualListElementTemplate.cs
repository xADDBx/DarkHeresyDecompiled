using System;

namespace Owlcat.UI;

public interface IVirtualListElementTemplate
{
	Type ElementType { get; }

	int Id { get; }

	IVirtualListElementView View { get; }
}
