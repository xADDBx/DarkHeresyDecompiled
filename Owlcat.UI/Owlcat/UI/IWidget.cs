using System;

namespace Owlcat.UI;

[Obsolete]
public interface IWidget
{
	void OnWidgetInstantiated();

	void OnWidgetTaken();

	void OnWidgetReturned();
}
