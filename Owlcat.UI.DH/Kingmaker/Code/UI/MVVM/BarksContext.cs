using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BarksContext : ViewModel, IBarkHandler, ISubscriber<IEntity>, ISubscriber
{
	private class ReactiveHandler<T> : IBarkHandler, ISubscriber<IEntity>, ISubscriber where T : class, IBarkHandler
	{
		private readonly ReactiveProperty<T> m_Source;

		public ReactiveHandler(ReactiveProperty<T> source)
		{
			m_Source = source;
		}

		public void HandleOnShowBark(string text)
		{
			m_Source.Value?.HandleOnShowBark(text);
		}

		public void HandleOnShowLinkedBark(string text, string encyclopediaLink)
		{
			m_Source.Value?.HandleOnShowLinkedBark(text, encyclopediaLink);
		}

		public void HandleOnHideBark()
		{
			m_Source.Value?.HandleOnHideBark();
		}
	}

	private readonly Dictionary<BarkType, List<IBarkHandler>> m_Handlers;

	private readonly Dictionary<Entity, BarkType> m_ActiveBarks = new Dictionary<Entity, BarkType>();

	public BarksContext(Dictionary<BarkType, List<IBarkHandler>> handlers)
	{
		m_Handlers = handlers;
		EventBus.Subscribe(this).AddTo(this);
		Disposable.Create(delegate
		{
			m_ActiveBarks.Clear();
		}).AddTo(this);
	}

	public void HandleOnShowBark(string text)
	{
		Entity entity = EventInvokerExtensions.Entity;
		BarkType barkType = ResolveBarkType(entity);
		if (entity != null)
		{
			m_ActiveBarks[entity] = barkType;
		}
		ForwardToHandlers(barkType, delegate(IBarkHandler h)
		{
			h.HandleOnShowBark(text);
		});
	}

	public void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
		Entity entity = EventInvokerExtensions.Entity;
		BarkType barkType = ResolveBarkType(entity);
		if (entity != null)
		{
			m_ActiveBarks[entity] = barkType;
		}
		ForwardToHandlers(barkType, delegate(IBarkHandler h)
		{
			h.HandleOnShowLinkedBark(text, encyclopediaLink);
		});
	}

	public void HandleOnHideBark()
	{
		Entity entity = EventInvokerExtensions.Entity;
		BarkType barkType;
		if (entity != null && m_ActiveBarks.TryGetValue(entity, out var value))
		{
			barkType = value;
			m_ActiveBarks.Remove(entity);
		}
		else
		{
			barkType = ResolveBarkType(entity);
		}
		ForwardToHandlers(barkType, delegate(IBarkHandler h)
		{
			h.HandleOnHideBark();
		});
	}

	private static BarkType ResolveBarkType(Entity entity)
	{
		if (entity?.GetOptional<PartDetectiveServoSkull>() != null)
		{
			Game instance = Game.Instance;
			if (instance != null && instance.Controllers?.TurnController?.TurnBasedModeActive == true)
			{
				return BarkType.ServoSkull;
			}
		}
		return BarkType.Default;
	}

	private void ForwardToHandlers(BarkType barkType, Action<IBarkHandler> action)
	{
		if (!m_Handlers.TryGetValue(barkType, out var value))
		{
			return;
		}
		foreach (IBarkHandler item in value)
		{
			action(item);
		}
	}

	public static IBarkHandler WrapReactive<T>(ReactiveProperty<T> source) where T : class, IBarkHandler
	{
		return new ReactiveHandler<T>(source);
	}
}
