namespace Owlcat.UI;

public class EmptyConsoleNavigationEntity : IConsoleNavigationEntity, IConsoleEntity
{
	public void SetFocus(bool value)
	{
	}

	public bool IsValid()
	{
		return false;
	}
}
