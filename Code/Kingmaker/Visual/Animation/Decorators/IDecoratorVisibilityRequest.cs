namespace Kingmaker.Visual.Animation.Decorators;

public interface IDecoratorVisibilityRequest
{
	public enum RequestType
	{
		Show,
		Hide
	}

	object Requester { get; }

	RequestType Type { get; }

	bool IsReleased { get; }

	void Release();
}
