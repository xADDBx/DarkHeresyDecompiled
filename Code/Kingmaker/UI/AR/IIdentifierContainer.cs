namespace Kingmaker.UI.AR;

public interface IIdentifierContainer
{
	void Push(int cellIdentifier);

	void PushRange(int cellIdentifierBegin, int cellIdentifierEnd);
}
