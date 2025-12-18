namespace Owlcat.UI;

public interface IDeclineClickHandler : IConsoleEntity
{
	bool CanDeclineClick();

	void OnDeclineClick();

	string GetDeclineClickHint();
}
