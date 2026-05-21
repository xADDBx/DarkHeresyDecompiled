using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.FogOfWar.LineOfSight;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.Gameplay.Features.AreaEffects.Parts;
using Kingmaker.Gameplay.Features.AreaEffects.Shapes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public class AreaEffectEntity : MechanicEntity<BlueprintAreaEffect>, IAreaHandler, ISubscriber, IAreaEffectEntity, IMechanicEntity, IEntity, IDisposable, IEntityPositionChangedHandler, ISubscriber<IEntity>, ICameraFocusTarget, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnEndHandler, ICombatParticipant, IHashable, IOwlPackable<AreaEffectEntity>
{
	public delegate void EntityEventDelegate(MechanicEntity entity);

	public interface IEntityWithinBoundsHandler
	{
		HashSet<EntityRef<MechanicEntity>> Entered { get; }

		HashSet<EntityRef<MechanicEntity>> Exited { get; }

		void ClearDelta();
	}

	[OwlPackable(OwlPackableMode.Generate)]
	public struct EntityInfo : IEquatable<EntityInfo>, IHashable, IOwlPackable, IOwlPackable<EntityInfo>
	{
		[JsonProperty]
		[OwlPackInclude]
		public EntityRef<MechanicEntity> Reference;

		[JsonProperty(IsReference = false)]
		[OwlPackInclude]
		public Vector3 PreviousNodePosition;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "EntityInfo",
			Fields = new FieldInfo[2]
			{
				new FieldInfo("Reference", typeof(EntityRef<MechanicEntity>)),
				new FieldInfo("PreviousNodePosition", typeof(Vector3))
			}
		};

		public bool IsValid => Reference.Entity?.IsInState ?? false;

		public EntityInfo(EntityRef<MechanicEntity> reference)
		{
			this = default(EntityInfo);
			Reference = reference;
		}

		public bool Equals(EntityInfo other)
		{
			return Reference.Equals(other.Reference);
		}

		public override bool Equals(object obj)
		{
			if (obj is EntityInfo other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Reference.GetHashCode();
		}

		public Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			EntityRef<MechanicEntity> obj = Reference;
			Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
			result.Append(ref val);
			result.Append(ref PreviousNodePosition);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			EntityInfo source = default(EntityInfo);
			result = Unsafe.As<EntityInfo, TPossiblyBase>(ref source);
		}

		public void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<EntityInfo>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "Reference", ref Reference, state);
			formatter.Field(1, "PreviousNodePosition", ref PreviousNodePosition, state);
			formatter.EndObject();
		}

		public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EntityInfo>();
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
					Reference = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
					break;
				case 1:
					PreviousNodePosition = formatter.ReadPackable<Vector3>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	private bool m_OnUnit;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_UsePatternFromAbility;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_CreatedFromSceneObject;

	[JsonProperty]
	[OwlPackInclude]
	public bool ForceEnded;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_CanAffectAllies = true;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_InitialEntityScanDone;

	[JsonProperty]
	[OwlPackInclude]
	private List<EntityInfo> m_EntitiesInside = new List<EntityInfo>();

	private List<EntityRef<MechanicEntity>> m_EntitiesNotInside = new List<EntityRef<MechanicEntity>>();

	[JsonProperty]
	[OwlPackInclude]
	private TimeSpan? m_CasterDisappearTime;

	[JsonProperty]
	[OwlPackInclude]
	private float? m_ForceInitiative;

	[CanBeNull]
	private GridNodeBase m_TargetNode;

	private IScriptZoneShape m_Shape;

	private bool m_ForceUpdate;

	[CanBeNull]
	private AoEPattern m_OverridePattern;

	private bool m_PatternDirty;

	private readonly List<MechanicEntity> m_EntitiesEnteredDuringUpdate = new List<MechanicEntity>(16);

	private readonly List<MechanicEntity> m_EntitiesExitedDuringUpdate = new List<MechanicEntity>(16);

	private readonly Predicate<EntityRef<MechanicEntity>> m_IsMovedFromOutsideToTheVoidDelegate;

	private readonly Predicate<EntityInfo> m_IsMovedFromInsideToTheVoidDelegate;

	private readonly Predicate<EntityInfo> m_IsMovedFromInsideToOutsideDelegate;

	private readonly Predicate<EntityRef<MechanicEntity>> m_IsMovedFromOutsideToInsideDelegate;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "AreaEffectEntity",
		OldNames = null,
		Fields = new FieldInfo[30]
		{
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_IsInGame", typeof(bool)),
			new FieldInfo("m_Position", typeof(Vector3)),
			new FieldInfo("m_Orientation", typeof(float)),
			new FieldInfo("m_InitialPosition", typeof(Vector3?)),
			new FieldInfo("m_InitialOrientation", typeof(float?)),
			new FieldInfo("Facts", typeof(EntityFactsManager)),
			new FieldInfo("Parts", typeof(EntityPartsManager)),
			new FieldInfo("m_IsRevealed", typeof(bool)),
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("m_Context", typeof(MechanicsContext)),
			new FieldInfo("m_Target", typeof(TargetWrapper)),
			new FieldInfo("m_CreationTime", typeof(TimeSpan)),
			new FieldInfo("m_OnUnit", typeof(bool)),
			new FieldInfo("m_UsePatternFromAbility", typeof(bool)),
			new FieldInfo("m_CreatedFromSceneObject", typeof(bool)),
			new FieldInfo("ForceEnded", typeof(bool)),
			new FieldInfo("m_CanAffectAllies", typeof(bool)),
			new FieldInfo("m_InitialEntityScanDone", typeof(bool)),
			new FieldInfo("m_EntitiesInside", typeof(List<EntityInfo>)),
			new FieldInfo("m_CasterDisappearTime", typeof(TimeSpan?)),
			new FieldInfo("m_ForceInitiative", typeof(float?)),
			new FieldInfo("Duration", typeof(Rounds?)),
			new FieldInfo("Lifetime", typeof(Rounds)),
			new FieldInfo("SourceFact", typeof(EntityFactRef)),
			new FieldInfo("RetainCameraOnEnd", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private MechanicsContext m_Context { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	private TargetWrapper m_Target { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	private TimeSpan m_CreationTime { get; set; }

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public Rounds? Duration { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public Rounds Lifetime { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public EntityFactRef SourceFact { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool RetainCameraOnEnd { get; set; }

	public EntityEventDelegate OnEntityEnter { get; set; }

	public EntityEventDelegate OnEntityExit { get; set; }

	public EntityEventDelegate OnEntityMove { get; set; }

	public EntityEventDelegate OnEntityStartTurn { get; set; }

	public EntityEventDelegate OnEntityEndTurn { get; set; }

	public bool IsInInitialEntityScan { get; private set; }

	public IScriptZoneShape Shape
	{
		get
		{
			IScriptZoneShape shape;
			if (!m_CreatedFromSceneObject)
			{
				shape = m_Shape;
				if (shape == null)
				{
					IScriptZoneShape scriptZoneShape2;
					IScriptZoneShape scriptZoneShape;
					if (!base.Blueprint.IsAllArea)
					{
						scriptZoneShape = new AreaEffectShapePattern();
						scriptZoneShape2 = scriptZoneShape;
					}
					else
					{
						scriptZoneShape = new AreaEffectShapeAllArea();
						scriptZoneShape2 = scriptZoneShape;
					}
					scriptZoneShape = scriptZoneShape2;
					m_Shape = scriptZoneShape2;
					return scriptZoneShape;
				}
			}
			else
			{
				shape = m_Shape;
			}
			return shape;
		}
	}

	public string ViewName => Config?.ViewName ?? "AreaEffectView";

	public bool IsAllArea => base.Blueprint.IsAllArea;

	[CanBeNull]
	public new IAreaEffectView View => (IAreaEffectView)base.View;

	public new IAreaEffectConfig Config => (IAreaEffectConfig)base.Config;

	public NodeList CoveredNodes => Shape.CoveredNodes;

	public IEnumerable<MechanicEntity> InGameEntitiesInside => from i in m_EntitiesInside
		select i.Reference.Entity into i
		where i != null && i.IsInGame && i.IsInState
		select i;

	public bool IsEnded
	{
		get
		{
			if (!ForceEnded)
			{
				Rounds lifetime = Lifetime;
				Rounds? duration = Duration;
				return lifetime >= duration;
			}
			return true;
		}
	}

	public MechanicsContext Context => m_Context;

	public bool AffectEnemies => base.Blueprint.AffectEnemies;

	public bool AggroEnemies => base.Blueprint.AggroEnemies;

	public override bool IsSuppressible => true;

	private bool IsInTurnBasedMode => Game.Instance.Controllers.TurnController.TurnBasedModeActive;

	[CanBeNull]
	public BlueprintAbilityFXSettings FXSettings => base.Blueprint.FXSettings;

	bool ICombatParticipant.Active => m_ForceInitiative.HasValue;

	public override bool IsInCombat => Game.Instance.Controllers.TurnController.InCombat;

	public override bool NeedsView => !base.Blueprint.DontNeedView;

	public override bool SetTransformFromConfigOnLoad => m_CreatedFromSceneObject;

	public IEntityWithinBoundsHandler EntityHandler
	{
		get
		{
			if (!base.Blueprint.IsAllArea)
			{
				return GetRequired<AreaEffectBoundsPart>();
			}
			return GetRequired<AreaEffectUnboundPart>();
		}
	}

	public MechanicEntity CameraHolder => m_Context.MaybeCaster;

	public float TimeToFocus => 1f;

	public NodeList GetPatternCoveredNodes()
	{
		if (!base.Blueprint.SavePersistentArea)
		{
			return NodeList.Empty;
		}
		return Shape.CoveredNodes;
	}

	public bool IsEntityOutside(MechanicEntity entity)
	{
		return m_EntitiesNotInside.Contains(entity);
	}

	public AreaEffectEntity([NotNull] IAreaEffectConfig config, [CanBeNull] IEvalContext parentContext, [NotNull] BlueprintAreaEffect blueprint, [NotNull] TargetWrapper target, TimeSpan creationTime, TimeSpan? duration, bool onUnit, bool usePatternFromAbility = false, float? forceInitiative = null)
		: this(config.EntityId, config.IsInGameBySettings, parentContext, blueprint, target, creationTime, duration, onUnit, usePatternFromAbility, forceInitiative)
	{
		m_CreatedFromSceneObject = !config.CreatedAtRuntime;
		if (m_CreatedFromSceneObject)
		{
			m_Shape = config.Shape;
			m_InitialPosition = (m_Position = config.Position);
			m_InitialOrientation = (m_Orientation = config.Orientation);
		}
	}

	public AreaEffectEntity(string uniqueId, bool isInGame, [CanBeNull] IEvalContext parentContext, [NotNull] BlueprintAreaEffect blueprint, [NotNull] TargetWrapper target, TimeSpan creationTime, TimeSpan? duration, bool onUnit, bool usePatternFromAbility = false, float? forceInitiative = null)
		: base(uniqueId, isInGame, blueprint)
	{
		if (onUnit && target.Entity == null)
		{
			throw new Exception("Area effect is attached to unit, but unit is missing");
		}
		m_Context = MechanicsContext.Claim(blueprint, parentContext?.Caster, this, parentContext, target);
		m_Target = target;
		m_TargetNode = target.NearestNode;
		m_CreationTime = creationTime;
		Duration = duration?.ToRounds();
		m_InitialOrientation = (m_Orientation = m_Target.Orientation);
		m_OnUnit = onUnit;
		m_UsePatternFromAbility = usePatternFromAbility;
		m_Context.Recalculate();
		m_IsMovedFromOutsideToTheVoidDelegate = IsMovedFromOutsideToTheVoid;
		m_IsMovedFromInsideToTheVoidDelegate = IsMovedFromInsideToTheVoid;
		m_IsMovedFromInsideToOutsideDelegate = IsMovedFromInsideToOutside;
		m_IsMovedFromOutsideToInsideDelegate = IsMovedFromOutsideToInside;
		m_ForceInitiative = forceInitiative;
		m_ForceUpdate = true;
	}

	[UsedImplicitly]
	protected AreaEffectEntity(OwlPackConstructorParameter _)
		: base(_)
	{
		m_IsMovedFromOutsideToTheVoidDelegate = IsMovedFromOutsideToTheVoid;
		m_IsMovedFromInsideToTheVoidDelegate = IsMovedFromInsideToTheVoid;
		m_IsMovedFromInsideToOutsideDelegate = IsMovedFromInsideToOutside;
		m_IsMovedFromOutsideToInsideDelegate = IsMovedFromOutsideToInside;
		m_ForceUpdate = true;
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		if (!base.Blueprint.IsAllArea)
		{
			GetOrCreate<AreaEffectBoundsPart>();
			Remove<AreaEffectUnboundPart>();
		}
		else
		{
			GetOrCreate<AreaEffectUnboundPart>();
			Remove<AreaEffectBoundsPart>();
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		m_Context.Recalculate();
		base.Blueprint.HandleSpawn(m_Context, this);
		UpdateTransform(base.Blueprint);
		UpdateCombatInitiative();
	}

	protected override IEntityView CreateViewForData()
	{
		if (base.Blueprint.DontNeedView)
		{
			return null;
		}
		if (m_Target == null || (!m_OnUnit && m_Target.NearestNode == null))
		{
			PFLog.Default.Error($"Target for AreaEffectEntity {base.Blueprint} is null or empty! Skipping View create");
			return null;
		}
		AreaEffectView areaEffectView = new GameObject($"AreaEffect [{base.Blueprint}]").AddComponent<AreaEffectView>();
		areaEffectView.OnUnit = m_OnUnit;
		areaEffectView.InitAtRuntime(m_Context, base.Blueprint, m_Target, m_CreationTime, Duration?.Seconds, m_UsePatternFromAbility);
		return areaEffectView;
	}

	protected override void OnSetConfig(IEntityConfig config)
	{
		base.OnSetConfig(config);
		if (m_CreatedFromSceneObject)
		{
			m_Shape = Config.Shape;
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (!SourceFact.IsEmpty && SourceFact.Fact == null)
		{
			PFLog.Default.ErrorWithReport($"SourceFact of area effect is missing: {base.Blueprint}");
			ForceEnd();
		}
		m_TargetNode = m_Target.NearestNode;
	}

	protected override void OnDispose()
	{
		base.EventBus.RaiseEvent((IAreaEffectEntity)this, (Action<IAreaEffectHandler>)delegate(IAreaEffectHandler h)
		{
			h.HandleAreaEffectDestroyed();
		}, isCheckRuntime: true);
		m_Context.Dispose();
		base.OnDispose();
	}

	public void UpdateCombatInitiative()
	{
		TurnController turnController = Game.Instance.Controllers.TurnController;
		if (!turnController.InCombat)
		{
			base.Initiative.Value = 0f;
			base.Initiative.Order = 0;
		}
		else if (m_ForceInitiative.HasValue)
		{
			base.Initiative.Value = m_ForceInitiative.Value;
			base.Initiative.Order = 0;
		}
		else
		{
			MechanicEntity maybeCaster = Context.MaybeCaster;
			Initiative initiative = ((maybeCaster != null && maybeCaster.IsInCombat) ? maybeCaster : null)?.Initiative;
			base.Initiative.Value = initiative?.Value ?? 0f;
			base.Initiative.Order = initiative?.Order ?? 0;
		}
		if (turnController.InCombat)
		{
			MechanicEntity currentUnit = turnController.CurrentUnit;
			if (currentUnit != null && base.Initiative.TurnOrderPriority >= currentUnit.Initiative.TurnOrderPriority)
			{
				base.Initiative.LastTurn = turnController.GameRound;
			}
		}
	}

	public void Tick()
	{
		using (ProfileScope.New("EndIfNecessary"))
		{
			EndEffectIfNecessary();
		}
		if (!IsEnded)
		{
			Update();
			using (ProfileScope.New("HandleTick"))
			{
				base.Blueprint.HandleTick(m_Context, this);
			}
		}
		if (IsEnded)
		{
			HandleEnd();
		}
	}

	public void Update()
	{
		Bounds bounds = Shape.GetBounds();
		if (!base.Blueprint.IsAllArea)
		{
			UpdateTransform(base.Blueprint);
			View?.SyncTransform();
			GetRequired<AreaEffectBoundsPart>().Tick();
		}
		Bounds bounds2 = Shape.GetBounds();
		bool isInCombat = Game.Instance.Player.IsInCombat;
		bool flag = bounds2 != bounds;
		if (!m_InitialEntityScanDone)
		{
			DoInitialEntityScan();
		}
		if (!isInCombat || flag || m_ForceUpdate)
		{
			m_ForceUpdate = false;
			try
			{
				UpdateEntities();
			}
			finally
			{
				m_EntitiesEnteredDuringUpdate.Clear();
				m_EntitiesExitedDuringUpdate.Clear();
			}
		}
	}

	public void NextRound()
	{
		using (EvalContext.PushContext(Context))
		{
			Lifetime += 1.Rounds();
			HandleNewRound();
			if (IsEnded)
			{
				HandleEnd();
			}
			m_ForceUpdate = true;
		}
	}

	private void HandleEnd()
	{
		using (EvalContext.PushContext(Context))
		{
			base.Blueprint.HandleEnd(EvalContext.Current, this);
			foreach (EntityInfo item in m_EntitiesInside)
			{
				HandleEntityExit(item.Reference);
			}
			if (RetainCameraOnEnd)
			{
				((ICameraFocusTarget)this).RetainCamera();
			}
			m_EntitiesInside.Clear();
			m_EntitiesNotInside.Clear();
			Game.Instance.Controllers.EntityDestroyer.Destroy(this);
		}
	}

	private void EndEffectIfNecessary()
	{
		if (Context?.MaybeCaster == null || (m_OnUnit && m_Target.Entity == null))
		{
			ForceEnd();
			return;
		}
		MechanicsContext context = Context;
		bool? obj;
		if (context == null)
		{
			obj = null;
		}
		else
		{
			MechanicEntity maybeCaster = context.MaybeCaster;
			obj = ((maybeCaster != null) ? new bool?(!maybeCaster.IsInState) : null);
		}
		bool? flag = obj;
		if (flag.GetValueOrDefault() && m_OnUnit)
		{
			MechanicEntity entity = m_Target.Entity;
			if (entity != null && !entity.IsInState && base.IsInState)
			{
				ForceEnd();
				return;
			}
		}
		if (m_Context.SourceAbilityBlueprint != null && (m_Context.MaybeCaster == null || m_Context.MaybeCaster.IsDead) && !Game.Instance.Player.IsInCombat)
		{
			TimeSpan timeSpan = ConfigRoot.Instance.SystemMechanics.AreaEffectAutoDestroySeconds.Seconds();
			TimeSpan gameTime = Game.Instance.Controllers.TimeController.GameTime;
			if (!m_CasterDisappearTime.HasValue)
			{
				m_CasterDisappearTime = gameTime;
			}
			else if (gameTime - m_CasterDisappearTime.Value >= timeSpan)
			{
				ForceEnd();
			}
		}
	}

	private bool IsMovedFromOutsideToTheVoid(EntityRef<MechanicEntity> entityRef)
	{
		return EntityHandler.Exited.Contains(entityRef);
	}

	private bool IsMovedFromInsideToTheVoid(EntityInfo entityInfo)
	{
		if (!EntityHandler.Exited.Contains(entityInfo.Reference))
		{
			return false;
		}
		m_EntitiesExitedDuringUpdate.Add(entityInfo.Reference);
		return true;
	}

	private bool IsMovedFromInsideToOutside(EntityInfo entityInfo)
	{
		MechanicEntity entity = entityInfo.Reference.Entity;
		if (entity == null || entity.IsDisposed)
		{
			return true;
		}
		if (!IsEnded && ShouldEntityBeInside(entity))
		{
			return false;
		}
		if (m_EntitiesNotInside.HasItem((EntityRef<MechanicEntity> i) => i == entity))
		{
			return true;
		}
		m_EntitiesNotInside.Add(entity);
		m_EntitiesExitedDuringUpdate.Add(entityInfo.Reference);
		return true;
	}

	private bool IsMovedFromOutsideToInside(EntityRef<MechanicEntity> entityRef)
	{
		if (IsEnded)
		{
			return false;
		}
		MechanicEntity entity = entityRef.Entity;
		if (entity == null || entity.IsDisposed)
		{
			return false;
		}
		if (m_EntitiesInside.HasItem((EntityInfo i) => i.Reference == entityRef))
		{
			return true;
		}
		if (!ShouldEntityBeInside(entity))
		{
			return false;
		}
		m_EntitiesInside.Add(new EntityInfo(entityRef)
		{
			PreviousNodePosition = entity.CurrentUnwalkableNode.Vector3Position()
		});
		m_EntitiesEnteredDuringUpdate.Add(entityRef);
		return true;
	}

	private void HandleEntityEnter(MechanicEntity entity)
	{
		using (ProfileScope.NewScope("HandleEntityEnter"))
		{
			if (IsEntityInAnotherAreaOfCluster(entity))
			{
				return;
			}
			using (EvalContext.PushContext(Context, entity))
			{
				base.Blueprint.HandleEntityEnter(EvalContext.Current, this, entity);
				base.EventBus.RaiseEvent((IAreaEffectEntity)this, (Action<IAreaEffectEntityHandler>)delegate(IAreaEffectEntityHandler h)
				{
					h.HandleEntityEnterAreaEffect(entity);
				}, isCheckRuntime: true);
				OnEntityEnter?.Invoke(entity);
			}
		}
	}

	private void HandleEntityExit(MechanicEntity entity)
	{
		using (ProfileScope.NewScope("HandleEntityExit"))
		{
			if (IsEntityInAnotherAreaOfCluster(entity))
			{
				return;
			}
			using (EvalContext.PushContext(Context, entity))
			{
				base.Blueprint.HandleEntityExit(EvalContext.Current, this, entity);
				base.EventBus.RaiseEvent((IAreaEffectEntity)this, (Action<IAreaEffectEntityHandler>)delegate(IAreaEffectEntityHandler h)
				{
					h.HandleEntityExitAreaEffect(entity);
				}, isCheckRuntime: true);
				OnEntityExit?.Invoke(entity);
			}
		}
	}

	private void HandleEntityMoveInside(MechanicEntity entity)
	{
		using (ProfileScope.NewScope("HandleEntityMoveInside"))
		{
			using (EvalContext.PushContext(Context, entity))
			{
				base.Blueprint.HandleEntityMove(EvalContext.Current, this, entity);
				OnEntityMove?.Invoke(entity);
			}
		}
	}

	private void HandleNewRound()
	{
		using (ProfileScope.NewScope("HandleNewRound"))
		{
			using (EvalContext.PushContext(Context))
			{
				base.Blueprint.HandleRound(EvalContext.Current, this);
			}
		}
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		using (ProfileScope.NewScope("HandleUnitStartTurn"))
		{
			MechanicEntity currentEntity = EventInvokerExtensions.MechanicEntity;
			if (m_EntitiesInside.Any((EntityInfo x) => x.Reference.Entity == currentEntity))
			{
				using (EvalContext.PushContext(Context, currentEntity))
				{
					base.Blueprint.HandleUnitStartTurn(EvalContext.Current, this, currentEntity);
					OnEntityStartTurn?.Invoke(currentEntity);
					return;
				}
			}
		}
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		using (ProfileScope.NewScope("HandleUnitEndTurn"))
		{
			MechanicEntity currentEntity = EventInvokerExtensions.MechanicEntity;
			if (m_EntitiesInside.Any((EntityInfo x) => x.Reference.Entity == currentEntity))
			{
				using (EvalContext.PushContext(Context, currentEntity))
				{
					base.Blueprint.HandleUnitEndTurn(EvalContext.Current, this, currentEntity);
					OnEntityEndTurn?.Invoke(currentEntity);
					return;
				}
			}
		}
	}

	private bool IsEntityInAnotherAreaOfCluster(MechanicEntity entity)
	{
		AreaEffectClusterComponent clusterComponent = base.Blueprint.ClusterComponent;
		if (clusterComponent != null)
		{
			return entity.IsCurrentlyInAnotherClusterArea(clusterComponent.ClusterLogicBlueprint, this);
		}
		return false;
	}

	private void DoInitialEntityScan()
	{
		IsInInitialEntityScan = true;
		try
		{
			using (EvalContext.PushContext(Context))
			{
				foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
				{
					if (ShouldEntityBeInside(allBaseUnit))
					{
						m_EntitiesInside.Add(new EntityInfo(allBaseUnit)
						{
							PreviousNodePosition = allBaseUnit.CurrentUnwalkableNode.Vector3Position()
						});
						HandleEntityEnter(allBaseUnit);
					}
				}
				if (!base.Blueprint.AffectDestructibleObjects)
				{
					return;
				}
				foreach (DestructibleEntity destructibleEntity in Game.Instance.EntityPools.DestructibleEntities)
				{
					if (ShouldEntityBeInside(destructibleEntity))
					{
						m_EntitiesInside.Add(new EntityInfo(destructibleEntity)
						{
							PreviousNodePosition = destructibleEntity.CurrentUnwalkableNode.Vector3Position()
						});
						HandleEntityEnter(destructibleEntity);
					}
				}
			}
		}
		finally
		{
			IsInInitialEntityScan = false;
			m_EntitiesEnteredDuringUpdate.Clear();
			m_EntitiesExitedDuringUpdate.Clear();
			m_InitialEntityScanDone = true;
		}
	}

	private void UpdateEntities()
	{
		using (ProfileScope.NewScope("UpdateEntities"))
		{
			m_EntitiesNotInside.RemoveAll(m_IsMovedFromOutsideToTheVoidDelegate);
			m_EntitiesInside.RemoveAll(m_IsMovedFromInsideToTheVoidDelegate);
			int count = m_EntitiesNotInside.Count;
			foreach (EntityRef<MechanicEntity> item in EntityHandler.Entered)
			{
				m_EntitiesNotInside.Add(item);
			}
			m_EntitiesNotInside.Sort(count, m_EntitiesNotInside.Count - count, EntityRef<MechanicEntity>.Comparer.Instance);
			m_EntitiesInside.RemoveAll(m_IsMovedFromInsideToOutsideDelegate);
			m_EntitiesNotInside.RemoveAll(m_IsMovedFromOutsideToInsideDelegate);
			EntityHandler.ClearDelta();
			foreach (MechanicEntity item2 in m_EntitiesExitedDuringUpdate)
			{
				HandleEntityExit(item2);
			}
			foreach (MechanicEntity item3 in m_EntitiesEnteredDuringUpdate)
			{
				HandleEntityEnter(item3);
			}
			for (int i = 0; i < m_EntitiesInside.Count; i++)
			{
				EntityInfo entityInfo = m_EntitiesInside[i];
				MechanicEntity entity = entityInfo.Reference.Entity;
				PartMovable optional = entity.GetOptional<PartMovable>();
				if (optional != null && optional.HasMotionThisSimulationTick)
				{
					Vector3 vector = entity.CurrentUnwalkableNode.Vector3Position();
					if ((vector - entityInfo.PreviousNodePosition).sqrMagnitude > 1f && (vector - entity.Position).sqrMagnitude < 0.1f)
					{
						HandleEntityMoveInside(entity);
						m_EntitiesInside[i] = new EntityInfo(entity)
						{
							PreviousNodePosition = vector
						};
					}
				}
			}
		}
	}

	private bool ShouldEntityBeInside(MechanicEntity entity)
	{
		using (ProfileScope.NewScope("ShouldEntityBeInside"))
		{
			if (!entity.IsInGame)
			{
				return false;
			}
			if (!(entity is BaseUnitEntity) && (!base.Blueprint.AffectDestructibleObjects || !(entity is DestructibleEntity)))
			{
				return false;
			}
			if (entity.IsInFogOfWar && !entity.IsInCombat && !(entity is BaseUnitEntity { AwakeTimer: >0f }))
			{
				return false;
			}
			if (!entity.IsConscious && !base.Blueprint.AffectDead)
			{
				return false;
			}
			if (!IsSuitableTargetType(entity))
			{
				return false;
			}
			if (AbstractUnitCommand.CommandTargetUntargetable(this, entity))
			{
				return false;
			}
			PartAreaEffectImmunity optional = entity.GetOptional<PartAreaEffectImmunity>();
			if (optional != null && optional.IsImmune(this))
			{
				return false;
			}
			return base.Blueprint.IsAllArea || (Contains(entity) && !LineOfSightGeometry.Instance.HasObstacle(base.Position, entity.Position));
		}
	}

	public bool IsSuitableTargetType(MechanicEntity entity)
	{
		MechanicEntity mechanicEntity = Context?.MaybeCaster;
		if (mechanicEntity == null)
		{
			return false;
		}
		bool flag = mechanicEntity.IsEnemy(entity) || entity.IsNeutral;
		if (!flag || !base.Blueprint.CanTargetEnemies)
		{
			if (!flag && base.Blueprint.CanTargetAllies)
			{
				return m_CanAffectAllies;
			}
			return false;
		}
		return true;
	}

	public bool Overlaps(IEnumerable<GridNodeBase> nodes)
	{
		foreach (GridNodeBase node in nodes)
		{
			if (node == null)
			{
				throw new Exception("AreaEffectEntity.Overlaps: Cannot check an empty node for overlap");
			}
			if (Contains(node))
			{
				return true;
			}
		}
		return false;
	}

	public DestructibleEntity[] GetAllDestructibleEntityInside()
	{
		List<DestructibleEntity> list = TempList.Get<DestructibleEntity>();
		foreach (DestructibleEntity destructibleEntity in Game.Instance.EntityPools.DestructibleEntities)
		{
			if (Contains(destructibleEntity.Position))
			{
				list.Add(destructibleEntity);
			}
		}
		return list.ToArray();
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		foreach (EntityInfo item in m_EntitiesInside)
		{
			EntityRef<MechanicEntity> reference = item.Reference;
			if (reference.Entity != null)
			{
				HandleEntityExit(item.Reference);
			}
		}
		m_EntitiesInside.Clear();
	}

	void IAreaHandler.OnAreaDidLoad()
	{
	}

	public void ForceEnd()
	{
		ForceEnded = true;
		base.EventBus.RaiseEvent((IAreaEffectEntity)this, (Action<IAreaEffectForceEndHandler>)delegate(IAreaEffectForceEndHandler h)
		{
			h.HandleAreaEffectForceEndRequested();
		}, isCheckRuntime: true);
	}

	public bool Contains(MechanicEntity entity)
	{
		return Shape.Contains(entity.Position, entity.SizeRect);
	}

	public bool Contains(Vector3 point)
	{
		return Shape.Contains(point);
	}

	public bool Contains(GridNodeBase node)
	{
		return Shape.Contains(node);
	}

	public int CountInside([NotNull] Func<MechanicEntity, bool> predicate)
	{
		int num = 0;
		foreach (EntityInfo item in m_EntitiesInside)
		{
			MechanicEntity entity = item.Reference.Entity;
			if (entity != null && predicate(entity))
			{
				num++;
			}
		}
		return num;
	}

	public bool ContainsInside([NotNull] Func<MechanicEntity, bool> predicate)
	{
		return CountInside(predicate) > 0;
	}

	public bool ContainsInside([CanBeNull] MechanicEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		foreach (EntityInfo item in m_EntitiesInside)
		{
			if (item.Reference.Entity == entity)
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnIsInGameChanged()
	{
		base.OnIsInGameChanged();
		m_ForceUpdate = true;
	}

	public void HandleEntityPositionChanged()
	{
		m_ForceUpdate = true;
	}

	public void UpdateTransform(BlueprintAreaEffect blueprint)
	{
		using (ProfileScope.NewScope("UpdateTransform"))
		{
			if (!Game.Instance.CurrentlyLoadedArea.IsNavmeshArea || m_Context == null || m_Target == null || !(Shape is AreaEffectShapePattern areaEffectShapePattern))
			{
				return;
			}
			GridNodeBase targetNode = m_TargetNode;
			if (m_OnUnit)
			{
				m_TargetNode = m_Target.NearestNode;
				base.Orientation = m_Target.Orientation;
			}
			if (areaEffectShapePattern.ApplicationNodeExists && targetNode == m_TargetNode && !m_PatternDirty)
			{
				return;
			}
			GridNodeBase nearestNodeXZUnwalkable = (m_Context.MaybeCaster?.Position ?? m_Target.Point).GetNearestNodeXZUnwalkable();
			OrientedPatternData pattern;
			GridNodeBase gridNodeBase;
			if (m_UsePatternFromAbility && m_OverridePattern == null)
			{
				AbilityData sourceAbility = Context.SourceAbility;
				if ((object)sourceAbility != null)
				{
					Vector3? sourceCastPosition = Context.SourceCastPosition;
					if (sourceCastPosition.HasValue)
					{
						Vector3 valueOrDefault = sourceCastPosition.GetValueOrDefault();
						if (sourceAbility.GetPatternSettings() != null)
						{
							pattern = sourceAbility.GetPattern(m_Target, valueOrDefault);
							gridNodeBase = pattern.ApplicationNode ?? m_TargetNode;
							goto IL_01e4;
						}
					}
				}
				throw new InvalidOperationException();
			}
			AoEPattern aoEPattern = m_OverridePattern ?? blueprint.Pattern ?? throw new InvalidOperationException();
			Size targetSize = Size.Medium;
			if (m_OnUnit && m_Target.Entity is AbstractUnitEntity abstractUnitEntity)
			{
				targetSize = abstractUnitEntity.Size;
			}
			object caster;
			if (m_OnUnit)
			{
				MechanicEntity entity = m_Target.Entity;
				if (entity != null)
				{
					caster = entity;
					goto IL_01a3;
				}
			}
			caster = this;
			goto IL_01a3;
			IL_01a3:
			pattern = AoEPatternHelper.GetOrientedPattern(null, (MechanicEntity)caster, aoEPattern, blueprint, nearestNodeXZUnwalkable, m_TargetNode, castOnSameLevel: false, aoEPattern.CanBeDirectional, coveredTargetsOnly: false, targetSize, out gridNodeBase);
			gridNodeBase = pattern.ApplicationNode ?? gridNodeBase;
			goto IL_01e4;
			IL_01e4:
			areaEffectShapePattern.SetPattern(gridNodeBase, gridNodeBase.Vector3Position().y, in pattern);
			base.Position = gridNodeBase.Vector3Position();
			base.EventBus.RaiseEvent((IAreaEffectEntity)this, (Action<IAreaEffectShapeUpdatedHandler>)delegate(IAreaEffectShapeUpdatedHandler h)
			{
				h.HandleAreaEffectShapeUpdated();
			}, isCheckRuntime: true);
			m_PatternDirty = false;
		}
	}

	public void OverridePattern(AoEPattern pattern)
	{
		m_OverridePattern = pattern;
		m_PatternDirty = true;
	}

	public void ForceUpdate()
	{
		m_ForceUpdate = true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicsContext>.GetHash128(m_Context);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<TargetWrapper>.GetHash128(m_Target);
		result.Append(ref val3);
		TimeSpan val4 = m_CreationTime;
		result.Append(ref val4);
		result.Append(ref m_OnUnit);
		result.Append(ref m_UsePatternFromAbility);
		result.Append(ref m_CreatedFromSceneObject);
		result.Append(ref ForceEnded);
		result.Append(ref m_CanAffectAllies);
		result.Append(ref m_InitialEntityScanDone);
		List<EntityInfo> entitiesInside = m_EntitiesInside;
		if (entitiesInside != null)
		{
			for (int i = 0; i < entitiesInside.Count; i++)
			{
				EntityInfo obj = entitiesInside[i];
				Hash128 val5 = StructHasher<EntityInfo>.GetHash128(ref obj);
				result.Append(ref val5);
			}
		}
		if (m_CasterDisappearTime.HasValue)
		{
			TimeSpan val6 = m_CasterDisappearTime.Value;
			result.Append(ref val6);
		}
		if (m_ForceInitiative.HasValue)
		{
			float val7 = m_ForceInitiative.Value;
			result.Append(ref val7);
		}
		if (Duration.HasValue)
		{
			Rounds val8 = Duration.Value;
			result.Append(ref val8);
		}
		Rounds val9 = Lifetime;
		result.Append(ref val9);
		EntityFactRef obj2 = SourceFact;
		Hash128 val10 = StructHasher<EntityFactRef>.GetHash128(ref obj2);
		result.Append(ref val10);
		bool val11 = RetainCameraOnEnd;
		result.Append(ref val11);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		AreaEffectEntity source = new AreaEffectEntity(default(OwlPackConstructorParameter));
		result = Unsafe.As<AreaEffectEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<AreaEffectEntity>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.UniqueId;
		formatter.StringField(0, "UniqueId", ref value, state);
		formatter.UnmanagedField(1, "m_IsInGame", ref m_IsInGame, state);
		formatter.Field(2, "m_Position", ref m_Position, state);
		formatter.UnmanagedField(3, "m_Orientation", ref m_Orientation, state);
		formatter.NullableField(4, "m_InitialPosition", ref m_InitialPosition, state);
		formatter.UnmanagedNullableField(5, "m_InitialOrientation", ref m_InitialOrientation, state);
		formatter.Field(6, "Facts", ref Facts, state);
		formatter.Field(7, "Parts", ref Parts, state);
		formatter.UnmanagedField(8, "m_IsRevealed", ref m_IsRevealed, state);
		formatter.EnumNullableField(9, "m_ViewHandlingOnDisposePolicyOverride", ref m_ViewHandlingOnDisposePolicyOverride, state);
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		MechanicsContext value3 = m_Context;
		formatter.Field(14, "m_Context", ref value3, state);
		TargetWrapper value4 = m_Target;
		formatter.Field(15, "m_Target", ref value4, state);
		TimeSpan value5 = m_CreationTime;
		formatter.Field(16, "m_CreationTime", ref value5, state);
		formatter.UnmanagedField(17, "m_OnUnit", ref m_OnUnit, state);
		formatter.UnmanagedField(18, "m_UsePatternFromAbility", ref m_UsePatternFromAbility, state);
		formatter.UnmanagedField(19, "m_CreatedFromSceneObject", ref m_CreatedFromSceneObject, state);
		formatter.UnmanagedField(20, "ForceEnded", ref ForceEnded, state);
		formatter.UnmanagedField(21, "m_CanAffectAllies", ref m_CanAffectAllies, state);
		formatter.UnmanagedField(22, "m_InitialEntityScanDone", ref m_InitialEntityScanDone, state);
		formatter.Field(23, "m_EntitiesInside", ref m_EntitiesInside, state);
		formatter.NullableField(24, "m_CasterDisappearTime", ref m_CasterDisappearTime, state);
		formatter.UnmanagedNullableField(25, "m_ForceInitiative", ref m_ForceInitiative, state);
		Rounds? value6 = Duration;
		formatter.NullableField(26, "Duration", ref value6, state);
		Rounds value7 = Lifetime;
		formatter.Field(27, "Lifetime", ref value7, state);
		EntityFactRef value8 = SourceFact;
		formatter.Field(28, "SourceFact", ref value8, state);
		bool value9 = RetainCameraOnEnd;
		formatter.UnmanagedField(29, "RetainCameraOnEnd", ref value9, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<AreaEffectEntity>();
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
				base.UniqueId = formatter.ReadString(state);
				break;
			case 1:
				m_IsInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				m_Position = formatter.ReadPackable<Vector3>(state);
				break;
			case 3:
				m_Orientation = formatter.ReadUnmanaged<float>(state);
				break;
			case 4:
				m_InitialPosition = formatter.ReadNullablePackable<Vector3>(state);
				break;
			case 5:
				m_InitialOrientation = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 6:
				Facts = formatter.ReadPackable<EntityFactsManager>(state);
				break;
			case 7:
				Parts = formatter.ReadPackable<EntityPartsManager>(state);
				break;
			case 8:
				m_IsRevealed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				m_ViewHandlingOnDisposePolicyOverride = formatter.ReadNullableEnum<ViewHandlingOnDisposePolicyType>(state);
				break;
			case 10:
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				m_Context = formatter.ReadPackable<MechanicsContext>(state);
				break;
			case 15:
				m_Target = formatter.ReadPackable<TargetWrapper>(state);
				break;
			case 16:
				m_CreationTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 17:
				m_OnUnit = formatter.ReadUnmanaged<bool>(state);
				break;
			case 18:
				m_UsePatternFromAbility = formatter.ReadUnmanaged<bool>(state);
				break;
			case 19:
				m_CreatedFromSceneObject = formatter.ReadUnmanaged<bool>(state);
				break;
			case 20:
				ForceEnded = formatter.ReadUnmanaged<bool>(state);
				break;
			case 21:
				m_CanAffectAllies = formatter.ReadUnmanaged<bool>(state);
				break;
			case 22:
				m_InitialEntityScanDone = formatter.ReadUnmanaged<bool>(state);
				break;
			case 23:
				m_EntitiesInside = formatter.ReadPackable<List<EntityInfo>>(state);
				break;
			case 24:
				m_CasterDisappearTime = formatter.ReadNullablePackable<TimeSpan>(state);
				break;
			case 25:
				m_ForceInitiative = formatter.ReadNullableUnmanaged<float>(state);
				break;
			case 26:
				Duration = formatter.ReadNullablePackable<Rounds>(state);
				break;
			case 27:
				Lifetime = formatter.ReadPackable<Rounds>(state);
				break;
			case 28:
				SourceFact = formatter.ReadPackable<EntityFactRef>(state);
				break;
			case 29:
				RetainCameraOnEnd = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
