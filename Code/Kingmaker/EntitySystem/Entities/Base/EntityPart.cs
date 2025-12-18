using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.Core.Logging;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class EntityPart : IEntitySubscriber, IHashable, IOwlPackable, IOwlPackable<EntityPart>
{
	public virtual Type RequiredEntityType => EntityInterfacesHelper.EntityInterface;

	public IEntity Owner { get; private set; }

	public Entity ConcreteOwner => (Entity)Owner;

	public bool IsSubscribedOnEventBus { get; private set; }

	public void Attach(Entity owner)
	{
		if (owner == null)
		{
			LogChannel.System.Error("Owner is null");
			return;
		}
		if (!RequiredEntityType.IsInterface)
		{
			throw new Exception($"RequiredEntityType should be an interface, but given: {RequiredEntityType}");
		}
		if (!RequiredEntityType.IsAssignableFrom(owner.GetType()))
		{
			LogChannel.System.Error("EntityPartsManager.Add: invalid Owner type " + owner.GetType().Name + " (expected " + RequiredEntityType.Name + ")");
			return;
		}
		Owner = owner;
		if (Owner != null)
		{
			try
			{
				OnAttach();
				OnAttachOrPrePostLoad();
				OnAttachOrPostLoad();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
			Subscribe();
		}
	}

	public void Detach()
	{
		Unsubscribe();
		try
		{
			OnDetach();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
		Owner = null;
	}

	public void PreSave()
	{
		try
		{
			OnPreSave();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public void PrePostLoad(Entity owner)
	{
		Owner = owner;
		try
		{
			OnPrePostLoad();
			OnAttachOrPrePostLoad();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public void PostLoad()
	{
		try
		{
			OnPostLoad();
			OnAttachOrPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public void DidPostLoad()
	{
		try
		{
			OnDidPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public void ApplyPostLoadFixes()
	{
		if (Owner == null)
		{
			LogChannel.System.Error("Owner is null");
			return;
		}
		try
		{
			OnApplyPostLoadFixes();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
	}

	public void Subscribe()
	{
		if (!IsSubscribedOnEventBus)
		{
			EventBus.Subscribe(this);
			try
			{
				OnSubscribe();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
			IsSubscribedOnEventBus = true;
		}
	}

	public void Unsubscribe()
	{
		if (IsSubscribedOnEventBus)
		{
			EventBus.Unsubscribe(this);
			try
			{
				OnUnsubscribe();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
			IsSubscribedOnEventBus = false;
		}
	}

	public void ViewDidAttach()
	{
		try
		{
			OnViewDidAttach();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public void ViewWillDetach()
	{
		try
		{
			OnViewWillDetach();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public void HoldingStateChanged()
	{
		try
		{
			OnHoldingStateChanged();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public void AreaLoadingComplete()
	{
		try
		{
			OnAreaLoadingComplete();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	protected virtual void OnAttach()
	{
	}

	protected virtual void OnAttachOrPrePostLoad()
	{
	}

	protected virtual void OnAttachOrPostLoad()
	{
	}

	protected virtual void OnDetach()
	{
	}

	protected virtual void OnPreSave()
	{
	}

	protected virtual void OnPrePostLoad()
	{
	}

	protected virtual void OnPostLoad()
	{
	}

	protected virtual void OnDidPostLoad()
	{
	}

	protected virtual void OnApplyPostLoadFixes()
	{
	}

	protected virtual void OnSubscribe()
	{
	}

	protected virtual void OnUnsubscribe()
	{
	}

	protected virtual void OnViewDidAttach()
	{
	}

	protected virtual void OnViewWillDetach()
	{
	}

	protected virtual void OnHoldingStateChanged()
	{
	}

	protected virtual void OnAreaLoadingComplete()
	{
	}

	public static implicit operator bool(EntityPart part)
	{
		return part != null;
	}

	public IEntity GetSubscribingEntity()
	{
		return Owner;
	}

	protected void RemoveSelf()
	{
		ConcreteOwner?.Parts?.Remove(this);
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class EntityPart<T> : EntityPart, IHashable, IOwlPackable<EntityPart<T>> where T : IEntity
{
	public override Type RequiredEntityType => EntityInterfacesHelper.GetEntityInterfaceType<T>();

	public new T Owner => (T)base.Owner;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
