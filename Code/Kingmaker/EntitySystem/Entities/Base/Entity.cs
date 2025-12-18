using System;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.FogOfWar.Culling;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Framework.EntitySystem;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[JsonObject(MemberSerialization.OptIn)]
[OwlPackable(OwlPackableMode.Generate)]
public abstract class Entity : IEntity, IDisposable, IHashable, IOwlPackable, IOwlPackable<Entity>
{
	public class ForcePostLoad : ContextFlag<ForcePostLoad>
	{
	}

	public enum ViewHandlingOnDisposePolicyType
	{
		Destroy,
		Deactivate,
		FadeOutAndDestroy,
		Keep
	}

	public class FogOfWarTarget : ITarget
	{
		private string m_DebugString;

		private Entity m_Data;

		public int RegistryIndex { get; set; } = -1;


		public string SortOrder => m_Data.UniqueId;

		public TargetProperties Properties
		{
			get
			{
				TargetProperties result = default(TargetProperties);
				result.Center = m_Data.FoWPosition;
				result.Radius = (m_Data.View as MapObjectView)?.FogOfWarFudgeRadius ?? 0f;
				return result;
			}
		}

		public bool ForceReveal => m_Data.AlwaysRevealedInFogOfWar;

		public bool Revealed
		{
			get
			{
				return !m_Data.IsInFogOfWar;
			}
			set
			{
				m_Data.IsInFogOfWar = !value;
			}
		}

		public FogOfWarTarget(Entity data)
		{
			m_Data = data;
			m_DebugString = data.View?.ToString();
		}
	}

	private sealed class DisableSetViewTransform : ContextFlag<DisableSetViewTransform>
	{
	}

	public const float PositionChangedThreshold = 1E-05f;

	private FogOfWarTarget m_FogOfWarTarget;

	[JsonProperty]
	[OwlPackInclude]
	protected bool m_IsInGame = true;

	[JsonProperty]
	[OwlPackInclude]
	protected Vector3 m_Position;

	[JsonProperty]
	[OwlPackInclude]
	protected float m_Orientation;

	[JsonProperty]
	[OwlPackInclude]
	protected Vector3? m_InitialPosition;

	[JsonProperty]
	[OwlPackInclude]
	protected float? m_InitialOrientation;

	[JsonProperty]
	[OwlPackInclude]
	public EntityFactsManager Facts;

	[JsonProperty]
	[OwlPackInclude]
	public EntityPartsManager Parts;

	[JsonProperty]
	[OwlPackInclude]
	protected bool m_IsRevealed;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[OwlPackInclude]
	protected ViewHandlingOnDisposePolicyType? m_ViewHandlingOnDisposePolicyOverride;

	private bool m_IsInFogOfWar;

	private bool m_Suppressed;

	[CanBeNull]
	private SceneEntitiesState m_HoldingState;

	public virtual float2 FoWPosition => new float2(Position.x, Position.z);

	[JsonProperty]
	[OwlPackInclude]
	public string UniqueId { get; protected set; }

	public ViewHandlingOnDisposePolicyType ViewHandlingOnDisposePolicy => m_ViewHandlingOnDisposePolicyOverride ?? DefaultViewHandlingOnDisposePolicy;

	[CanBeNull]
	public IEntityViewBase View { get; private set; }

	private EntityViewBase m_view => (EntityViewBase)View;

	public bool Destroyed { get; private set; }

	public bool IsPrePostLoadExecuted { get; private set; }

	public bool IsPostLoadExecuted { get; private set; }

	public bool IsInitialized { get; private set; }

	public bool IsDisposed { get; private set; }

	public bool IsDisposingNow { get; private set; }

	public bool IsSubscribedOnEventBus { get; private set; }

	public bool WillBeDestroyed { get; set; }

	[CanBeNull]
	public EntityServiceProxy Proxy { get; set; }

	public bool IsInCameraFrustum { get; set; } = true;


	public bool IsInCameraFrustum_Closer { get; set; } = true;


	public virtual bool NeedsView => true;

	public virtual bool ForbidFactsAndPartsModifications => false;

	public virtual bool SetTransformFromConfigOnLoad => true;

	public virtual bool SetViewTransform => true;

	public virtual bool SetViewOrientation => SetViewTransform;

	public bool Suppressed
	{
		get
		{
			return m_Suppressed;
		}
		set
		{
			if (m_Suppressed != value)
			{
				m_Suppressed = value;
				if (View != null)
				{
					View.UpdateViewActive();
					UpdateFogOfWarState();
				}
				EventBus.RaiseEvent((IEntity)this, (Action<IEntitySuppressedHandler>)delegate(IEntitySuppressedHandler h)
				{
					h.HandleEntitySuppressionChanged(this, value);
				}, isCheckRuntime: true);
			}
		}
	}

	[CanBeNull]
	public virtual SceneEntitiesState HoldingState => m_HoldingState;

	public bool IsRegistered => EntityService.Instance.Contains(this);

	public bool IsInState => HoldingState != null;

	public virtual ViewHandlingOnDisposePolicyType DefaultViewHandlingOnDisposePolicy
	{
		get
		{
			if (!(View.Or(null)?.GO.scene.name == "BaseMechanics"))
			{
				return ViewHandlingOnDisposePolicyType.Keep;
			}
			return ViewHandlingOnDisposePolicyType.Destroy;
		}
	}

	public bool ShouldBeEnumeratedByEntityPoolEnumerator
	{
		get
		{
			if (m_IsInGame)
			{
				return !m_Suppressed;
			}
			return false;
		}
	}

	public bool IsInGame
	{
		get
		{
			return m_IsInGame;
		}
		set
		{
			if (m_IsInGame != value)
			{
				m_IsInGame = value;
				if (View != null)
				{
					View.UpdateViewActive();
				}
				UpdateFogOfWarState();
				OnIsInGameChanged();
				EventBus.RaiseEvent((IEntity)this, (Action<IInGameHandler>)delegate(IInGameHandler h)
				{
					h.HandleObjectInGameChanged();
				}, isCheckRuntime: true);
			}
		}
	}

	public Vector3 Position
	{
		get
		{
			return m_Position;
		}
		set
		{
			if ((m_Position - value).sqrMagnitude < 1E-05f)
			{
				return;
			}
			m_Position = value;
			using (Counters.EntityPositionChanged?.Measure())
			{
				OnPositionChanged();
			}
			if (SetViewTransform && !ContextData<DisableSetViewTransform>.Current)
			{
				IEntityViewBase view = View;
				if (view != null)
				{
					Transform viewTransform = view.ViewTransform;
					if ((object)viewTransform != null)
					{
						viewTransform.position = EntityPositionToViewPosition(m_Position);
					}
				}
			}
			UpdateChildrenPositions();
		}
	}

	public float Orientation
	{
		get
		{
			return m_Orientation;
		}
		set
		{
			if (Mathf.Approximately(m_Orientation, value))
			{
				return;
			}
			m_Orientation = value;
			if (SetViewTransform && SetViewOrientation && !ContextData<DisableSetViewTransform>.Current)
			{
				IEntityViewBase view = View;
				if (view != null)
				{
					Transform viewTransform = view.ViewTransform;
					if ((object)viewTransform != null)
					{
						viewTransform.rotation = Quaternion.Euler(viewTransform.rotation.eulerAngles.x, m_Orientation, viewTransform.rotation.eulerAngles.z);
					}
				}
			}
			ApplyOrientationToChildren();
		}
	}

	public Vector3 InitialPosition => m_InitialPosition ?? m_Position;

	public float InitialOrientation => m_InitialOrientation ?? m_Orientation;

	public Quaternion Rotation => Quaternion.Euler(0f, m_Orientation, 0f);

	public Vector3 Forward => Quaternion.AngleAxis(m_Orientation, Vector3.up) * Vector3.forward;

	public virtual bool IsViewActive => m_IsInGame;

	public virtual bool IsVisibleForPlayer
	{
		get
		{
			if (IsInGame && !IsInFogOfWar)
			{
				return View?.IsVisible ?? false;
			}
			return false;
		}
	}

	public bool IsInFogOfWar
	{
		get
		{
			return m_IsInFogOfWar;
		}
		set
		{
			if (m_IsInFogOfWar != value)
			{
				m_IsInFogOfWar = value;
				View.Or(null)?.OnInFogOfWarChanged();
			}
		}
	}

	public bool IsRevealed
	{
		get
		{
			return m_IsRevealed;
		}
		set
		{
			if (m_IsRevealed == value)
			{
				return;
			}
			m_IsRevealed = value;
			if (m_IsRevealed)
			{
				EventBus.RaiseEvent((IEntity)this, (Action<IEntityRevealedHandler>)delegate(IEntityRevealedHandler h)
				{
					h.HandleEntityRevealed();
				}, isCheckRuntime: true);
			}
		}
	}

	public virtual bool IsSuppressible => false;

	public virtual bool IsAffectedByFogOfWar => false;

	public virtual bool AlwaysRevealedInFogOfWar => false;

	public virtual bool AddToGrid => false;

	protected ReadonlyList<EntityRef> Children => View?.ChildrenEntityRefs ?? ReadonlyList<EntityRef>.Empty;

	public EntityRef Ref => new EntityRef(this);

	protected void UpdateFogOfWarState()
	{
		if (m_FogOfWarTarget != null)
		{
			if (Game.Instance.Controllers.FogOfWar.FowStateIsSet && !FogOfWarScheduleController.FowIsActive)
			{
				IsInFogOfWar = false;
			}
			FogOfWarCulling.UpdateTarget(m_FogOfWarTarget);
		}
	}

	protected Entity(string uniqueId, bool isInGame)
	{
		Parts = new EntityPartsManager(this);
		Facts = new EntityFactsManager(this);
		UniqueId = uniqueId;
		m_IsInGame = isInGame;
	}

	protected Entity(JsonConstructorMark _)
	{
	}

	protected Entity(OwlPackConstructorParameter _)
	{
	}

	protected Entity()
	{
	}

	private void Initialize()
	{
		IsPrePostLoadExecuted = true;
		IsPostLoadExecuted = true;
		Subscribe();
		EntityService.Instance?.Register(this);
		OnPrepareOrPrePostLoad();
		OnCreateParts();
		OnInitialize();
		if (!ContextData<UnitHelper.ChargenUnit>.Current)
		{
			PartUnitBody optional = GetOptional<PartUnitBody>();
			if (optional != null)
			{
				optional.Initialize();
				optional.InitializeWeapons(optional.Owner.OriginalBlueprint.Body);
			}
		}
		IsInitialized = true;
	}

	protected virtual void OnPrepareOrPrePostLoad()
	{
	}

	protected virtual void OnCreateParts()
	{
	}

	protected virtual void OnInitialize()
	{
	}

	protected virtual void OnIsInGameChanged()
	{
	}

	public void SetHoldingState([CanBeNull] SceneEntitiesState newHoldingState)
	{
		if (m_HoldingState != newHoldingState && !IsDisposed && !IsDisposingNow)
		{
			m_HoldingState = newHoldingState;
			Facts.OnHoldingStateChanged();
			Parts.OnHoldingStateChanged();
			try
			{
				OnHoldingStateChanged();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			EventBus.RaiseEvent((IEntity)this, (Action<IEntityHoldingStateChangedHandler>)delegate(IEntityHoldingStateChangedHandler h)
			{
				h.HandleEntityHoldingStateChanged();
			}, isCheckRuntime: true);
		}
	}

	public void AttachToViewOnLoad([CanBeNull] IEntityViewBase view)
	{
		if (view != null && view.GO != null)
		{
			UniqueId = view.UniqueViewId;
		}
		else
		{
			view = CreateViewForData();
			view?.MarkCreatedAtRuntime();
			if (view == null)
			{
				if (IsInGame && NeedsView)
				{
					IsInGame = false;
					PFLog.Default.Error("Entity data '{0}' (id={1}) failed to create a view on load", GetType().Name, UniqueId);
				}
				return;
			}
		}
		AttachView(view);
	}

	public void PrePostLoad()
	{
		if (!IsPrePostLoadExecuted || (bool)ContextData<ForcePostLoad>.Current)
		{
			IsInitialized = true;
			EntityService.Instance?.Register(this);
			Facts.PrePostLoad(this);
			Parts.PrePostLoad(this);
			try
			{
				OnPrepareOrPrePostLoad();
				OnPrePostLoad();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			IsPrePostLoadExecuted = true;
		}
	}

	public void PostLoad()
	{
		if (IsPostLoadExecuted && !ContextData<ForcePostLoad>.Current)
		{
			PFLog.Entity.Error($"EntityDataBase.PostLoad: already executed ({this})");
			return;
		}
		PrePostLoad();
		try
		{
			OnPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
		Facts.PostLoad();
		Parts.PostLoad();
		try
		{
			OnDidPostLoad();
		}
		catch (Exception ex2)
		{
			PFLog.Entity.Exception(ex2);
		}
		Facts.DidPostLoad();
		Parts.DidPostLoad();
		IsPostLoadExecuted = true;
	}

	public void ApplyPostLoadFixes()
	{
		Facts.ApplyPostLoadFixes();
		Parts.ApplyPostLoadFixes();
		try
		{
			OnApplyPostLoadFixes();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
	}

	public void PreSave()
	{
		try
		{
			OnPreSave();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
		Parts.PreSave();
	}

	public void Subscribe()
	{
		if (!IsSubscribedOnEventBus)
		{
			Facts.Subscribe();
			Parts.Subscribe();
			EventBus.Subscribe(this);
			try
			{
				OnSubscribe();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			IsSubscribedOnEventBus = true;
		}
	}

	public void Unsubscribe()
	{
		if (IsSubscribedOnEventBus)
		{
			Facts.Unsubscribe();
			Parts.Unsubscribe();
			EventBus.Unsubscribe(this);
			try
			{
				OnUnsubscribe();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			IsSubscribedOnEventBus = false;
		}
	}

	public void AttachView([NotNull] IEntityViewBase view)
	{
		if (view == null)
		{
			PFLog.Default.ErrorWithReport("Try attach null view to " + ToString());
		}
		else
		{
			if (View == view)
			{
				return;
			}
			if (View != null)
			{
				DetachView();
			}
			View = view;
			View.AttachToData(this);
			m_view.EntityPartComponentEnsureEntityPart(Parts);
			AbstractEntityPartComponent[] components = m_view.GetComponents<AbstractEntityPartComponent>();
			Parts.RemoveAll((ViewBasedPart i) => i.ShouldCheckSourceComponent && !components.HasItem((AbstractEntityPartComponent ii) => i.SourceType == ii.GetType().Name));
			try
			{
				OnViewDidAttach();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			Parts.ViewDidAttach();
			Facts.ViewDidAttach();
			EventBus.RaiseEvent((IEntity)this, (Action<IViewAttachedHandler>)delegate(IViewAttachedHandler h)
			{
				h.OnViewAttached(view);
			}, isCheckRuntime: true);
		}
	}

	public void DetachView()
	{
		if (View != null)
		{
			EventBus.RaiseEvent((IEntity)this, (Action<IViewDetachedHandler>)delegate(IViewDetachedHandler h)
			{
				h.OnViewDetached(View);
			}, isCheckRuntime: true);
			Parts.ViewWillDetach();
			Facts.ViewWillDetach();
			try
			{
				OnViewWillDetach();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			View.Or(null)?.DetachFromData();
			View = null;
		}
	}

	public void AreaLoadingComplete()
	{
		Parts.AreaLoadingComplete();
		Facts.AreaLoadingComplete();
		try
		{
			OnAreaLoadingComplete();
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
		}
	}

	public void HandleDestroy()
	{
		if (!Destroyed)
		{
			if (HoldingState != null)
			{
				PFLog.Default.ErrorWithReport("It is unsafe to destroy entities which still in game state");
			}
			try
			{
				OnDestroy();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			Destroyed = true;
			EventBus.RaiseEvent((IEntity)this, (Action<IEntityDestructionHandler>)delegate(IEntityDestructionHandler h)
			{
				h.HandleEntityDestroyed();
			}, isCheckRuntime: true);
			EventBus.ClearEntitySubscriptions(this);
		}
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			DisposeImplementation();
		}
	}

	protected virtual void DisposeImplementation()
	{
		m_HoldingState?.RemoveEntityData(this);
		m_HoldingState = null;
		Unsubscribe();
		try
		{
			IsDisposingNow = true;
			try
			{
				OnDispose();
			}
			catch (Exception ex)
			{
				PFLog.Entity.Exception(ex);
			}
			IEntityViewBase view = View;
			ViewHandlingOnDisposePolicyType viewHandlingOnDisposePolicyType = m_ViewHandlingOnDisposePolicyOverride ?? DefaultViewHandlingOnDisposePolicy;
			DetachView();
			if (view != null)
			{
				switch (viewHandlingOnDisposePolicyType)
				{
				case ViewHandlingOnDisposePolicyType.Destroy:
				case ViewHandlingOnDisposePolicyType.FadeOutAndDestroy:
					view.DestroyViewObject();
					break;
				case ViewHandlingOnDisposePolicyType.Deactivate:
					view.GO.SetActive(value: false);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				case ViewHandlingOnDisposePolicyType.Keep:
					break;
				}
			}
			Parts.Dispose();
			Facts.Dispose();
		}
		finally
		{
			EntityService.Instance?.Unregister(this);
			IsDisposed = true;
			IsDisposingNow = false;
		}
	}

	protected virtual void OnHoldingStateChanged()
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

	protected virtual void OnPreSave()
	{
	}

	protected virtual void OnSubscribe()
	{
	}

	protected virtual void OnUnsubscribe()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void OnDispose()
	{
	}

	protected virtual void OnViewDidAttach()
	{
		TrySetTransformFromConfigOnLoad();
		if (SetViewTransform && !SetTransformFromConfigOnLoad)
		{
			View.ViewTransform.position = EntityPositionToViewPosition(Position);
			if (SetViewOrientation)
			{
				View.ViewTransform.rotation = Quaternion.Euler(View.ViewTransform.rotation.eulerAngles.x, Orientation, View.ViewTransform.rotation.eulerAngles.z);
			}
		}
		if (Application.isPlaying && IsAffectedByFogOfWar)
		{
			if (m_FogOfWarTarget == null)
			{
				m_FogOfWarTarget = new FogOfWarTarget(this);
			}
			IsInFogOfWar = !Game.Instance.Controllers.FogOfWar.FowStateIsSet || FogOfWarScheduleController.FowIsActive;
			FogOfWarCulling.RegisterTarget(m_FogOfWarTarget);
		}
	}

	protected virtual void OnViewWillDetach()
	{
		if (m_FogOfWarTarget != null && Application.isPlaying)
		{
			FogOfWarCulling.UnregisterTarget(m_FogOfWarTarget);
			m_FogOfWarTarget = null;
		}
	}

	protected virtual void OnAreaLoadingComplete()
	{
	}

	[CanBeNull]
	protected abstract IEntityViewBase CreateViewForData();

	public IEntity GetSubscribingEntity()
	{
		return this;
	}

	public override string ToString()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		string value = ((View != null) ? View.GO.name : "-");
		builder.Append(GetType().Name);
		builder.Append(" [");
		builder.Append(value);
		builder.Append("] #");
		builder.Append(UniqueId);
		return builder.ToString();
	}

	private void TrySetTransformFromConfigOnLoad()
	{
		IEntityViewBase view = View;
		if (view != null && !view.CreatedAtRuntime)
		{
			Transform viewTransform = View.ViewTransform;
			if (!m_InitialPosition.HasValue || SetTransformFromConfigOnLoad || (m_InitialPosition.Value - m_Position).magnitude < 0.1f)
			{
				Vector3 position = viewTransform.position;
				Vector3 position2 = ViewPositionToEntityPosition(position);
				m_InitialPosition = (m_Position = position2);
			}
			if (!m_InitialOrientation.HasValue || SetTransformFromConfigOnLoad || Math.Abs(m_InitialOrientation.Value - m_Orientation) < 1f)
			{
				float y = viewTransform.eulerAngles.y;
				m_InitialOrientation = (m_Orientation = y);
			}
		}
	}

	public void SetTransformFromView()
	{
		IEntityViewBase view = View;
		if (view != null)
		{
			Transform viewTransform = view.ViewTransform;
			if ((object)viewTransform != null)
			{
				Position = ViewPositionToEntityPosition(viewTransform.position);
				Orientation = viewTransform.eulerAngles.y;
			}
		}
	}

	private void UpdateChildrenPositions()
	{
		ReadonlyList<EntityRef> children = Children;
		if (children.Count <= 0)
		{
			return;
		}
		using (ContextData<DisableSetViewTransform>.Request())
		{
			Vector3 vector = m_Position - (m_InitialPosition ?? m_Position);
			foreach (EntityRef item in children)
			{
				if (item.Entity is Entity entity)
				{
					entity.Position = entity.InitialPosition + vector;
				}
			}
		}
	}

	private void ApplyOrientationToChildren()
	{
		ReadonlyList<EntityRef> children = Children;
		if (children.Count <= 0)
		{
			return;
		}
		using (ContextData<DisableSetViewTransform>.Request())
		{
			foreach (EntityRef item in children)
			{
				if (item.Entity is Entity child)
				{
					ApplyOrientationToChild(child);
				}
			}
		}
	}

	private void ApplyOrientationToChild(Entity child)
	{
		(child.Position, child.Orientation) = GetPositionAndOrientationForChild(InitialPosition, InitialOrientation, Position, Orientation, child.InitialPosition, child.InitialOrientation);
	}

	protected static (Vector3 position, float orientation) GetPositionAndOrientationForChild(Vector3 initialParentPosition, float initialParentOrientation, Vector3 currentParentPosition, float currentParentOrientation, Vector3 initialChildPosition, float initialChildOrientation)
	{
		Vector3 vector = Quaternion.Euler(0f, 0f - initialParentOrientation, 0f) * (initialChildPosition - initialParentPosition);
		float num = initialChildOrientation - initialParentOrientation;
		Quaternion quaternion = Quaternion.Euler(0f, currentParentOrientation, 0f);
		Vector3 item = currentParentPosition + quaternion * vector;
		float item2 = currentParentOrientation + num;
		return (position: item, orientation: item2);
	}

	protected virtual void OnPositionChanged()
	{
		if (m_FogOfWarTarget != null && !AlwaysRevealedInFogOfWar)
		{
			FogOfWarCulling.UpdateTarget(m_FogOfWarTarget);
		}
		EventBus.RaiseEvent((IEntity)this, (Action<IEntityPositionChangedHandler>)delegate(IEntityPositionChangedHandler h)
		{
			h.HandleEntityPositionChanged();
		}, isCheckRuntime: true);
	}

	protected virtual Vector3 ViewPositionToEntityPosition(Vector3 viewPosition)
	{
		return viewPosition;
	}

	protected virtual Vector3 EntityPositionToViewPosition(Vector3 entityPosition)
	{
		return entityPosition;
	}

	[CanBeNull]
	public TPart GetOptional<TPart>() where TPart : EntityPart, new()
	{
		return Parts.GetOptional<TPart>();
	}

	[CanBeNull]
	public TPart GetOptional<TPart>(Type type) where TPart : EntityPart
	{
		return Parts.GetOptional<TPart>(type);
	}

	[NotNull]
	public TPart GetRequired<TPart>() where TPart : EntityPart, new()
	{
		return Parts.GetRequired<TPart>();
	}

	public TPart GetOrCreate<TPart>() where TPart : EntityPart, new()
	{
		return Parts.GetOrCreate<TPart>();
	}

	public void Remove<TPart>() where TPart : EntityPart, new()
	{
		Parts.Remove<TPart>();
	}

	public EntityPartsManager.PartsByTypeEnumerable<TPart> GetAll<TPart>() where TPart : class
	{
		return Parts.GetAll<TPart>();
	}

	public void AssertRequiredPart<TPart>() where TPart : EntityPart, new()
	{
		GetRequired<TPart>();
	}

	[NotNull]
	public static TEntity Initialize<TEntity>([NotNull] TEntity entity) where TEntity : Entity
	{
		entity.Initialize();
		return entity;
	}

	public void SetViewHandlingOnDisposePolicy(ViewHandlingOnDisposePolicyType policy)
	{
		m_ViewHandlingOnDisposePolicyOverride = policy;
	}

	public static implicit operator Entity(EntityRef @ref)
	{
		return (Entity)@ref.Entity;
	}

	public static implicit operator EntityReference([CanBeNull] Entity entity)
	{
		return new EntityReference
		{
			UniqueId = entity?.UniqueId
		};
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(UniqueId);
		result.Append(ref m_IsInGame);
		result.Append(ref m_Position);
		result.Append(ref m_Orientation);
		if (m_InitialPosition.HasValue)
		{
			Vector3 val = m_InitialPosition.Value;
			result.Append(ref val);
		}
		if (m_InitialOrientation.HasValue)
		{
			float val2 = m_InitialOrientation.Value;
			result.Append(ref val2);
		}
		Hash128 val3 = ClassHasher<EntityFactsManager>.GetHash128(Facts);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<EntityPartsManager>.GetHash128(Parts);
		result.Append(ref val4);
		result.Append(ref m_IsRevealed);
		if (m_ViewHandlingOnDisposePolicyOverride.HasValue)
		{
			ViewHandlingOnDisposePolicyType val5 = m_ViewHandlingOnDisposePolicyOverride.Value;
			result.Append(ref val5);
		}
		return result;
	}

	public abstract void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter;

	public abstract void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter;
}
