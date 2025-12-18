namespace Owlcat.UI;

public interface IFunc02ClickHandler : IConsoleEntity
{
	bool CanFunc02Click();

	void OnFunc02Click();

	string GetFunc02ClickHint();
}
