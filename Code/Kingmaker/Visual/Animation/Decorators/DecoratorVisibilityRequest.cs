namespace Kingmaker.Visual.Animation.Decorators;

internal class DecoratorVisibilityRequest : IDecoratorVisibilityRequest
{
	protected readonly IRuntimeDecorator m_Decorator;

	public object Requester { get; }

	public IDecoratorVisibilityRequest.RequestType Type { get; }

	public bool IsReleased { get; private set; }

	public DecoratorVisibilityRequest(IRuntimeDecorator decorator, object requester, IDecoratorVisibilityRequest.RequestType type)
	{
		m_Decorator = decorator;
		Requester = requester;
		Type = type;
		m_Decorator.AddRequest(this);
	}

	public void Release()
	{
		if (!IsReleased)
		{
			IsReleased = true;
			m_Decorator.RemoveRequest(this);
		}
	}

	public override string ToString()
	{
		return m_Decorator.DecoratorAsset.name + " | " + string.Format("{0} by {1}", (Type == IDecoratorVisibilityRequest.RequestType.Show) ? "Showing" : "Hiding", Requester);
	}
}
