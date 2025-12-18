using System;

namespace Owlcat.UI;

public interface IViewComposer
{
	void Add(object data, Action show, Action hide);

	void Remove(object data);
}
