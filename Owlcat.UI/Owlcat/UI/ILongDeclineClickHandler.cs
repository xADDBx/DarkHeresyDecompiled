namespace Owlcat.UI;

public interface ILongDeclineClickHandler : IConsoleEntity
{
	bool CanLongDeclineClick();

	void OnLongDeclineClick();

	string GetLongDeclineClickHint();
}
