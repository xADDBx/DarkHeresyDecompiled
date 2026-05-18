using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.AreaLogic.Cutscenes.Components;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Controllers;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.EntitySystem.Interfaces.View;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[OwlPackable(OwlPackableMode.Generate)]
public class CutscenePlayerData : Entity, ICutscenePlayerData, IReloadMechanicsHandler, ISubscriber, IHashable, IOwlPackable<CutscenePlayerData>
{
	private struct DisableStackTraceScope : IDisposable
	{
		private readonly LogSeverity m_PreviousStackTraceLevel;

		public static DisableStackTraceScope Get()
		{
			return new DisableStackTraceScope(Logger.MinStackTraceLevel);
		}

		private DisableStackTraceScope(LogSeverity severity)
		{
			m_PreviousStackTraceLevel = severity;
			Logger.SetMinStackTraceLevel(LogSeverity.Disabled);
		}

		public void Dispose()
		{
			Logger.SetMinStackTraceLevel(m_PreviousStackTraceLevel);
		}
	}

	public static readonly LogChannel Logger = PFLog.Cutscene;

	private static CutscenePlayerData s_LastQueuedCutscene;

	[JsonProperty]
	[OwlPackInclude]
	private CutscenePlayerData m_QueuedAfter;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsQueued;

	[JsonProperty]
	[OwlPackInclude]
	public NamedParametersContext Parameters = new NamedParametersContext();

	[JsonProperty]
	[OwlPackInclude]
	private bool m_Restart;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_Remove;

	[JsonProperty]
	[OwlPackInclude]
	private StatefulRandom m_Random;

	[JsonProperty]
	[OwlPackInclude]
	private BlueprintCutscene m_Cutscene;

	[JsonProperty]
	[OwlPackInclude]
	private List<CutscenePlayerBlockData> m_BlockPlayerData = new List<CutscenePlayerBlockData>();

	[OwlPackInclude]
	private List<int> m_TriggeredStages = new List<int>();

	private bool m_StoppingInProgress;

	private readonly Dictionary<CommandBase, object> m_CommandData = new Dictionary<CommandBase, object>();

	[JsonProperty]
	[OwlPackInclude]
	private readonly Dictionary<CutscenePauseReason, int> m_Paused = new Dictionary<CutscenePauseReason, int>();

	private readonly Dictionary<CutscenePauseReason, int> m_PauseDelayed = new Dictionary<CutscenePauseReason, int>();

	private bool m_TickInProgress;

	private bool m_AnchorsCollected;

	private bool m_ResourcesCollected;

	private bool m_ShouldResume;

	public readonly List<EntityRef> Anchors = new List<EntityRef>();

	public readonly HashSet<CommandBase> FailedCommands = new HashSet<CommandBase>();

	public readonly HashSet<CommandBase> FailedCheckCommands = new HashSet<CommandBase>();

	public readonly HashSet<CutsceneTrack> FinishedTracks = new HashSet<CutsceneTrack>();

	public SimpleBlueprint OriginBlueprint;

	public List<LogInfo> LogList = new List<LogInfo>();

	public CutscenePauseReason LastHandledReason;

	public CutsceneSignalDispatcher CutsceneSignals = new CutsceneSignalDispatcher();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "CutscenePlayerData",
		OldNames = null,
		Fields = new FieldInfo[23]
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
			new FieldInfo("m_QueuedAfter", typeof(CutscenePlayerData)),
			new FieldInfo("m_IsQueued", typeof(bool)),
			new FieldInfo("Parameters", typeof(NamedParametersContext)),
			new FieldInfo("m_Restart", typeof(bool)),
			new FieldInfo("m_Remove", typeof(bool)),
			new FieldInfo("Exclusive", typeof(bool)),
			new FieldInfo("PlayActionId", typeof(string)),
			new FieldInfo("IsFinished", typeof(bool)),
			new FieldInfo("m_Random", typeof(StatefulRandom)),
			new FieldInfo("m_Cutscene", typeof(BlueprintCutscene)),
			new FieldInfo("m_BlockPlayerData", typeof(List<CutscenePlayerBlockData>)),
			new FieldInfo("m_TriggeredStages", typeof(List<int>)),
			new FieldInfo("m_Paused", typeof(Dictionary<CutscenePauseReason, int>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public bool Exclusive { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public string PlayActionId { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsFinished { get; private set; }

	public float PlayingTime { get; set; }

	public bool PreventDestruction { get; set; }

	public bool TraceCommands { get; set; }

	public Vector3 CameraStartingPosition { get; set; }

	public float CameraStartingRotation { get; set; }

	public float CameraStartingZoom { get; set; }

	public ParametrizedContextSetter ParameterSetter { get; set; }

	public bool Paused => m_Paused.Count > 0;

	public new ICutscenePlayerView View => (ICutscenePlayerView)base.View;

	public BlueprintCutscene Cutscene => m_Cutscene;

	public ICutscene ICutscene => Cutscene;

	public bool HasActiveLockControl
	{
		get
		{
			if (!IsFinished && !Paused)
			{
				if (!m_Cutscene.LockControl)
				{
					return m_BlockPlayerData.Any((CutscenePlayerBlockData b) => b.IsActivated && !b.IsComplete && b.Block.LockControl);
				}
				return true;
			}
			return false;
		}
	}

	public bool RequireLockControl
	{
		get
		{
			if (!IsFinished && !Paused)
			{
				if (!m_Cutscene.LockControl)
				{
					return m_BlockPlayerData.Any((CutscenePlayerBlockData b) => b.CanBeActivated && b.Block.LockControl);
				}
				return true;
			}
			return false;
		}
	}

	public bool IsQueued => m_IsQueued;

	public bool IsFirstInQueue
	{
		get
		{
			if (m_IsQueued)
			{
				if (m_QueuedAfter != null)
				{
					return m_QueuedAfter.IsDisposed;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsLastInQueue
	{
		get
		{
			if (m_IsQueued)
			{
				return s_LastQueuedCutscene == this;
			}
			return false;
		}
	}

	public IEnumerable<CutscenePauseReason> PauseReasons => from i in m_Paused
		where i.Value > 0
		select i.Key;

	public static IEnumerable<CutscenePlayerData> Queue
	{
		get
		{
			for (CutscenePlayerData q = s_LastQueuedCutscene; q != null; q = q.m_QueuedAfter)
			{
				yield return q;
			}
		}
	}

	public StatefulRandom Random => m_Random;

	public CutscenePlayerData(BlueprintCutscene cutscene, IEntityConfig player)
		: base(player.EntityId, player.IsInGameBySettings)
	{
		m_Cutscene = cutscene;
		foreach (CutsceneBlock block in m_Cutscene.Blocks)
		{
			m_BlockPlayerData.Add(new CutscenePlayerBlockData(block, this));
		}
	}

	protected CutscenePlayerData(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public CutscenePlayerDataScope GetDataScope()
	{
		return ContextData<CutscenePlayerDataScope>.Request().Setup(this);
	}

	public T GetCommandData<T>(CommandBase cmd) where T : class, new()
	{
		m_CommandData.TryGetValue(cmd, out var value);
		T val = value as T;
		if (val == null)
		{
			val = (T)(m_CommandData[cmd] = new T());
		}
		return val;
	}

	public void ClearCommandData(CommandBase cmd)
	{
		m_CommandData.Remove(cmd);
	}

	public void SetPaused(bool value, CutscenePauseReason reason)
	{
		if (m_TickInProgress)
		{
			m_PauseDelayed[reason] = m_PauseDelayed.Get(reason, 0) + (value ? 1 : (-1));
			return;
		}
		bool paused = Paused;
		int num = m_Paused.Get(reason, 0) + (value ? 1 : (-1));
		if (num <= 0)
		{
			m_Paused.Remove(reason);
		}
		else
		{
			m_Paused[reason] = num;
		}
		if (Paused && !paused)
		{
			using (Parameters.RequestContextData())
			{
				m_BlockPlayerData.ForEach(delegate(CutscenePlayerBlockData bp)
				{
					bp.Pause();
				});
			}
			RaiseEvent(this, delegate(ICutsceneHandler h)
			{
				h.HandleCutscenePaused(reason);
			});
			LastHandledReason = reason;
		}
		if (paused && !Paused)
		{
			Resume();
			RaiseEvent(this, delegate(ICutsceneHandler h)
			{
				h.HandleCutsceneResumed();
			});
		}
	}

	public void InterruptBark()
	{
		if (Paused)
		{
			return;
		}
		using (Parameters.RequestContextData())
		{
			m_BlockPlayerData.ForEach(delegate(CutscenePlayerBlockData bp)
			{
				bp.InterruptBark();
			});
		}
	}

	private bool AllUnitsAreFreezed()
	{
		bool flag = false;
		bool flag2 = true;
		foreach (EntityRef anchor in Anchors)
		{
			IEntity entity = anchor.Entity;
			if (entity != null && entity.IsInGame && entity is AbstractUnitEntity { FreezeOutsideCamera: not false } abstractUnitEntity)
			{
				flag = true;
				if (!abstractUnitEntity.IsSleeping)
				{
					flag2 = false;
					break;
				}
			}
		}
		return flag && flag2;
	}

	private bool HasActiveAnchor()
	{
		foreach (EntityRef anchor in Anchors)
		{
			IEntity entity = anchor.Entity;
			if (entity == null || !entity.IsInGame)
			{
				continue;
			}
			if (entity.Suppressed)
			{
				return false;
			}
			if (!entity.IsInFogOfWar)
			{
				return true;
			}
			Vector3 position = entity.Position;
			foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
			{
				if ((partyAndPet.Position - position).sqrMagnitude <= 585.64f)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override IEntityView CreateViewForData()
	{
		if (!Cutscene)
		{
			return null;
		}
		CutscenePlayerView cutscenePlayerView = new GameObject("[cutscene player " + Cutscene.name + "]").AddComponent<CutscenePlayerView>();
		cutscenePlayerView.Cutscene = Cutscene;
		cutscenePlayerView.UniqueId = base.UniqueId;
		return cutscenePlayerView;
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		uint uintValue = PFStatefulRandom.Cutscene.uintValue;
		if (m_Random == null)
		{
			m_Random = new StatefulRandom("Cutscene " + base.UniqueId, uintValue);
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (Cutscene != null && Cutscene.GetComponent<DestroyCutsceneOnLoad>() != null)
		{
			m_Remove = true;
		}
		if (m_IsQueued && (s_LastQueuedCutscene == null || s_LastQueuedCutscene.HoldingState != HoldingState))
		{
			FixupMultipleQueues();
		}
		bool flag = true;
		foreach (CutscenePlayerBlockData blockPlayerDatum in m_BlockPlayerData)
		{
			flag &= blockPlayerDatum.TryRestoreOnPostLoad(m_Cutscene, this);
		}
		if (!flag)
		{
			m_Restart = true;
		}
		StopCopies();
		m_ShouldResume = !Paused;
	}

	private void FixupMultipleQueues()
	{
		s_LastQueuedCutscene = null;
		foreach (CutscenePlayerData cutscene in Game.Instance.EntityPools.Cutscenes)
		{
			if (!cutscene.m_IsQueued)
			{
				continue;
			}
			s_LastQueuedCutscene = s_LastQueuedCutscene ?? cutscene;
			CutscenePlayerData queuedAfter = s_LastQueuedCutscene;
			while (queuedAfter != null && queuedAfter != cutscene)
			{
				queuedAfter = queuedAfter.m_QueuedAfter;
			}
			if (queuedAfter != cutscene)
			{
				queuedAfter = cutscene;
				while (queuedAfter.m_QueuedAfter != null && queuedAfter.m_QueuedAfter != s_LastQueuedCutscene)
				{
					queuedAfter = queuedAfter.m_QueuedAfter;
				}
				if (queuedAfter.m_QueuedAfter == s_LastQueuedCutscene)
				{
					s_LastQueuedCutscene = cutscene;
					continue;
				}
				queuedAfter.m_QueuedAfter = s_LastQueuedCutscene;
				s_LastQueuedCutscene = cutscene;
			}
		}
	}

	public void TickScene(bool skipping = false)
	{
		if (IsFinished)
		{
			if (!PreventDestruction)
			{
				Game.Instance.Controllers.EntityDestroyer.Destroy(this);
			}
			return;
		}
		if (m_QueuedAfter != null)
		{
			if (!m_QueuedAfter.IsInState)
			{
				m_QueuedAfter = null;
			}
			else if (!m_QueuedAfter.IsFinished)
			{
				return;
			}
		}
		try
		{
			CollectAnchors();
			using (ProfileScope.New("UpdateActiveCutscenes"))
			{
				foreach (EntityRef anchor in Anchors)
				{
					if (anchor.Entity is BaseUnitEntity unit)
					{
						CutsceneControlledUnit.UpdateActiveCutscene(unit);
					}
				}
			}
			using (ProfileScope.New("Check Should Pause Cutscene"))
			{
				bool flag = !Cutscene.Sleepless && !HasActiveLockControl && Anchors.Count > 0 && !HasActiveAnchor();
				bool flag2 = m_Paused.ContainsKey(CutscenePauseReason.HasNoActiveAnchors);
				if (flag != flag2)
				{
					SetPaused(flag, CutscenePauseReason.HasNoActiveAnchors);
				}
				if (!flag)
				{
					bool flag3 = !Cutscene.Sleepless && !Cutscene.Freezeless && !HasActiveLockControl && Anchors.Count > 0 && AllUnitsAreFreezed();
					bool flag4 = m_Paused.ContainsKey(CutscenePauseReason.Freezed);
					if (flag3 != flag4)
					{
						SetPaused(flag3, CutscenePauseReason.Freezed);
					}
				}
			}
			if (m_ShouldResume)
			{
				Resume();
				m_ShouldResume = false;
			}
			if (Paused)
			{
				return;
			}
			m_TickInProgress = true;
			m_PauseDelayed.Clear();
			using (ProfileScope.New("Process Tick"))
			{
				Parameters.Cutscene = this;
				BarkController barkController = Game.Instance.Controllers.BarkController;
				bool suppressOtherBarks = m_Cutscene.SuppressOtherBarks;
				if (suppressOtherBarks)
				{
					barkController.EnterCutsceneContext();
				}
				try
				{
					using (Parameters.RequestContextData())
					{
						for (int i = 0; i < m_BlockPlayerData.Count; i++)
						{
							bool skipping2 = skipping && !Cutscene.NonSkippable && (Cutscene.LockControl || m_BlockPlayerData[i].Block.LockControl);
							m_BlockPlayerData[i].TickBlock(skipping2);
						}
					}
				}
				finally
				{
					if (suppressOtherBarks)
					{
						barkController.LeaveCutsceneContext();
					}
				}
			}
			m_TickInProgress = false;
			foreach (KeyValuePair<CutscenePauseReason, int> item in m_PauseDelayed)
			{
				var (reason, j) = (KeyValuePair<CutscenePauseReason, int>)(ref item);
				while (j > 0)
				{
					SetPaused(value: true, reason);
					j--;
				}
				for (; j < 0; j++)
				{
					SetPaused(value: false, reason);
				}
			}
			if (Paused)
			{
				return;
			}
			PlayingTime += Game.Instance.Controllers.TimeController.DeltaTime;
			bool flag5 = true;
			for (int k = 0; k < m_BlockPlayerData.Count; k++)
			{
				flag5 &= !m_BlockPlayerData[k].IsActivated || m_BlockPlayerData[k].IsComplete;
			}
			if (flag5)
			{
				CheckResetZoom();
				if (Cutscene.StageSwitches.All((CutsceneStageSwitch s) => m_TriggeredStages.Contains(s.StageId)))
				{
					IsFinished = true;
					if (Cutscene.LockControl && !skipping)
					{
						Metrics.Cutscene.Id(Cutscene.AssetGuid).Skipped(skipped: false).Send();
					}
				}
				else
				{
					SetPaused(value: true, CutscenePauseReason.AwaitingNextStage);
				}
			}
		}
		catch (Exception arg)
		{
			IsFinished = true;
			LogError($"[{Cutscene.NameSafe()}] Unhandled exception: {arg}", needQaReport: true);
		}
		finally
		{
			m_TickInProgress = false;
		}
		if (IsFinished)
		{
			if (Cutscene.SuppressOtherBarks)
			{
				Game.Instance.Controllers.BarkController.LeaveSuppression();
			}
			Cutscene.OnFinished?.Run();
		}
		if (IsFinished && !PreventDestruction)
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(this);
		}
	}

	public void Start(bool queued = false)
	{
		ParametrizedContextSetter.ParameterEntry[] parameters = Cutscene.DefaultParameters.Parameters;
		foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in parameters)
		{
			if (!Parameters.Params.ContainsKey(parameterEntry.Name))
			{
				Parameters.Params[parameterEntry.Name] = parameterEntry.GetValue();
			}
		}
		StopCopies();
		PreloadCutsceneResources();
		if (Cutscene.LockControl && CameraRig.Instance?.Camera != null)
		{
			CameraStartingPosition = CameraRig.Instance.transform.position;
			CameraStartingRotation = CameraRig.Instance.transform.rotation.eulerAngles.y;
			CameraStartingZoom = CameraRig.Instance.CameraZoom.CurrentNormalizePosition;
		}
		if (queued)
		{
			m_IsQueued = true;
			if (s_LastQueuedCutscene != null && !s_LastQueuedCutscene.IsDisposed)
			{
				m_QueuedAfter = s_LastQueuedCutscene;
			}
			s_LastQueuedCutscene = this;
		}
		if (Cutscene.SuppressOtherBarks)
		{
			Game.Instance.Controllers.BarkController.StopAll();
			Game.Instance.Controllers.BarkController.EnterSuppression();
		}
		RaiseEvent(this, delegate(ICutsceneHandler h)
		{
			h.HandleCutsceneStarted(queued);
		});
	}

	private void CollectResourcesFromAction(GameAction action, HashSet<EntityRef> result, HashSet<CutsceneGate> seenGates)
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			if (action is Spawn { Spawners: var spawners } spawn)
			{
				for (int i = 0; i < spawners.Length; i++)
				{
					AbstractUnitSpawnerEntity unitSpawner = GameHelper.GetUnitSpawner(spawners[i]);
					if (!(unitSpawner?.Blueprint?.Prefab == null))
					{
						unitSpawner.Blueprint.Prefab.Load();
						GameAction[] actions = spawn.ActionsOnSpawn.Actions;
						foreach (GameAction action2 in actions)
						{
							CollectResourcesFromAction(action2, result, seenGates);
						}
					}
				}
				return;
			}
			if (action is SpawnByUnitGroup spawnByUnitGroup)
			{
				if (spawnByUnitGroup.m_Group.FindData() is UnitGroupEntity unitGroupEntity)
				{
					foreach (AbstractUnitSpawnerEntity item in unitGroupEntity.Members.OfType<AbstractUnitSpawnerEntity>())
					{
						AbstractUnitSpawnerEntity unitSpawner2 = GameHelper.GetUnitSpawner(item);
						if (!(unitSpawner2?.Blueprint?.Prefab == null))
						{
							unitSpawner2.Blueprint.Prefab.Load();
						}
					}
				}
				GameAction[] actions = spawnByUnitGroup.ActionsOnSpawn.Actions;
				foreach (GameAction action3 in actions)
				{
					CollectResourcesFromAction(action3, result, seenGates);
				}
				return;
			}
			if (action is SpawnFx spawnFx)
			{
				PooledGameObject component = spawnFx.FxPrefab.GetComponent<PooledGameObject>();
				if (component != null)
				{
					GameObjectsPool.Warmup(component, 1);
				}
				return;
			}
			if (action is PlayCutscene playCutscene)
			{
				{
					foreach (CutsceneBlock block in playCutscene.Cutscene.Blocks)
					{
						CollectResourcesFromBlock(block, result, seenGates);
					}
					return;
				}
			}
			if (action is Conditional conditional)
			{
				GameAction[] actions = conditional.IfTrue.Actions;
				foreach (GameAction action4 in actions)
				{
					CollectResourcesFromAction(action4, result, seenGates);
				}
				actions = conditional.IfFalse.Actions;
				foreach (GameAction action5 in actions)
				{
					CollectResourcesFromAction(action5, result, seenGates);
				}
			}
		}
	}

	private void CollectResourcesFromBlock(CutsceneBlock block, HashSet<EntityRef> result, HashSet<CutsceneGate> seenGates)
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			foreach (CutsceneGate gate in block.Gates)
			{
				if (!seenGates.Add(gate))
				{
					continue;
				}
				foreach (CutsceneTrack track in gate.Tracks)
				{
					if (track == null)
					{
						continue;
					}
					foreach (CommandBase item in from c in track.Commands
						where c != null
						select c.Get())
					{
						if (item == null)
						{
							continue;
						}
						if (item is CommandAction commandAction && commandAction.Action?.Actions != null)
						{
							GameAction[] actions = commandAction.Action.Actions;
							foreach (GameAction action in actions)
							{
								CollectResourcesFromAction(action, result, seenGates);
							}
						}
						else if (item is CommandUnitPlayCutsceneAnimation commandUnitPlayCutsceneAnimation)
						{
							commandUnitPlayCutsceneAnimation.Preload();
						}
					}
				}
			}
		}
	}

	public void PreloadCutsceneResources()
	{
		if (m_ResourcesCollected)
		{
			return;
		}
		m_ResourcesCollected = true;
		using (CodeTimer.New($"PreloadCutsceneResources {this}"))
		{
			using (ProfileScope.New("PreloadCutsceneResources"))
			{
				HashSet<CutsceneGate> seenGates = TempHashSet.Get<CutsceneGate>();
				foreach (CutsceneBlock block in m_Cutscene.Blocks)
				{
					CollectResourcesFromBlock(block, null, seenGates);
				}
			}
		}
	}

	private void StopCopies()
	{
		if (Cutscene.AllowCopies || m_Remove)
		{
			return;
		}
		foreach (CutscenePlayerData item in Game.Instance.EntityPools.Cutscenes.ToTempList())
		{
			if (item != this && !item.m_Remove && item.Cutscene == Cutscene && Parameters.IsTheSame(item.Parameters))
			{
				item.Stop();
				item.m_Remove = true;
			}
		}
	}

	private void CheckResetZoom()
	{
		if (HasActiveLockControl)
		{
			float? num = CameraRig.Instance?.CameraZoom.CurrentNormalizePosition;
			if (num.HasValue && !Mathf.Approximately(num.Value, CameraStartingZoom))
			{
				CameraRig.Instance.CameraZoom.ResetZoom(CameraStartingZoom);
			}
		}
	}

	protected override void OnDispose()
	{
		using (Parameters.RequestContextData())
		{
			m_BlockPlayerData.ForEach(delegate(CutscenePlayerBlockData bp)
			{
				bp.Dispose();
			});
		}
		m_BlockPlayerData.Clear();
		CutsceneSignals.Clear();
		if (s_LastQueuedCutscene == this)
		{
			s_LastQueuedCutscene = null;
		}
		base.OnDispose();
	}

	private void Resume()
	{
		if (m_Remove)
		{
			m_Remove = false;
			m_Restart = false;
			m_IsQueued = false;
			m_QueuedAfter = null;
			m_BlockPlayerData.Clear();
			return;
		}
		if (m_Restart)
		{
			m_Restart = false;
			m_BlockPlayerData.Clear();
			m_TriggeredStages.Clear();
			Exclusive = false;
			foreach (CutsceneBlock block in m_Cutscene.Blocks)
			{
				m_BlockPlayerData.Add(new CutscenePlayerBlockData(block, this));
			}
			RaiseEvent(this, delegate(ICutsceneHandler h)
			{
				h.HandleCutsceneRestarted();
			});
			return;
		}
		using (Parameters.RequestContextData())
		{
			m_BlockPlayerData.ForEach(delegate(CutscenePlayerBlockData bp)
			{
				bp.Resume();
			});
		}
	}

	public void SignalGateExtra(string gateId)
	{
		foreach (CutscenePlayerBlockData blockPlayerDatum in m_BlockPlayerData)
		{
			blockPlayerDatum.SignalGateExtra(gateId);
		}
	}

	public void StartCutsceneStage(int stage, bool stopOtherStages)
	{
		if (m_TriggeredStages.Contains(stage))
		{
			PFLog.Cutscene.Warning("Stage trigger called more than once. And will be ignored.");
			return;
		}
		if (stopOtherStages)
		{
			foreach (CutscenePlayerBlockData blockPlayerDatum in m_BlockPlayerData)
			{
				if (blockPlayerDatum.IsActivated && !blockPlayerDatum.IsComplete)
				{
					blockPlayerDatum.Stop();
				}
			}
		}
		m_TriggeredStages.Add(stage);
		if (Paused)
		{
			SetPaused(value: false, CutscenePauseReason.AwaitingNextStage);
		}
		foreach (CutsceneStageSwitch stageSwitch in m_Cutscene.StageSwitches)
		{
			if (stageSwitch.StageId != stage)
			{
				continue;
			}
			foreach (string signalGuid in stageSwitch.NextBlocks)
			{
				CutscenePlayerBlockData cutscenePlayerBlockData = m_BlockPlayerData.FirstOrDefault((CutscenePlayerBlockData b) => b.Block.Guid == signalGuid);
				using (Parameters.RequestContextData())
				{
					cutscenePlayerBlockData?.SignalBlock();
				}
			}
		}
	}

	public void Stop()
	{
		if (m_StoppingInProgress)
		{
			return;
		}
		if (Cutscene.SuppressOtherBarks && !IsFinished)
		{
			Game.Instance.Controllers.BarkController.LeaveSuppression();
		}
		m_StoppingInProgress = true;
		if (!IsFinished)
		{
			using (Parameters.RequestContextData())
			{
				m_BlockPlayerData.ForEach(delegate(CutscenePlayerBlockData bp)
				{
					bp.Stop();
				});
			}
		}
		RaiseEvent(this, delegate(ICutsceneHandler h)
		{
			h.HandleCutsceneStopped();
		});
		IsFinished = true;
		m_StoppingInProgress = false;
		if (!PreventDestruction)
		{
			Game.Instance.Controllers.EntityDestroyer.Destroy(this);
		}
	}

	public bool HasCommandData(CommandBase commandBase)
	{
		return m_CommandData.ContainsKey(commandBase);
	}

	private void CollectAnchors()
	{
		if (m_AnchorsCollected)
		{
			return;
		}
		Anchors.AddRange(Cutscene.Anchors.Select((EntityReference r) => new EntityRef(r.UniqueId)));
		m_AnchorsCollected = true;
		using (Parameters.RequestContextData())
		{
			HashSet<EntityRef> hashSet = new HashSet<EntityRef>();
			HashSet<CutsceneGate> seenGates = new HashSet<CutsceneGate>();
			foreach (CutsceneBlock block in m_Cutscene.Blocks)
			{
				CollectAnchorsFromBlock(block, hashSet, seenGates);
			}
			Anchors.AddRange(hashSet);
		}
	}

	public void CollectAnchorsFromBlock(CutsceneBlock block, HashSet<EntityRef> result, HashSet<CutsceneGate> seenGates)
	{
		foreach (CutsceneGate gate in block.Gates)
		{
			if (!seenGates.Add(gate))
			{
				break;
			}
			foreach (CutsceneTrack track in gate.Tracks)
			{
				if (track == null)
				{
					continue;
				}
				foreach (CommandBase item in from c in track.Commands
					where c != null
					select c.Get())
				{
					try
					{
						AbstractUnitEntity abstractUnitEntity = item?.GetControlledUnit()?.ToAbstractUnitEntity();
						if (abstractUnitEntity != null)
						{
							result.Add(abstractUnitEntity.Ref);
						}
						else if (item != null && item.ShouldHaveControlledUnit)
						{
							LogError($"[{Cutscene.Name}]: Cmd should have a controlled unit, but it doesn't. {item}");
						}
					}
					catch (Exception arg)
					{
						LogError($"[{Cutscene.Name}]: Evaluation exception {item}: {arg}");
					}
				}
			}
		}
	}

	public List<AbstractUnitEntity> GetCurrentControlledUnits()
	{
		List<AbstractUnitEntity> currentControlledUnits = new List<AbstractUnitEntity>();
		using (Parameters.RequestContextData())
		{
			m_BlockPlayerData.ForEach(delegate(CutscenePlayerBlockData bp)
			{
				currentControlledUnits.AddRange(bp.GetCurrentControlledUnits());
			});
		}
		return currentControlledUnits;
	}

	public void LogError(string message, bool needQaReport = false, [CanBeNull] CommandBase failedCommand = null)
	{
		if (failedCommand != null)
		{
			FailedCommands.Add(failedCommand);
		}
		CutsceneLogSink.Instance.PrepareForLog(this, failedCommand);
		if (needQaReport)
		{
			Logger.ErrorWithReport(Cutscene, message);
		}
		else
		{
			Logger.Error(Cutscene, message);
		}
	}

	public void LogCommandTrace(string message)
	{
		if (TraceCommands)
		{
			LogSeverity minStackTraceLevel = Logger.MinStackTraceLevel;
			Logger.SetMinStackTraceLevel(LogSeverity.Error);
			CutsceneLogSink.Instance.PrepareForLog(this, null);
			Logger.Log(Cutscene, $"[{Cutscene}] {message}");
			Logger.SetMinStackTraceLevel(minStackTraceLevel);
		}
	}

	public void StopGateOnErrorInsideTrack(CutscenePlayerTrackData track)
	{
		foreach (CutscenePlayerBlockData blockPlayerDatum in m_BlockPlayerData)
		{
			blockPlayerDatum.StopGateOnErrorInsideTrack(track);
		}
	}

	private void ClearErrorStatus()
	{
		foreach (CutscenePlayerBlockData blockPlayerDatum in m_BlockPlayerData)
		{
			foreach (CutscenePlayerGateData gatesPlayerDatum in blockPlayerDatum.GatesPlayerData)
			{
				foreach (CutscenePlayerTrackData track in gatesPlayerDatum.Tracks)
				{
					foreach (CutscenePlayerCommandData item in track.CommandsPlayer)
					{
						item.ClearErrorStatus();
					}
				}
			}
		}
	}

	private void RaiseEvent<T>(CutscenePlayerData entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<CutscenePlayerData>
	{
		base.EventBus.RaiseEvent(entity, action, isCheckRuntime);
	}

	public void OnBeforeMechanicsReload()
	{
	}

	public void OnMechanicsReloaded()
	{
		if (!IsFinished && FailedCommands.Count > 0)
		{
			ClearErrorStatus();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<CutscenePlayerData>.GetHash128(m_QueuedAfter);
		result.Append(ref val2);
		result.Append(ref m_IsQueued);
		Hash128 val3 = NamedParametersContext.Hasher.GetHash128(Parameters);
		result.Append(ref val3);
		result.Append(ref m_Restart);
		result.Append(ref m_Remove);
		bool val4 = Exclusive;
		result.Append(ref val4);
		result.Append(PlayActionId);
		bool val5 = IsFinished;
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<StatefulRandom>.GetHash128(m_Random);
		result.Append(ref val6);
		Hash128 val7 = SimpleBlueprintHasher.GetHash128(m_Cutscene);
		result.Append(ref val7);
		List<CutscenePlayerBlockData> blockPlayerData = m_BlockPlayerData;
		if (blockPlayerData != null)
		{
			for (int i = 0; i < blockPlayerData.Count; i++)
			{
				Hash128 val8 = ClassHasher<CutscenePlayerBlockData>.GetHash128(blockPlayerData[i]);
				result.Append(ref val8);
			}
		}
		Dictionary<CutscenePauseReason, int> paused = m_Paused;
		if (paused != null)
		{
			int val9 = 0;
			foreach (KeyValuePair<CutscenePauseReason, int> item in paused)
			{
				Hash128 hash = default(Hash128);
				CutscenePauseReason obj = item.Key;
				Hash128 val10 = UnmanagedHasher<CutscenePauseReason>.GetHash128(ref obj);
				hash.Append(ref val10);
				int obj2 = item.Value;
				Hash128 val11 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash.Append(ref val11);
				val9 ^= hash.GetHashCode();
			}
			result.Append(ref val9);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		CutscenePlayerData source = new CutscenePlayerData(default(OwlPackConstructorParameter));
		result = Unsafe.As<CutscenePlayerData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<CutscenePlayerData>(OwlPackTypeInfo);
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
		formatter.Field(10, "m_QueuedAfter", ref m_QueuedAfter, state);
		formatter.UnmanagedField(11, "m_IsQueued", ref m_IsQueued, state);
		formatter.Field(12, "Parameters", ref Parameters, state);
		formatter.UnmanagedField(13, "m_Restart", ref m_Restart, state);
		formatter.UnmanagedField(14, "m_Remove", ref m_Remove, state);
		bool value2 = Exclusive;
		formatter.UnmanagedField(15, "Exclusive", ref value2, state);
		string value3 = PlayActionId;
		formatter.StringField(16, "PlayActionId", ref value3, state);
		bool value4 = IsFinished;
		formatter.UnmanagedField(17, "IsFinished", ref value4, state);
		formatter.Field(18, "m_Random", ref m_Random, state);
		formatter.Field(19, "m_Cutscene", ref m_Cutscene, state);
		formatter.Field(20, "m_BlockPlayerData", ref m_BlockPlayerData, state);
		formatter.Field(21, "m_TriggeredStages", ref m_TriggeredStages, state);
		Dictionary<CutscenePauseReason, int> value5 = m_Paused;
		formatter.Field(22, "m_Paused", ref value5, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CutscenePlayerData>();
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
				m_QueuedAfter = formatter.ReadPackable<CutscenePlayerData>(state);
				break;
			case 11:
				m_IsQueued = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				Parameters = formatter.ReadPackable<NamedParametersContext>(state);
				break;
			case 13:
				m_Restart = formatter.ReadUnmanaged<bool>(state);
				break;
			case 14:
				m_Remove = formatter.ReadUnmanaged<bool>(state);
				break;
			case 15:
				Exclusive = formatter.ReadUnmanaged<bool>(state);
				break;
			case 16:
				PlayActionId = formatter.ReadString(state);
				break;
			case 17:
				IsFinished = formatter.ReadUnmanaged<bool>(state);
				break;
			case 18:
				m_Random = formatter.ReadPackable<StatefulRandom>(state);
				break;
			case 19:
				m_Cutscene = formatter.ReadPackable<BlueprintCutscene>(state);
				break;
			case 20:
				m_BlockPlayerData = formatter.ReadPackable<List<CutscenePlayerBlockData>>(state);
				break;
			case 21:
				m_TriggeredStages = formatter.ReadPackable<List<int>>(state);
				break;
			case 22:
				Unsafe.AsRef(in m_Paused) = formatter.ReadPackable<Dictionary<CutscenePauseReason, int>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
