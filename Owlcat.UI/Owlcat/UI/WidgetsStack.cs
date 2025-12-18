using System.Collections.Generic;

namespace Owlcat.UI;

public class WidgetsStack<T> : LinkedList<T>
{
	public T Pop()
	{
		T value = base.Last.Value;
		RemoveLast();
		return value;
	}

	public void Push(T item)
	{
		AddLast(item);
	}
}
