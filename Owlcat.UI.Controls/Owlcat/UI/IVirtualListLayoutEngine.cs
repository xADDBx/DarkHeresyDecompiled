namespace Owlcat.UI;

internal interface IVirtualListLayoutEngine
{
	void SetClear();

	void SetOffsetElement(VirtualListElement element, bool forItself = false);

	void SetOffset(float position);

	void UpdatePosition(VirtualListElement element);

	bool IsInFieldOfView(VirtualListElement element);
}
