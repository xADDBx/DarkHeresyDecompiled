namespace Owlcat.UI;

public interface IConsoleNavigationEntity : IConsoleEntity
{
	void SetFocus(bool value);

	bool IsValid();
}
