using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Framework.Utility.UnityExtensions;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Code._Legacy.Components;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Code.View.Visual.Animation.AddOffset;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.ManualCoroutines;
using Kingmaker.View.Equipment;
using Kingmaker.Visual;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharactersRigidbody;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Kingmaker.Visual.Decals;
using Kingmaker.Visual.MaterialEffects;
using Kingmaker.Visual.MaterialEffects.BloodMask;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Updatables;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Highlighting;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using Pathfinding;
using UnityEngine;
using UnityEngine.AI;

namespace Kingmaker.View.Mechanics.Entities;

[Serializable]
[KnowledgeDatabaseID("140695237c9c40d0b269732622d8f9fc")]
public abstract class AbstractUnitEntityView : MechanicEntityView, IAreaHandler, ISubscriber, IResource, IDetectHover, IGameModeHandler, IEntitySubscriber, IPartyCombatHandler
{
	public class LateUpdateDriver : RegisteredObjectBase, ILateUpdatable
	{
		private readonly AbstractUnitEntityView m_Unit;

		public LateUpdateDriver(AbstractUnitEntityView unitEntityView)
		{
			m_Unit = unitEntityView;
		}

		void ILateUpdatable.DoLateUpdate()
		{
			m_Unit.DoLateUpdate();
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("UnitEntityView");

	public const string SoftColliderName = "[soft collider]";

	public const string CoreColliderName = "[core collider]";

	[SerializeField]
	private float m_SoftColliderHeight = 1.8f;

	[SerializeField]
	[InspectorReadOnly]
	private CapsuleCollider m_SoftCollider;

	[SerializeField]
	[InspectorReadOnly]
	private MeshCollider m_CoreCollider;

	[SerializeField]
	public SoftColliderPlaceholder SoftColliderPlaceholder;

	public bool ForbidRotation;

	public GameObject OverrideRotatablePart;

	[Space(5f)]
	public GameObject[] Footprints;

	private ParticlesSnapMap m_ParticleSnapMap;

	private UnitMultiHighlight m_Highlighter;

	private Transform m_CenterTorso;

	private UnitAnimationManager m_AnimatorManager;

	private bool m_RenderersAndCollidersAreUpdated;

	private UnitAsksManager m_Asks;

	private StandardMaterialController m_StandardMaterialController;

	private OccludedObjectHighlighter m_OccludedObjectHighlighter;

	private UnitDismembermentManager m_DismembermentManager;

	[UsedImplicitly]
	private LateUpdateDriver m_LateUpdateDriver;

	private readonly List<FxDecal> m_Decals = new List<FxDecal>();

	private TimeSpan m_StartGetUpTime;

	private bool m_WasHighlightedOnHover;

	[InspectorReadOnly]
	private Bounds? m_CachedLocalBounds;

	private string m_Race;

	private BloodyFaceController m_BloodyFaceController;

	private UnitMovementAgentBase m_AgentOverride;

	private bool m_DirectHover;

	[CanBeNull]
	public RigidbodyCreatureController RigidbodyController;

	private readonly CountingGuard m_MouseHoverHighlighting = new CountingGuard();

	public bool HasOverriddenRotatablePart => OverrideRotatablePart != null;

	[CanBeNull]
	public Character CharacterAvatar { get; set; }

	public Animator Animator { get; private set; }

	public UnitMovementAgentBase AgentASP { get; private set; }

	public bool IsProne { get; protected set; }

	public bool IsHighlighted { get; private set; }

	public BlueprintUnit Blueprint { get; set; }

	public UnitViewMechadendritesEquipment MechadendritesEquipment { get; protected set; }

	[CanBeNull]
	public CapsuleCollider SoftCollider => m_SoftCollider;

	[CanBeNull]
	public MeshCollider CoreCollider => m_CoreCollider;

	[UsedImplicitly]
	private bool HideColliderFieldsInInspector => GetType() == typeof(AbstractUnitEntityView);

	public new AbstractUnitEntity EntityData => (AbstractUnitEntity)base.EntityData;

	public new AbstractUnitEntity Data => (AbstractUnitEntity)base.Data;

	public virtual UnitMovementAgentBase MovementAgent
	{
		get
		{
			if (!(AgentOverride == null))
			{
				return AgentOverride;
			}
			return AgentASP;
		}
	}

	public UnitMovementAgentBase AgentOverride
	{
		get
		{
			return m_AgentOverride;
		}
		set
		{
			if (value == null)
			{
				UnityEngine.Object.DestroyImmediate(m_AgentOverride);
			}
			m_AgentOverride = value;
		}
	}

	public virtual bool KeepCollidersSetupAsIs => false;

	public virtual bool UseHorizontalSoftCollider => false;

	internal virtual float HorizontalSoftColliderRadius => 1f;

	public virtual float Corpulence => 0.3f;

	public virtual bool IsInAoePattern => false;

	public override UnitAsksManager Asks => m_Asks;

	public override ParticlesSnapMap ParticlesSnapMap
	{
		get
		{
			if (!(CharacterAvatar != null))
			{
				return m_ParticleSnapMap;
			}
			return ObjectExtensions.Or(CharacterAvatar.ParticlesSnapMap, null) ?? ObjectExtensions.Or(m_ParticleSnapMap, null);
		}
	}

	public Vector2 CameraOrientedBoundsSize
	{
		get
		{
			if (m_SoftCollider == null)
			{
				return Vector2.zero;
			}
			Bounds bounds = m_SoftCollider.bounds;
			float y = bounds.max.y - base.transform.position.y;
			return new Vector2(bounds.size.x, y);
		}
	}

	public Vector2 CameraOrientedCoreBoundsSize
	{
		get
		{
			if (m_CoreCollider == null)
			{
				return Vector2.zero;
			}
			Bounds bounds = m_CoreCollider.bounds;
			float y = bounds.max.y - base.transform.position.y;
			return new Vector2(bounds.size.x, y);
		}
	}

	public bool IsGetUp
	{
		get
		{
			bool flag = (bool)AnimationManager && AnimationManager.IsStandUp;
			if (TurnController.IsInTurnBasedCombat())
			{
				return flag;
			}
			if (!flag)
			{
				return Game.Instance.Controllers.TimeController.GameTime - m_StartGetUpTime < 0.5.Seconds();
			}
			return true;
		}
	}

	public float SoftColliderHeight => m_SoftColliderHeight;

	[CanBeNull]
	public UnitDismembermentManager DismembermentManager => m_DismembermentManager;

	[CanBeNull]
	public UnitAnimationManager AnimationManager
	{
		get
		{
			if (!(CharacterAvatar != null))
			{
				return m_AnimatorManager;
			}
			return CharacterAvatar.AnimationManager;
		}
	}

	[NotNull]
	public Transform CenterTorso
	{
		get
		{
			if (!m_CenterTorso)
			{
				return base.transform;
			}
			return m_CenterTorso;
		}
	}

	public bool IsCommandsPreventMovement => EntityData.Commands.PreventMovement;

	public Bounds RenderersBounds => m_LocalBounds;

	private Bounds m_LocalBounds
	{
		get
		{
			if (m_CachedLocalBounds.HasValue)
			{
				return m_CachedLocalBounds.Value;
			}
			m_CachedLocalBounds = GetLocalBoundsFromRenderers(base.gameObject);
			return m_CachedLocalBounds.Value;
		}
	}

	public ViewInterpolationHelper InterpolationHelper { get; private set; }

	public BlockMode BlockMode
	{
		get
		{
			AbstractUnitEntity data = Data;
			if (data == null || !data.Features.CanPassThroughUnits)
			{
				return BlockMode.AllExceptSelector;
			}
			return BlockMode.Ignore;
		}
	}

	public IKController IkController { get; protected set; }

	public bool BlowUpDismember { get; protected set; }

	public bool LimbsApartDismember { get; protected set; }

	public bool LimbsApartDismembermentRestricted { get; set; }

	public virtual List<ItemEntity> Mechadendrites => null;

	[CanBeNull]
	public PartAdditionalCombatObjectiveUnit AdditionalCombatObjective => EntityData.GetOptional<PartAdditionalCombatObjectiveUnit>();

	public bool MouseHoverHighlighting
	{
		get
		{
			return m_MouseHoverHighlighting;
		}
		set
		{
			if (SetValueSafe(m_MouseHoverHighlighting, value))
			{
				UpdateHighlightView();
				OnMouseHighlight(value);
			}
			static bool SetValueSafe(CountingGuard guard, bool b)
			{
				if (guard.Value || b)
				{
					return guard.SetValue(b);
				}
				return false;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.EnsureComponent<Highlighter>();
		m_Highlighter = base.gameObject.EnsureComponent<UnitMultiHighlight>();
		m_OccludedObjectHighlighter = base.gameObject.EnsureComponent<OccludedObjectHighlighter>();
		SetAgentASP();
		InterpolationHelper = new ViewInterpolationHelper(this);
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		Blueprint = EntityData.Blueprint;
		if ((bool)AgentASP)
		{
			if (!AgentASP.Unit)
			{
				AgentASP.Init(base.gameObject);
				EventBus.Subscribe(AgentASP);
			}
			else
			{
				AgentASP.ResetBlocker();
			}
		}
		UpdateAsks();
		Character componentInChildren = GetComponentInChildren<Character>();
		base.Fader = base.gameObject.EnsureComponent<EntityFader>();
		CharacterAvatar = SetupCharacterAvatar(componentInChildren);
		if (CharacterAvatar == null || !CharacterAvatar.ParticlesSnapMap)
		{
			m_ParticleSnapMap = GetComponentInChildren<ParticlesSnapMap>();
			if (m_ParticleSnapMap == null)
			{
				PFLog.Default.Error("EntityView " + base.name + " ParticlesSnapMap component is missing!");
			}
		}
		if (ParticlesSnapMap != null && EntityData.Blueprint != null && !ParticlesSnapMap.Initialized)
		{
			ParticlesSnapMap.Init();
			if (EntityData.Blueprint.Race != null)
			{
				ParticlesSnapMap.ParticleSizeScale *= ConfigRoot.Instance.FxRoot.RaceFxSnapMapScaleSettings.GetCoeff(EntityData.Blueprint.Race.RaceId);
			}
		}
		Animator = GetComponentInChildren<Animator>();
		if ((bool)Animator)
		{
			Animator.EnsureComponent<UnitAnimationCallbackReceiver>();
			if (!CharacterAvatar)
			{
				m_AnimatorManager = Animator.GetComponent<UnitAnimationManager>();
				if ((bool)m_AnimatorManager)
				{
					Animator.runtimeAnimatorController = null;
				}
			}
			Animator.enabled = true;
			Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
		}
		DeterminateRace();
		if (AnimationManager != null && EntityData != null)
		{
			OverrideAnimationRaceComponent component = Blueprint.GetComponent<OverrideAnimationRaceComponent>();
			if (component != null)
			{
				AnimationManager.AttachToView(this, component.BlueprintRace.Get());
			}
			else
			{
				PartUnitProgression optional = EntityData.GetOptional<PartUnitProgression>();
				AnimationManager.AttachToView(this, optional?.Race);
			}
			AnimationManager.FireEvents = true;
			EventBus.Subscribe(AnimationManager);
		}
		SpawnFxOnStart component2 = GetComponent<SpawnFxOnStart>();
		if ((bool)component2)
		{
			component2.SpawnFx();
		}
		base.Fader = base.gameObject.EnsureComponent<EntityFader>();
		SetupSoundImportance();
		GetComponentsInChildren<NavmeshCut>().ForEach(Utils.EditorSafeDestroy);
		m_CenterTorso = ObjectExtensions.Or(ParticlesSnapMap, null)?.GetLocatorFirst(FxRoot.Instance.LocatorGroupTorsoCenterFX)?.Transform;
		if (m_CenterTorso == null)
		{
			PFLog.Default.Warning("EntityView " + base.name + " Locator group " + FxRoot.Instance.LocatorGroupTorsoCenterFX.name + " is missing or empty! Using GO transform instead");
		}
		m_DismembermentManager = GetComponent<UnitDismembermentManager>();
		SetupSelectionColliders(forceRecreate: false);
		GetComponentsInChildren(m_Decals);
		SetOccluderColorAndState();
		if ((bool)AstarPath.active)
		{
			ForcePlaceAboveGround();
		}
		if (!EntityData.LifeState.IsConscious)
		{
			IsProne = true;
		}
		if (EntityData.LifeState.IsDead)
		{
			Game.Instance.Controllers.CoroutinesController.Start(SwitchCoreColliderToDeadState(), this);
		}
		if (Data.LifeState.IsFinallyDead && !Data.GetOptional<UnitPartCompanion>())
		{
			base.Fader.DisableAnimation();
		}
		IkController = GetComponentInChildren<IKController>();
		GetComponentInChildren<HumanoidRagdollManager>()?.InitHumanoidRagdoll();
		GetComponentInChildren<RigidbodyCreatureController>()?.InitRigidbodyCreatureController();
		if (EntityData.LifeState.IsConscious)
		{
			return;
		}
		if ((bool)RigidbodyController)
		{
			PartSavedRagdollState optional2 = EntityData.GetOptional<PartSavedRagdollState>();
			if (optional2 != null && optional2.Active)
			{
				optional2.RestoreRagdollState(RigidbodyController);
				return;
			}
		}
		if ((bool)DismembermentManager)
		{
			SavedDismembermentState optional3 = EntityData.GetOptional<SavedDismembermentState>();
			if (optional3 != null && optional3.Active)
			{
				optional3.RestoreDismembermentState(DismembermentManager);
				return;
			}
		}
		if (AnimationManager != null)
		{
			AnimationManager.IsProne = true;
			AnimationManager.IsDead = EntityData.LifeState.IsDead;
			AnimationManager.Tick(RealTimeController.SystemStepDurationSeconds);
			AnimationManager.FastForwardProneAnimation(AnimationManager.IsDead);
			AnimationManager.FastForwardDeathAnimation();
		}
	}

	private Character SetupCharacterAvatar([CanBeNull] Character character)
	{
		if (character == null)
		{
			return null;
		}
		character.Initialize();
		return character;
	}

	private void SetupSoundImportance()
	{
		AkUnitySoundEngine.SetRTPCValue("Importance", (EntityData.Blueprint.VisualSettings.ImportanceOverride > 0) ? EntityData.Blueprint.VisualSettings.ImportanceOverride : ((!EntityData.IsPlayerFaction) ? 1 : 3), base.gameObject);
	}

	protected override void OnWillDetachFromData()
	{
		base.OnWillDetachFromData();
		if (AnimationManager != null)
		{
			AnimationManager.FireEvents = false;
			EventBus.Unsubscribe(AnimationManager);
		}
		if (AgentASP != null)
		{
			EventBus.Unsubscribe(AgentASP);
		}
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		if (base.IsVisible)
		{
			if (EntityData.LifeState.IsDead)
			{
				EntityData.LifeState.IsDeathRevealed = EntityData.IsInCameraFrustum;
			}
			if ((bool)m_CoreCollider)
			{
				if (!EntityData.LifeState.IsConscious)
				{
					m_CoreCollider.transform.position = CenterTorso.position;
				}
				else
				{
					m_CoreCollider.transform.position = base.ViewTransform.position;
				}
			}
		}
		foreach (FxDecal decal in m_Decals)
		{
			if ((bool)decal)
			{
				decal.enabled = base.IsVisible;
			}
		}
		UpdateHighlight();
	}

	public void SetOccluderColorAndState()
	{
		m_OccludedObjectHighlighter.Color = ConfigRoot.Instance.FxRoot.OccluderColorDefault;
		bool flag = false;
		if (Data.IsInPlayerParty)
		{
			m_OccludedObjectHighlighter.Color = ConfigRoot.Instance.FxRoot.OccluderColorAlly;
			flag = true;
		}
		if (Data.IsPlayerEnemy)
		{
			m_OccludedObjectHighlighter.Color = ConfigRoot.Instance.FxRoot.OccluderColorEnemy;
			flag = EntityData.GetCombatStateOptional()?.IsInCombat ?? false;
		}
		if (!Data.IsPlayerEnemy && !Data.IsInPlayerParty)
		{
			m_OccludedObjectHighlighter.Color = ConfigRoot.Instance.FxRoot.OccluderColorUnknownCombatant;
			flag = EntityData.GetCombatStateOptional()?.IsInCombat ?? false;
		}
		if (Data.LifeState.State == UnitLifeState.Dead)
		{
			flag = false;
		}
		m_OccludedObjectHighlighter.enabled = flag;
	}

	public void SetupSelectionColliders(bool forceRecreate)
	{
		ConfigRoot instance = ConfigRoot.Instance;
		if (KeepCollidersSetupAsIs)
		{
			return;
		}
		if (!m_SoftCollider || !m_CoreCollider || forceRecreate)
		{
			Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.gameObject.layer == 10 || collider.name == "[soft collider]" || collider.name == "[core collider]")
				{
					Utils.EditorSafeDestroy(collider.gameObject);
				}
			}
			if ((bool)m_SoftCollider)
			{
				Utils.EditorSafeDestroy(m_SoftCollider.gameObject);
			}
			if ((bool)m_CoreCollider)
			{
				Utils.EditorSafeDestroy(m_CoreCollider.gameObject);
			}
			m_SoftCollider = new GameObject("[soft collider]").AddComponent<CapsuleCollider>();
			m_SoftCollider.transform.SetParent(base.ViewTransform, worldPositionStays: false);
			m_CoreCollider = new GameObject("[core collider]").AddComponent<MeshCollider>();
			m_CoreCollider.transform.SetParent(base.ViewTransform, worldPositionStays: false);
			m_CoreCollider.sharedMesh = instance.Prefabs.UnitCoreCollider;
		}
		m_SoftCollider.gameObject.tag = "SecondarySelection";
		m_SoftCollider.gameObject.layer = 10;
		m_CoreCollider.gameObject.layer = 10;
		m_SoftCollider.transform.position = CenterTorso.transform.position;
		m_SoftCollider.transform.rotation = CenterTorso.transform.rotation;
		m_SoftCollider.transform.localScale = Vector3.one;
		m_SoftCollider.height = m_SoftColliderHeight;
		if (UseHorizontalSoftCollider)
		{
			m_SoftCollider.transform.Rotate(90f, 0f, 0f, Space.Self);
			m_SoftCollider.radius = HorizontalSoftColliderRadius;
		}
		else
		{
			m_SoftCollider.radius = Corpulence * instance.Prefabs.SecondaryColliderWidthCoeff + 0.5f;
		}
		m_SoftCollider.center = Vector3.zero;
		Vector3 localScale = base.ViewTransform.localScale;
		m_CoreCollider.transform.localPosition = Vector3.zero;
		m_CoreCollider.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		m_CoreCollider.transform.localScale = ((Mathf.Abs(localScale.x * localScale.y * localScale.z) > 0.01f) ? new Vector3((Corpulence + 0.5f) * 1f / localScale.x, (Corpulence + 0.5f) * 1f / localScale.z, instance.Prefabs.CoreColliderHeight * 3.57f / localScale.y) : Vector3.one);
	}

	public virtual void MarkRenderersAndCollidersAreUpdated()
	{
		if (m_RenderersAndCollidersAreUpdated)
		{
			return;
		}
		RefreshHighlighters();
		if (m_StandardMaterialController != null || TryGetComponent<StandardMaterialController>(out m_StandardMaterialController))
		{
			m_StandardMaterialController.InvalidateRenderersAndMaterials();
			if (m_BloodyFaceController == null || m_BloodyFaceController.IsDisposed)
			{
				m_BloodyFaceController = new BloodyFaceController(EntityData, m_StandardMaterialController.BloodMaskController);
			}
			m_BloodyFaceController.UpdateBloodValues(force: true);
		}
		m_RenderersAndCollidersAreUpdated = true;
	}

	protected void RefreshHighlighters()
	{
		if ((bool)m_Highlighter)
		{
			m_Highlighter.Highlighter?.ReinitMaterials();
		}
		if ((bool)m_OccludedObjectHighlighter)
		{
			m_OccludedObjectHighlighter.InvalidateRenderers();
		}
	}

	public void UpdateAsks()
	{
		UnitAsksManager asks = Asks;
		UnitAsksManager unitAsksManager = null;
		try
		{
			m_Asks = null;
			BlueprintUnitAsksList list = EntityData.Asks.List;
			if (list != null)
			{
				unitAsksManager = (m_Asks = new UnitAsksManager(EntityData, list));
			}
		}
		finally
		{
			unitAsksManager?.LoadBanks();
			asks?.UnloadBanks();
		}
	}

	public void DeterminateRace()
	{
		if (EntityData != null && EntityData.AnimationManager != null && EntityData.AnimationManager.AnimationSet != null)
		{
			string text = EntityData.AnimationManager.AnimationSet.name;
			if (text.Contains("human", StringComparison.OrdinalIgnoreCase))
			{
				m_Race = "human";
			}
			else if (text.Contains("eldar", StringComparison.OrdinalIgnoreCase))
			{
				m_Race = "eldar";
			}
			else if (text.Contains("spaceMarine", StringComparison.OrdinalIgnoreCase))
			{
				m_Race = "spaceMarine";
			}
			else
			{
				m_Race = "empty";
			}
			string race = m_Race;
			if (!(race == "human") && !(race == "empty"))
			{
				base.gameObject.AddComponent<AddOffset>();
			}
		}
	}

	private Bounds GetLocalBoundsFromRenderers(GameObject g)
	{
		Bounds result = new Bounds(Vector3.zero, Vector3.zero);
		foreach (Renderer renderer in Renderers)
		{
			if (renderer is SkinnedMeshRenderer && renderer.gameObject.activeInHierarchy)
			{
				result.Encapsulate(renderer.localBounds);
			}
		}
		return result;
	}

	private void SetAgentASP()
	{
		base.gameObject.EnsureComponent<UnitMovementAgent>();
		NavMeshAgent component = GetComponent<NavMeshAgent>();
		if ((bool)component)
		{
			PFLog.Default.Warning("NavMesh agent should not be used anymore", this);
			UnityEngine.Object.Destroy(component);
		}
		UnitMovementAgent[] components = GetComponents<UnitMovementAgent>();
		if (components.Length > 1)
		{
			PFLog.Default.Warning("More than one UnitMovementAgent on one unit", this);
			for (int i = 1; i < components.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(components[i]);
			}
		}
		AgentASP = GetComponent<UnitMovementAgent>();
	}

	public void ForcePlaceAboveGround()
	{
		if (!EntityData.Features.ControlledByDirector && Game.Instance.CurrentlyLoadedArea != null && Game.Instance.CurrentlyLoadedArea.IsNavmeshArea && !(Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem))
		{
			EntityData.Position = ObstacleAnalyzer.GetNearestNode(EntityData.Position, null, ObstacleAnalyzer.UnwalkableXZConstraint).position;
			base.ViewTransform.position = GetViewPositionOnGround(EntityData.Position);
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		ForcePeacefulLook(peaceful: false);
		ResetMouseHighlighted();
	}

	public void HandleHoverChange(bool isHover)
	{
		m_DirectHover = isHover;
		MouseHoverHighlighting = isHover;
		if (isHover)
		{
			m_WasHighlightedOnHover = true;
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.Default)
		{
			ResetMouseHighlighted();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		UpdateHighlight();
	}

	public IEntity GetSubscribingEntity()
	{
		return EntityData;
	}

	public virtual void ForcePeacefulLook(bool peaceful)
	{
		if (CharacterAvatar != null)
		{
			CharacterAvatar.DisplayOptions.IsPeacefulMode = peaceful;
		}
	}

	public void ResetMouseHighlighted()
	{
		MassLootHelper.Clear();
		if (MouseHoverHighlighting)
		{
			EventBus.RaiseEvent(delegate(IUnitDirectHoverUIHandler h)
			{
				h.HandleHoverChange(this, isHover: false, isDirect: false);
			});
		}
		ResetHighlightGuard();
		UpdateHighlight();
		if (!TurnController.IsInTurnBasedCombat())
		{
			Game.Instance.CursorController.SetUnitCursor(EntityData, isHighlighted: false);
		}
	}

	public void UpdateHighlight(bool raiseEvent = true)
	{
		if (EntityData == null)
		{
			return;
		}
		bool isHighlighted = IsHighlighted;
		IsHighlighted = false;
		bool isDead = EntityData.LifeState.IsDead;
		bool flag = TurnController.IsInTurnBasedCombat();
		InteractionHighlightController interactionHighlightController;
		BaseUnitEntity baseUnitEntity2;
		if (!isDead || (EntityData.IsDeadAndHasLoot && !flag) || (Game.Instance.Player.UISettings.ShowInspect && !flag) || Game.Instance.Controllers.SelectedAbilityHandler?.Ability?.Blueprint.GetComponent<ICanTargetDeadUnits>() != null)
		{
			interactionHighlightController = Game.Instance.Controllers.InteractionHighlightController;
			TurnController turnController = Game.Instance.Controllers.TurnController;
			if (flag && turnController != null)
			{
				MechanicEntity currentUnit = turnController.CurrentUnit;
				if (currentUnit is BaseUnitEntity baseUnitEntity && currentUnit.IsPlayerFaction)
				{
					baseUnitEntity2 = baseUnitEntity;
					goto IL_00e5;
				}
			}
			baseUnitEntity2 = Game.Instance.Player.MainCharacterEntity;
			goto IL_00e5;
		}
		goto IL_01eb;
		IL_00e5:
		BaseUnitEntity initiator = baseUnitEntity2;
		IUnitInteraction unitInteraction = EntityData.SelectClickInteraction(initiator);
		bool flag2 = AdditionalCombatObjective != null && (AdditionalCombatObjective.HighlightType == HighlightType.Always || (AdditionalCombatObjective.HighlightType == HighlightType.Once && !m_WasHighlightedOnHover));
		bool flag3 = flag && AdditionalCombatObjective != null && (AdditionalCombatObjective.HighlightType != HighlightType.Once || !m_WasHighlightedOnHover);
		bool flag4 = (interactionHighlightController != null && interactionHighlightController.IsGlobalHighlighting) || MouseHoverHighlighting || flag2 || IsInAoePattern || flag3 || (flag && (unitInteraction?.AllowInCombat ?? false));
		IsHighlighted = interactionHighlightController != null && flag4 && EntityData.IsVisibleForPlayer && (EntityData.IsPlayerFaction || EntityData.IsPlayerEnemy || (AdditionalCombatObjective != null && flag) || isDead || unitInteraction != null);
		goto IL_01eb;
		IL_01eb:
		m_Highlighter.BaseColor = GetHighlightColor();
		if (raiseEvent && isHighlighted != IsHighlighted)
		{
			EventBus.RaiseEvent(delegate(IUnitHighlightUIHandler h)
			{
				h.HandleHighlightChange(this);
			});
		}
	}

	private Color GetHighlightColor()
	{
		if (!IsHighlighted)
		{
			return Color.clear;
		}
		Player player = Game.Instance.Player;
		IAbstractUnitEntity entity = player.MainCharacter.Entity;
		bool flag = EntityData.IsEnemy(entity);
		bool flag2 = EntityData.GetFactionOptional()?.Neutral ?? false;
		bool flag3 = player.PartyAndPets.Contains(EntityData);
		bool isDead = EntityData.LifeState.IsDead;
		bool lootViewed = EntityData.LootViewed;
		ViewHighlightingColors viewHighlightingColors = ConfigRoot.Instance.UIConfig.ViewHighlightingColors;
		if (isDead && lootViewed)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.UnitViewedLoot.HoverColor;
			}
			return viewHighlightingColors.UnitViewedLoot.HighlightColor;
		}
		if (isDead)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.UnitLoot.HoverColor;
			}
			return viewHighlightingColors.UnitLoot.HighlightColor;
		}
		if (AdditionalCombatObjective != null)
		{
			if (MouseHoverHighlighting)
			{
				if (flag3)
				{
					return viewHighlightingColors.UnitAlly.HoverColor;
				}
				if (flag)
				{
					return viewHighlightingColors.UnitEnemy.HoverColor;
				}
				return viewHighlightingColors.AdditionalCombatObjective.HoverColor;
			}
			return viewHighlightingColors.AdditionalCombatObjective.HighlightColor;
		}
		if (flag3)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.UnitAlly.HighlightColor;
			}
			return viewHighlightingColors.UnitAlly.HoverColor;
		}
		if (flag)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.UnitEnemy.HighlightColor;
			}
			return viewHighlightingColors.UnitEnemy.HoverColor;
		}
		if (flag2)
		{
			if (!MouseHoverHighlighting)
			{
				return viewHighlightingColors.UnitNeutral.HighlightColor;
			}
			return viewHighlightingColors.UnitNeutral.HoverColor;
		}
		if (!MouseHoverHighlighting)
		{
			return viewHighlightingColors.UnitDefault.HighlightColor;
		}
		return viewHighlightingColors.UnitDefault.HoverColor;
	}

	public virtual Vector3 GetViewPositionOnGround(Vector3 mechanicsPosition)
	{
		using (ProfileScope.NewScope("GetViewPositionOnGround"))
		{
			Vector3 baseViewPositionOnGround = GetBaseViewPositionOnGround(mechanicsPosition);
			return baseViewPositionOnGround + (CalcVerticalShift(baseViewPositionOnGround, MovementAgent.Corpulence * 0.85f) + EntityData.FlyHeight) * Vector3.up;
		}
		static float CalcVerticalShift(Vector3 pos, float corpulence)
		{
			for (float num = 1.5f; num <= 101.5f; num += 5f)
			{
				if (Cast(pos, num, num, corpulence, out var hitOffset2))
				{
					return hitOffset2;
				}
			}
			return 0f;
		}
		static bool Cast(Vector3 point, float offsetUp, float offsetDown, float corpulence, out float hitOffset)
		{
			if (!UnitMovementAgentBase.FallbackToRayCast)
			{
				return SphereCast(point, offsetUp, offsetDown, corpulence, out hitOffset);
			}
			return RayCast(point, offsetUp, offsetDown, out hitOffset);
		}
		static bool RayCast(Vector3 pivot, float offsetUp, float offsetDown, out float hitOffset)
		{
			if (Physics.Raycast(pivot + offsetUp * Vector3.up, Vector3.down, out var hitInfo, offsetDown + offsetUp, 2359553))
			{
				hitOffset = offsetUp - hitInfo.distance;
				return hitInfo.distance != 0f;
			}
			hitOffset = 0f;
			return false;
		}
		static bool SphereCast(Vector3 pivot, float offsetUp, float offsetDown, float corpulence, out float hitOffset)
		{
			if (Physics.SphereCast(pivot + offsetUp * Vector3.up, corpulence, Vector3.down, out var hitInfo2, offsetDown + offsetUp, 2359553))
			{
				hitOffset = offsetUp - hitInfo2.distance - corpulence;
				return hitInfo2.distance != 0f;
			}
			hitOffset = 0f;
			return false;
		}
	}

	private Vector3 GetBaseViewPositionOnGround(Vector3 mechanicsPosition)
	{
		bool flag = Data?.IsDead ?? false;
		if (flag && RigidbodyController != null && RigidbodyController.IsRagdollPositionsRestored)
		{
			return mechanicsPosition;
		}
		if ((bool)CenterTorso && (flag || (IsProne && RigidbodyController != null && RigidbodyController.RagdollWorking && (AnimationManager == null || AnimationManager.CurrentAction == null))))
		{
			return base.ViewTransform.position;
		}
		return mechanicsPosition + SizePathfindingHelper.GetSizePositionOffset(Data);
	}

	public void EnterProneState()
	{
		if (!IsProne)
		{
			IsProne = true;
			StopMoving();
			if ((bool)AgentASP)
			{
				ObstaclesHelper.RemoveFromGroup(AgentASP);
			}
			EntityData.Wake(6f);
			OnEnterProneState();
		}
	}

	public void LeaveProneState()
	{
		if (IsProne)
		{
			m_StartGetUpTime = Game.Instance.Controllers.TimeController.GameTime;
			m_CoreCollider.transform.position = base.ViewTransform.position;
			IsProne = false;
			if ((bool)AgentASP)
			{
				ObstaclesHelper.ConnectToGroups(AgentASP);
			}
			OnExitProneState();
		}
	}

	protected virtual void OnEnterProneState()
	{
	}

	protected virtual void OnExitProneState()
	{
	}

	public void StopMoving()
	{
		if ((bool)AgentASP)
		{
			AgentASP.Stop();
		}
	}

	public override void UpdateViewActive()
	{
		base.UpdateViewActive();
		if ((bool)CharacterAvatar)
		{
			CharacterAvatar.PreventUpdate = !Data.IsViewActive;
		}
		if (AnimationManager != null)
		{
			AnimationManager.enabled = Data.IsViewActive;
		}
	}

	public virtual void HandleDeath()
	{
		SpawnFxOnStart component = GetComponent<SpawnFxOnStart>();
		if ((bool)component)
		{
			component.HandleUnitDeath();
		}
		if (base.IsVisible)
		{
			EntityData.LifeState.IsDeathRevealed = EntityData.IsInCameraFrustum;
			if (EntityData.LifeState.IsFinallyDead)
			{
				Game.Instance.Controllers.CoroutinesController.Start(PlayDeathEffect(), this);
			}
		}
		if (Data.LifeState.IsFinallyDead && !Data.GetOptional<UnitPartCompanion>())
		{
			base.Fader.DisableAnimation();
		}
		Game.Instance.Controllers.CoroutinesController.Start(SwitchCoreColliderToDeadState(), this);
		SetOccluderColorAndState();
	}

	private DeathFxFromEnergyEntry LastDamageFxOptions()
	{
		RolledDamage rolledDamage = EntityData?.Health.LastHandledDamage?.ResultDamage;
		if (rolledDamage == null)
		{
			return DeathFxFromEnergyEntry.Default;
		}
		return ConfigRoot.Instance.FxRoot.DeathFxOptionForEnergyDamage(rolledDamage.Type);
	}

	private IEnumerator PlayDeathEffect()
	{
		if (!LastDamageFxOptions().PlayBloodPuddle)
		{
			yield break;
		}
		GameObject effect = Blueprint.VisualSettings.GetBloodPuddle(EntityData.SurfaceType);
		bool flag = true;
		if (DismembermentHandler.ShouldDismember(Data))
		{
			UnitDismemberType dismemberType = DismembermentHandler.GetDismemberType(Data);
			if (dismemberType == UnitDismemberType.LimbsApart)
			{
				LimbsApartDismember = true;
			}
			else
			{
				GameObject dismember = Blueprint.VisualSettings.GetDismember(EntityData.SurfaceType, dismemberType);
				if (dismember != null)
				{
					effect = dismember;
					flag = false;
					AkUnitySoundEngine.StopAll(base.gameObject);
					BlowUpDismember = true;
				}
			}
		}
		if (!effect)
		{
			yield break;
		}
		if (flag)
		{
			if (AnimationManager != null)
			{
				yield return null;
				while (AnimationManager.IsGoingProne)
				{
					yield return null;
				}
			}
			else if (RigidbodyController != null)
			{
				yield return YieldInstructions.WaitForSecondsGameTime(RigidbodyController.RagdollTime);
			}
		}
		GameObject obj = FxHelper.SpawnFxOnEntity(effect, this);
		UnitFxVisibilityManager.Remove(obj);
		FxHelper.RegisterBlood(obj);
	}

	private IEnumerator SwitchCoreColliderToDeadState()
	{
		TimeSpan finishTime = Game.Instance.Controllers.TimeController.GameTime + TimeSpan.FromSeconds(10.0);
		while (Game.Instance.Controllers.TimeController.GameTime < finishTime && EntityData != null && EntityData.LifeState.IsDead)
		{
			CoreCollider.transform.position = CenterTorso.position;
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		m_Asks?.UnloadBanks();
		m_Asks = null;
		if (m_LateUpdateDriver != null)
		{
			Logger.Error("m_LateUpdateDriver is not null on destroy! Shouldn't happen!");
			m_LateUpdateDriver.Disable();
			m_LateUpdateDriver = null;
		}
		m_BloodyFaceController?.Dispose();
		HandleOnDestroy();
		base.OnDestroy();
	}

	protected virtual void HandleOnDestroy()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_LateUpdateDriver = new LateUpdateDriver(this);
		m_LateUpdateDriver.Enable();
	}

	protected override void OnDisable()
	{
		if (m_LateUpdateDriver != null)
		{
			m_LateUpdateDriver.Disable();
			m_LateUpdateDriver = null;
		}
		base.OnDisable();
	}

	public void DoLateUpdate()
	{
		if (m_RenderersAndCollidersAreUpdated && base.gameObject.activeSelf)
		{
			UpdateCachedRenderersAndColliders();
			m_RenderersAndCollidersAreUpdated = false;
			m_CachedLocalBounds = null;
		}
		OnDoLateUpdate();
		if (!(m_SoftCollider != null))
		{
			return;
		}
		if (SoftColliderPlaceholder == null || !SoftColliderPlaceholder.gameObject.activeInHierarchy)
		{
			SoftColliderPlaceholder = GetComponentInChildren<SoftColliderPlaceholder>(includeInactive: false);
		}
		Transform transform = null;
		if (SoftColliderPlaceholder != null)
		{
			transform = SoftColliderPlaceholder.transform;
		}
		if (transform == null)
		{
			transform = (KeepCollidersSetupAsIs ? null : CenterTorso);
		}
		if (transform != null)
		{
			m_SoftCollider.transform.position = transform.transform.position;
			m_SoftCollider.transform.rotation = transform.transform.rotation;
			if ((bool)SoftColliderPlaceholder && SoftColliderPlaceholder.overrideColliderParameters)
			{
				m_SoftCollider.height = SoftColliderPlaceholder.ColliderHeight;
				m_SoftCollider.radius = SoftColliderPlaceholder.ColliderRadius;
				m_SoftCollider.center = new Vector3(m_SoftCollider.center.x, SoftColliderPlaceholder.ColliderCenterYOffset, m_SoftCollider.center.z);
			}
			if (UseHorizontalSoftCollider && SoftColliderPlaceholder == null)
			{
				m_SoftCollider.transform.Rotate(90f, 0f, 0f, Space.Self);
			}
		}
	}

	protected virtual void OnDoLateUpdate()
	{
	}

	public bool IsMoving()
	{
		if ((bool)AgentASP)
		{
			return AgentASP.IsReallyMoving;
		}
		return false;
	}

	public void MoveTo(ForcedPath path, Vector3 destination, float approachRadiusMeters)
	{
		if ((bool)AgentASP)
		{
			if (Game.Instance.CurrentlyLoadedArea.IsNavmeshArea && (bool)AstarPath.active && AstarPath.active.graphs.Length != 0)
			{
				AgentASP.FollowPath(path, destination, approachRadiusMeters);
				return;
			}
			AgentASP.ForcePath(ForcedPath.Construct(new List<Vector3> { EntityData.Position, destination }));
		}
	}

	internal virtual void OnMovementStarted(Vector3 pathDestination, bool preview = false)
	{
	}

	internal virtual void OnMovementInterrupted(Vector3 destination)
	{
		EntityData.Commands.CurrentMoveTo?.Interrupt();
	}

	internal virtual void OnMovementComplete()
	{
	}

	internal virtual void OnMovementWaypointUpdate()
	{
	}

	internal void OnPathNotFound()
	{
		StopMoving();
	}

	public virtual void HandleDamage()
	{
		if (m_BloodyFaceController != null && !m_BloodyFaceController.IsDisposed)
		{
			m_BloodyFaceController.UpdateBloodValues();
		}
	}

	public virtual float GetSpeedAnimationCoeff(WalkSpeedType type, bool inCombat)
	{
		return 1f;
	}

	protected void ResetHighlightGuard()
	{
		m_MouseHoverHighlighting.Reset();
	}

	protected virtual void UpdateHighlightView()
	{
		UpdateHighlight();
	}

	protected virtual void OnMouseHighlight(bool value)
	{
		Game.Instance.CursorController.SetUnitCursor(EntityData, value);
		if (!EntityData.Features.IsUntargetable)
		{
			EventBus.RaiseEvent(delegate(IUnitDirectHoverUIHandler h)
			{
				h.HandleHoverChange(this, value, m_DirectHover);
			});
		}
		if (EntityData.IsDeadAndHasLoot)
		{
			MassLootHelper.HighlightLoot(this, value);
		}
	}
}
