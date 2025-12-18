using UnityEngine;

namespace Owlcat.UI;

public interface IVirtualListElementView
{
	RectTransform RectTransform { get; }

	VirtualListLayoutElementSettings LayoutSettings { get; }

	bool NeedRebuildToGetSize { get; }

	void BindVirtualList(IVirtualListElementData data);

	void UnbindVirtualList();

	IVirtualListElementView Instantiate();
}
