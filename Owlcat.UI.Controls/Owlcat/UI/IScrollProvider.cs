namespace Owlcat.UI;

internal interface IScrollProvider
{
	bool ScrollUpdated();

	float GetScrollValue();

	void SetScrollValue(float value);
}
