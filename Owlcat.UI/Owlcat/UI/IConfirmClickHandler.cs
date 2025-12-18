namespace Owlcat.UI;

public interface IConfirmClickHandler : IConsoleEntity
{
	bool CanConfirmClick();

	void OnConfirmClick();

	string GetConfirmClickHint();
}
