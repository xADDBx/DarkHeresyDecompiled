namespace Owlcat.UI;

public class ViewTransitor : IViewTransitor
{
	public Transition Show(object view)
	{
		if (view is ITransitable transitable)
		{
			return transitable.Show();
		}
		return Transition.None;
	}

	public Transition Hide(object view)
	{
		if (view is ITransitable transitable)
		{
			return transitable.Hide();
		}
		return Transition.None;
	}
}
