namespace Owlcat.UI;

public interface IViewTransitor
{
	Transition Show(object view);

	Transition Hide(object view);
}
