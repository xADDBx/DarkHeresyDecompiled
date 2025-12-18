namespace Owlcat.UI;

public interface IScrollController
{
	void Scroll(float speed);

	void ScrollTowards(IVirtualListElementData data, float speed);

	void ForceScrollToElement(IVirtualListElementData data);

	void ForceScrollToTop();

	void ForceScrollToBottom();
}
