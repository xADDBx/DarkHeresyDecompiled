namespace Owlcat.UI;

public interface IFunc01ClickHandler : IConsoleEntity
{
	bool CanFunc01Click();

	void OnFunc01Click();

	string GetFunc01ClickHint();
}
