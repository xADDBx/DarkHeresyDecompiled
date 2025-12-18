using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[OwlPackable(OwlPackableMode.Generate)]
public abstract class EntityFactComponent : IDisposable, IEventBusLoopGuard, IHashable, IOwlPackable, IOwlPackable<EntityFactComponent>
{
	public class Scope : SimpleContextData<EntityFactComponent, Scope>
	{
	}

	private static int s_NextInstanceID = 1;

	private readonly int m_InstanceID;

	public virtual Type RequiredEntityType => typeof(IEntity);

	public EntityFact Fact { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public string SourceBlueprintComponentName { get; protected set; }

	public BlueprintComponent SourceBlueprintComponent { get; private set; }

	public bool IsSubscribedOnEventBus { get; private set; }

	public IEntity Owner => Fact.Owner;

	public bool IsActive
	{
		get
		{
			if (Fact != null)
			{
				return Fact.IsActive;
			}
			return false;
		}
	}

	public bool IsInitialized { get; private set; }

	public virtual IEntity GetSubscribingEntity()
	{
		IEntity entity = (Owner as ItemEntity)?.Wielder;
		return entity ?? Owner;
	}

	[JsonConstructor]
	public EntityFactComponent()
	{
		m_InstanceID = s_NextInstanceID++;
	}

	public virtual void Setup(BlueprintComponent component)
	{
		IsInitialized = true;
		SourceBlueprintComponent = component;
		SourceBlueprintComponentName = component.name;
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(SourceBlueprintComponentName))
		{
			return GetType().Name + "[" + SourceBlueprintComponentName + "]";
		}
		return GetType().Name ?? "";
	}

	[NotNull]
	public virtual TData GetData<TData>() where TData : class
	{
		throw new NotImplementedException();
	}

	[NotNull]
	public TData RequestSavableData<TData>() where TData : IEntityFactComponentSavableData, new()
	{
		return Fact.RequestSavableData<TData>(SourceBlueprintComponent);
	}

	[NotNull]
	public TData RequestTransientData<TData>() where TData : IEntityFactComponentTransientData, new()
	{
		return Fact.RequestTransientData<TData>(SourceBlueprintComponent);
	}

	public void Initialize(EntityFact fact)
	{
		Fact = fact;
		using (SetScope())
		{
			try
			{
				OnInitialize();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
	}

	public void FactAttached()
	{
		using (SetScope())
		{
			try
			{
				OnFactAttached();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
	}

	public void FactDetached()
	{
		using (SetScope())
		{
			try
			{
				OnFactDetached();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
	}

	public void Activate()
	{
		using (SetScope())
		{
			try
			{
				OnActivate();
				OnActivateOrPostLoad();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
			if (Fact == null)
			{
				PFLog.EntityFact.Error("Trying to activate null Fact");
			}
			else if (Fact.IsSubscribedOnEventBus)
			{
				Subscribe();
			}
		}
	}

	public void Deactivate()
	{
		using (SetScope())
		{
			Unsubscribe();
			try
			{
				OnDeactivate();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
	}

	public void Subscribe()
	{
		if (!IsSubscribedOnEventBus)
		{
			EventBus.Subscribe(this);
			IsSubscribedOnEventBus = true;
		}
	}

	public void Unsubscribe()
	{
		if (IsSubscribedOnEventBus)
		{
			EventBus.Unsubscribe(this);
			IsSubscribedOnEventBus = false;
		}
	}

	public void PostLoad(EntityFact owner)
	{
		Fact = owner;
		using (SetScope())
		{
			try
			{
				OnPostLoad();
				if (Fact.IsActive)
				{
					OnActivateOrPostLoad();
				}
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
	}

	public void ApplyPostLoadFixes()
	{
		using (SetScope())
		{
			try
			{
				OnApplyPostLoadFixes();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
	}

	public void ViewDidAttach()
	{
		using (SetScope())
		{
			try
			{
				OnViewDidAttach();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
	}

	public void ViewWillDetach()
	{
		using (SetScope())
		{
			try
			{
				OnViewWillDetach();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
	}

	public void AreaLoadingComplete()
	{
		using (SetScope())
		{
			try
			{
				OnAreaLoadingComplete();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
		}
	}

	public void Dispose()
	{
		if (!IsInitialized)
		{
			return;
		}
		using (SetScope())
		{
			Unsubscribe();
			try
			{
				OnDispose();
			}
			catch (Exception ex)
			{
				PFLog.EntityFact.Exception(ex);
			}
			SourceBlueprintComponent = null;
			IsInitialized = false;
		}
	}

	protected virtual void OnInitialize()
	{
	}

	protected virtual void OnFactAttached()
	{
	}

	protected virtual void OnFactDetached()
	{
	}

	protected virtual void OnActivate()
	{
	}

	protected virtual void OnActivateOrPostLoad()
	{
	}

	protected virtual void OnDeactivate()
	{
	}

	protected virtual void OnPostLoad()
	{
	}

	protected virtual void OnApplyPostLoadFixes()
	{
	}

	protected virtual void OnViewDidAttach()
	{
	}

	protected virtual void OnViewWillDetach()
	{
	}

	protected virtual void OnAreaLoadingComplete()
	{
	}

	protected virtual void OnDispose()
	{
	}

	public IDisposable RequestEventContext()
	{
		return SetScope();
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityFactComponent entityFactComponent)
		{
			return m_InstanceID == entityFactComponent.m_InstanceID;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(typeof(EntityFactComponent), m_InstanceID);
	}

	public IDisposable SetScope()
	{
		return DisposableBag.Claim(SimpleContextData<EntityFactComponent, Scope>.Set(this), Fact?.MaybeContext?.SetScope());
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(SourceBlueprintComponentName);
		return result;
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class EntityFactComponent<TEntity> : EntityFactComponent, IHashable, IOwlPackable<EntityFactComponent<TEntity>> where TEntity : IEntity
{
	public sealed override Type RequiredEntityType => typeof(TEntity);

	public new TEntity Owner => (TEntity)base.Owner;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
[OwlPackable(OwlPackableMode.Generate)]
public abstract class EntityFactComponent<TEntity, TBlueprintComponent> : EntityFactComponent<TEntity>, IHashable, IOwlPackable<EntityFactComponent<TEntity, TBlueprintComponent>> where TEntity : Entity where TBlueprintComponent : BlueprintComponent
{
	public new TBlueprintComponent SourceBlueprintComponent => (TBlueprintComponent)base.SourceBlueprintComponent;

	public TBlueprintComponent Settings => SourceBlueprintComponent;

	public override void Setup(BlueprintComponent component)
	{
		if (!(component is TBlueprintComponent))
		{
			LogChannel.System.Error($"EntityFactComponent<{typeof(TEntity).Name}, {typeof(TBlueprintComponent).Name}>.Setup: invalid component type {component.GetType().Name}");
		}
		else
		{
			base.Setup(component);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
