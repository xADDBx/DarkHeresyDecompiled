namespace Owlcat.UI;

public interface ILongConfirmClickHandler : IConsoleEntity
{
	bool CanLongConfirmClick();

	void OnLongConfirmClick();

	string GetLongConfirmClickHint();
}
