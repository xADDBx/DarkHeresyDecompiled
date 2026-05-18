namespace Kingmaker.Visual.Animation.Decorators;

internal class DecoratorVisibilityRequestWithDuration : DecoratorVisibilityRequest
{
	private float m_Lifetime;

	public DecoratorVisibilityRequestWithDuration(IRuntimeDecorator decorator, object requester, float duration, IDecoratorVisibilityRequest.RequestType type)
		: base(decorator, requester, type)
	{
		m_Lifetime = duration;
	}

	public void Tick(float dt)
	{
		if (!base.IsReleased)
		{
			m_Lifetime -= dt;
			if (m_Lifetime <= 0f)
			{
				Release();
			}
		}
	}

	public override string ToString()
	{
		return $"{m_Decorator.DecoratorAsset.name} | Expires in: {m_Lifetime} | " + string.Format("{0} by {1}", (base.Type == IDecoratorVisibilityRequest.RequestType.Show) ? "Showing" : "Hiding", base.Requester);
	}
}
