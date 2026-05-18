using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Core.Cheats;
using Kingmaker.Controllers;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Gameplay.Utility;
using Kingmaker.Interaction;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using Pathfinding.Util;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Kingmaker.View.MapObjects;

[OwlPackable(OwlPackableMode.Generate)]
public class InteractionDoorPart : InteractionPart<InteractionDoorSettings>, IHasInteractionVariantActors, IUpdatable, IInterpolatable, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, ISubscriber, IEventTag<IInGameHandler, EntitySubscriber>, IHashable, IOwlPackable<InteractionDoorPart>
{
	private const float OpenSpeed = 1f;

	private const float CloseSpeed = -1f;

	private const int PlayableIndex = 0;

	private Animator m_Animator;

	private PlayableDirector m_PlayableDirector;

	private float m_PreviousTime;

	private float m_CurrentTime;

	private bool m_IsPlayableDirectorAttached;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_State;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_Inited;

	[OwlPackInclude]
	private bool m_OnOpenActionsHasBeenRun;

	[OwlPackInclude]
	private bool m_OnCloseActionsHasBeenRun;

	private bool WasAnimationFinished = true;

	private bool m_InteractThroughVariants;

	private int _blockerCacheFrame = -1;

	private bool _blockerCacheResult;

	private bool _lastBlockerState;

	private bool _lastBlockerInitialized;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InteractionDoorPart",
		OldNames = null,
		Fields = new FieldInfo[9]
		{
			new FieldInfo("SourceType", typeof(string)),
			new FieldInfo("AlreadyUnlocked", typeof(bool)),
			new FieldInfo("AlreadyVisited", typeof(bool)),
			new FieldInfo("m_LastCombatRoundInteractionAttempt", typeof(int)),
			new FieldInfo("m_Enabled", typeof(bool)),
			new FieldInfo("m_State", typeof(bool)),
			new FieldInfo("m_Inited", typeof(bool)),
			new FieldInfo("m_OnOpenActionsHasBeenRun", typeof(bool)),
			new FieldInfo("m_OnCloseActionsHasBeenRun", typeof(bool))
		}
	};

	public bool IsOpen => m_State;

	public bool IsAnimationFinished
	{
		get
		{
			if (!m_IsPlayableDirectorAttached)
			{
				return true;
			}
			if (!(m_PlayableDirector.playableGraph.GetRootPlayable(0).GetSpeed() > 0.0))
			{
				return m_CurrentTime <= 0f;
			}
			return m_CurrentTime >= base.Settings.ObstacleAnimation.length;
		}
	}

	public override bool InteractThroughVariants
	{
		get
		{
			if (m_InteractThroughVariants && !base.AlreadyUnlocked)
			{
				return !m_State;
			}
			return false;
		}
	}

	public float OvertipCorrection => 0f;

	public override bool Enabled
	{
		get
		{
			if (!base.Settings.AlwaysDisabled && m_Enabled)
			{
				return !IsClosingBlocked();
			}
			return false;
		}
		set
		{
			bool flag = !base.Settings.AlwaysDisabled && value;
			if (m_Enabled != flag)
			{
				m_Enabled = flag;
				base.View?.UpdateHighlight();
				OnEnabledChanged();
				base.EventBus.RaiseEvent((IMapObjectEntity)base.Owner, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
				{
					h.HandleObjectInteractChanged();
				}, isCheckRuntime: true);
			}
		}
	}

	public bool CanClose => CheckCanClose(checkOutsideCombat: false);

	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		base.View.FogOfWarFudgeRadius = 0.5f;
		m_Animator = base.View?.GetComponentInChildren<Animator>();
		if (m_Animator != null)
		{
			m_Animator.fireEvents = true;
		}
		StaticRendererLink[] array = base.View?.GetComponents<StaticRendererLink>();
		if (array != null)
		{
			StaticRendererLink[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].DoLink();
			}
		}
		m_Animator.Or(null)?.Update(0f);
		AttachPlayableDirector();
		if (!m_Inited)
		{
			m_Inited = true;
			if (base.Settings.OpenByDefault)
			{
				Open();
			}
		}
		if ((bool)base.Settings.HideWhenOpen)
		{
			Renderer renderer = base.Settings.HideWhenOpen.FindLinkedTransform()?.GetComponent<Renderer>();
			if ((bool)renderer)
			{
				renderer.enabled = !m_State;
			}
		}
		SetNavmeshCutState();
		SetGridObstacleState();
	}

	protected override void OnDetach()
	{
		DetachPlayableDirector();
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		Open(user);
	}

	protected override void OnDidInteract(BaseUnitEntity user)
	{
		base.OnDidInteract(user);
		SetUnlocked();
	}

	public void Open(BaseUnitEntity user = null)
	{
		PlayOpen();
		m_State = !m_State;
		if ((bool)base.Settings.HideWhenOpen)
		{
			Renderer renderer = base.Settings.HideWhenOpen.FindLinkedTransform()?.GetComponent<Renderer>();
			if ((bool)renderer)
			{
				renderer.enabled = !m_State;
			}
		}
		string text = (m_State ? base.Settings.OpenSound : base.Settings.CloseSound);
		if (!string.IsNullOrEmpty(text))
		{
			SoundEventsManager.PostEvent(text, base.View?.gameObject);
		}
		if (base.Settings.DisableOnOpen)
		{
			Enabled = false;
		}
		base.EventBus.RaiseEvent((IMapObjectEntity)base.Owner, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
		{
			h.HandleObjectInteractChanged();
		}, isCheckRuntime: true);
		SetNavmeshCutState();
		if (IsOpen)
		{
			if (base.Settings.DoOpenActionsOnce && m_OnOpenActionsHasBeenRun)
			{
				return;
			}
			m_OnOpenActionsHasBeenRun = true;
			ActionsHolder actionsHolder = base.Settings.OnOpenActions?.Get();
			if (actionsHolder == null || !actionsHolder.HasActions)
			{
				return;
			}
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				using (ContextData<InteractingUnitData>.Request().Setup(user))
				{
					actionsHolder.Run();
					return;
				}
			}
		}
		if (base.Settings.DoCloseActionsOnce && m_OnCloseActionsHasBeenRun)
		{
			return;
		}
		m_OnCloseActionsHasBeenRun = true;
		ActionsHolder actionsHolder2 = base.Settings.OnCloseActions?.Get();
		if (actionsHolder2 == null || !actionsHolder2.HasActions)
		{
			return;
		}
		using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(user))
			{
				actionsHolder2.Run();
			}
		}
	}

	private void SetNavmeshCutState()
	{
		List<NavmeshCut> list = ((base.Settings.NavmeshCutAction == InteractionDoorSettings.NavMeshCutActionSettings.DoNotTouchNavmeshCut) ? null : base.View?.NavmeshCuts);
		if (list == null)
		{
			return;
		}
		foreach (NavmeshCut item in list)
		{
			item.enabled = base.Settings.NavmeshCutAction == InteractionDoorSettings.NavMeshCutActionSettings.EnableNavmeshCutWhenOpen == m_State;
		}
	}

	private void SetGridObstacleState()
	{
		ReadonlyList<GridObstacle>? readonlyList = ((base.Settings.GridObstacleAction == InteractionDoorSettings.GridObstacleActionSettings.DoNotTouchGridObstacle) ? null : base.View?.GridObstacles);
		if (!readonlyList.HasValue)
		{
			return;
		}
		foreach (GridObstacle item in readonlyList.Value)
		{
			item.enabled = base.Settings.GridObstacleAction == InteractionDoorSettings.GridObstacleActionSettings.EnableGridObstacleWhenOpen == m_State;
		}
	}

	public bool GetState()
	{
		return m_State;
	}

	IEnumerable<IInteractionVariantActor> IHasInteractionVariantActors.GetInteractionVariantActors()
	{
		if (Type == InteractionType.Direct || !InteractThroughVariants)
		{
			return null;
		}
		EntityPartsManager.PartsByTypeEnumerable<IInteractionVariantActor> all = base.View.Data.Parts.GetAll<IInteractionVariantActor>();
		if (all.Any((IInteractionVariantActor x) => x is KeyRestrictionPart && x.CheckRestriction(GameHelper.GetPlayerCharacter())))
		{
			return null;
		}
		return all.Where((IInteractionVariantActor x) => !(x is KeyRestrictionPart)).DefaultIfEmpty();
	}

	protected override void ConfigureRestrictions()
	{
		SleightOfHandRestriction component = base.View.GetComponent<SleightOfHandRestriction>();
		if (component != null)
		{
			base.Settings.SetShowOvertipOverride(value: false);
			component.Settings.InteractOnlyWithToolIfFailed = true;
			base.View.Data.Parts.GetOrCreate<SleightOfHandMultikeyItemRestrictionPart>().Settings.CopyDCData(component.Settings);
			base.View.Data.Parts.GetOrCreate<DemolitionMeltaChargeRestrictionPart>().Settings.CopyDCData(component.Settings);
			m_InteractThroughVariants = true;
		}
		LoreXenosRestriction component2 = base.View.GetComponent<LoreXenosRestriction>();
		if (component2 != null)
		{
			base.Settings.SetShowOvertipOverride(value: false);
			component2.Settings.InteractOnlyWithToolIfFailed = true;
			base.View.Data.Parts.GetOrCreate<LoreXenosMultikeyItemRestrictionPart>().Settings.CopyDCData(component2.Settings);
			m_InteractThroughVariants = true;
		}
	}

	void IUpdatable.Tick(float delta)
	{
		TickBlockerStateEvent();
		if (!(m_PlayableDirector == null) && m_PlayableDirector.state != 0)
		{
			m_PreviousTime = m_CurrentTime;
			m_CurrentTime += ((m_PlayableDirector.playableGraph.GetRootPlayable(0).GetSpeed() > 0.0) ? delta : (0f - delta));
			m_PlayableDirector.time = m_PreviousTime;
			m_PlayableDirector.Evaluate();
			if (WasAnimationFinished && !IsAnimationFinished)
			{
				WasAnimationFinished = false;
			}
			else if (!WasAnimationFinished && IsAnimationFinished)
			{
				WasAnimationFinished = true;
				OnAnimationFinished();
			}
			if (WasAnimationFinished && IsAnimationFinished)
			{
				m_PlayableDirector.Pause();
			}
		}
	}

	private void OnAnimationFinished()
	{
		SetGridObstacleState();
		m_PlayableDirector.Pause();
	}

	void IInterpolatable.Tick(float progress)
	{
		if (m_PlayableDirector.state != 0)
		{
			float num = Mathf.LerpUnclamped(m_PreviousTime, m_CurrentTime, progress);
			if (!(Math.Abs((double)num - m_PlayableDirector.time) < 0.0001))
			{
				m_PlayableDirector.time = num;
				m_PlayableDirector.Evaluate();
			}
		}
	}

	private bool AttachPlayableDirector()
	{
		if (base.Owner.View == null || m_Animator == null || !base.View.gameObject.activeInHierarchy)
		{
			return false;
		}
		m_PlayableDirector = base.Owner.View.gameObject.EnsureComponent<PlayableDirector>();
		m_PlayableDirector.timeUpdateMode = DirectorUpdateMode.Manual;
		TimelineAsset timelineAsset = m_PlayableDirector.playableAsset as TimelineAsset;
		if (timelineAsset == null)
		{
			timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
			m_PlayableDirector.playableAsset = timelineAsset;
		}
		AnimationTrack animationTrack = timelineAsset.CreateTrack<AnimationTrack>(null, "AnimationTrack");
		TimelineClip timelineClip = animationTrack.CreateClip(base.Settings.ObstacleAnimation);
		if (timelineClip.animationClip.hasGenericRootTransform)
		{
			animationTrack.position = m_Animator.transform.position;
			animationTrack.rotation = m_Animator.transform.rotation;
		}
		m_PlayableDirector.SetGenericBinding(animationTrack, m_Animator);
		m_Animator.runtimeAnimatorController = null;
		m_Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		m_PlayableDirector.time = (m_State ? timelineClip.duration : 0.0);
		m_CurrentTime = (float)m_PlayableDirector.time;
		m_PreviousTime = m_CurrentTime;
		m_PlayableDirector.Play();
		m_PlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(m_State ? 1f : (-1f));
		Game.Instance.Controllers.DoorUpdateController.Add(this);
		Game.Instance.Controllers.InterpolationController.Add(this);
		m_IsPlayableDirectorAttached = true;
		return true;
	}

	private void DetachPlayableDirector()
	{
		if (m_IsPlayableDirectorAttached)
		{
			m_IsPlayableDirectorAttached = false;
			Game.Instance.Controllers.DoorUpdateController.Remove(this);
			Game.Instance.Controllers.InterpolationController.Remove(this);
			if (!(m_PlayableDirector == null) && m_PlayableDirector.playableGraph.IsValid())
			{
				m_PlayableDirector.playableGraph.Destroy();
			}
		}
	}

	private void PlayOpen()
	{
		if (m_IsPlayableDirectorAttached || AttachPlayableDirector())
		{
			float num = (m_State ? (-1f) : 1f);
			m_PlayableDirector.time = Mathf.Clamp((float)m_PlayableDirector.time, 0f, base.Settings.ObstacleAnimation.length);
			m_CurrentTime = (float)m_PlayableDirector.time;
			m_PreviousTime = m_CurrentTime;
			m_PlayableDirector.Play();
			m_PlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(num);
		}
	}

	[Cheat(Name = "toggle_door", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ToggleDoor(string name, bool newState)
	{
		foreach (InteractionDoorPart item in from mapObj in Game.Instance.EntityPools.MapObjects
			where mapObj.IsInGame
			where mapObj.View != null && mapObj.View.name.Contains(name, StringComparison.InvariantCultureIgnoreCase)
			let doorPart = mapObj.GetOptional<InteractionDoorPart>()
			where doorPart != null
			where doorPart.IsOpen != newState
			select doorPart)
		{
			item.Open();
		}
	}

	public void HandleObjectInGameChanged()
	{
		if (base.Owner.IsInGame)
		{
			if (!m_IsPlayableDirectorAttached)
			{
				AttachPlayableDirector();
			}
		}
		else
		{
			DetachPlayableDirector();
		}
	}

	public bool CheckCanClose(bool checkOutsideCombat)
	{
		if (IsOpen)
		{
			return !IsClosingBlocked(checkOutsideCombat);
		}
		return true;
	}

	private void TickBlockerStateEvent()
	{
		bool flag = IsClosingBlocked();
		if (!_lastBlockerInitialized)
		{
			_lastBlockerInitialized = true;
			_lastBlockerState = flag;
		}
		else if (_lastBlockerState != flag)
		{
			_lastBlockerState = flag;
			base.View?.UpdateHighlight();
			base.EventBus.RaiseEvent((IMapObjectEntity)base.Owner, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectInteractChanged();
			}, isCheckRuntime: true);
		}
	}

	private bool IsClosingBlocked(bool checkOutsideCombat = false)
	{
		if (checkOutsideCombat)
		{
			return ComputeIsClosingBlocked(checkOutsideCombat: true);
		}
		int frameCount = Time.frameCount;
		if (_blockerCacheFrame == frameCount)
		{
			return _blockerCacheResult;
		}
		_blockerCacheFrame = frameCount;
		_blockerCacheResult = ComputeIsClosingBlocked(checkOutsideCombat: false);
		return _blockerCacheResult;
	}

	private bool ComputeIsClosingBlocked(bool checkOutsideCombat)
	{
		if (!checkOutsideCombat)
		{
			Game instance = Game.Instance;
			if (instance == null || instance.Controllers?.TurnController?.TurnBasedModeActive != true)
			{
				return false;
			}
		}
		IMapObjectView view = base.View;
		if (view == null)
		{
			return false;
		}
		bool flag = WouldGridObstacleActivateOnToggle() && view.GridObstacles.Count > 0;
		int num;
		if (WouldNavmeshCutActivateOnToggle())
		{
			List<NavmeshCut> navmeshCuts = view.NavmeshCuts;
			num = ((navmeshCuts != null && navmeshCuts.Count > 0) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag2 = (byte)num != 0;
		if (!flag && !flag2)
		{
			return false;
		}
		GridNodeToEntityCache gridNodeToEntityCache = Game.Instance?.GridNodeToEntityCache;
		if (gridNodeToEntityCache == null)
		{
			return false;
		}
		if (flag && AnyObstacleBlocked(view.GridObstacles, gridNodeToEntityCache))
		{
			return true;
		}
		if (flag2 && AnyCutBlocked(view.NavmeshCuts))
		{
			return true;
		}
		return false;
	}

	private bool WouldNavmeshCutActivateOnToggle()
	{
		InteractionDoorSettings settings = base.Settings;
		if (settings == null)
		{
			return false;
		}
		return settings.NavmeshCutAction switch
		{
			InteractionDoorSettings.NavMeshCutActionSettings.EnableNavmeshCutWhenClosed => m_State, 
			InteractionDoorSettings.NavMeshCutActionSettings.EnableNavmeshCutWhenOpen => !m_State, 
			_ => false, 
		};
	}

	private bool WouldGridObstacleActivateOnToggle()
	{
		InteractionDoorSettings settings = base.Settings;
		if (settings == null)
		{
			return false;
		}
		return settings.GridObstacleAction switch
		{
			InteractionDoorSettings.GridObstacleActionSettings.EnableGridObstacleWhenClosed => m_State, 
			InteractionDoorSettings.GridObstacleActionSettings.EnableGridObstacleWhenOpen => !m_State, 
			_ => false, 
		};
	}

	private bool AnyObstacleBlocked(IReadOnlyList<GridObstacle> obstacles, GridNodeToEntityCache cache)
	{
		GridGraph gridGraph = AstarPath.active?.data?.gridGraph;
		if (gridGraph == null)
		{
			return false;
		}
		GraphTransform transform = gridGraph.transform;
		for (int i = 0; i < obstacles.Count; i++)
		{
			GridObstacle gridObstacle = obstacles[i];
			if (!(gridObstacle == null))
			{
				var (node, node2) = gridObstacle.GetAffectedNodes(transform);
				if (cache.ContainsEntity(node, IsBlocker) || cache.ContainsEntity(node2, IsBlocker))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool AnyCutBlocked(IReadOnlyList<NavmeshCut> cuts)
	{
		EntityPools entityPools = Game.Instance?.EntityPools;
		if (entityPools == null)
		{
			return false;
		}
		foreach (AbstractUnitEntity allUnit in entityPools.AllUnits)
		{
			if (!IsBlocker(allUnit))
			{
				continue;
			}
			Vector3 position = allUnit.Position;
			for (int i = 0; i < cuts.Count; i++)
			{
				NavmeshCut navmeshCut = cuts[i];
				if (navmeshCut != null && IsPointInsideCut(navmeshCut, position))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsBlocker(MechanicEntity entity)
	{
		if (entity == null || entity.IsDisposed || !entity.IsInGame)
		{
			return false;
		}
		if (entity == base.Owner)
		{
			return false;
		}
		return true;
	}

	private static bool IsPointInsideCut(NavmeshCut cut, Vector3 worldPos)
	{
		Vector3 vector = cut.transform.InverseTransformPoint(worldPos) - cut.center;
		switch (cut.type)
		{
		case NavmeshCut.MeshType.Rectangle:
		case NavmeshCut.MeshType.Box:
			if (Mathf.Abs(vector.x) <= cut.rectangleSize.x * 0.5f)
			{
				return Mathf.Abs(vector.z) <= cut.rectangleSize.y * 0.5f;
			}
			return false;
		case NavmeshCut.MeshType.Circle:
		case NavmeshCut.MeshType.Sphere:
			return vector.x * vector.x + vector.z * vector.z <= cut.circleRadius * cut.circleRadius;
		default:
			return vector.x * vector.x + vector.z * vector.z <= cut.circleRadius * cut.circleRadius;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_State);
		result.Append(ref m_Inited);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InteractionDoorPart source = new InteractionDoorPart();
		result = Unsafe.As<InteractionDoorPart, TPossiblyBase>(ref source);
	}

	public override void Serialize<TFormatter>(TFormatter formatter, SerializerState state)
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<InteractionDoorPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		bool value2 = base.AlreadyUnlocked;
		formatter.UnmanagedField(1, "AlreadyUnlocked", ref value2, state);
		bool value3 = AlreadyVisited;
		formatter.UnmanagedField(2, "AlreadyVisited", ref value3, state);
		formatter.UnmanagedField(3, "m_LastCombatRoundInteractionAttempt", ref m_LastCombatRoundInteractionAttempt, state);
		formatter.UnmanagedField(4, "m_Enabled", ref m_Enabled, state);
		formatter.UnmanagedField(5, "m_State", ref m_State, state);
		formatter.UnmanagedField(6, "m_Inited", ref m_Inited, state);
		formatter.UnmanagedField(7, "m_OnOpenActionsHasBeenRun", ref m_OnOpenActionsHasBeenRun, state);
		formatter.UnmanagedField(8, "m_OnCloseActionsHasBeenRun", ref m_OnCloseActionsHasBeenRun, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InteractionDoorPart>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			switch (mappingForType[fieldID])
			{
			case byte.MaxValue:
				formatter.SkipField(size);
				break;
			case 0:
				base.SourceType = formatter.ReadString(state);
				break;
			case 1:
				base.AlreadyUnlocked = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				AlreadyVisited = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				m_LastCombatRoundInteractionAttempt = formatter.ReadUnmanaged<int>(state);
				break;
			case 4:
				m_Enabled = formatter.ReadUnmanaged<bool>(state);
				break;
			case 5:
				m_State = formatter.ReadUnmanaged<bool>(state);
				break;
			case 6:
				m_Inited = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				m_OnOpenActionsHasBeenRun = formatter.ReadUnmanaged<bool>(state);
				break;
			case 8:
				m_OnCloseActionsHasBeenRun = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
