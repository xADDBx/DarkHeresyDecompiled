using System;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Visual.Sound;

public abstract class BaseAsksController : IUnitAsksController, IDisposable
{
	protected BaseAsksController()
	{
		EventBus.Subscribe(this);
	}

	public virtual void Dispose()
	{
		EventBus.Unsubscribe(this);
	}
}
