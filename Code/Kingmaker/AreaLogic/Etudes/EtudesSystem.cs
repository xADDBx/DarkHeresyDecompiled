using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[OwlPackable(OwlPackableMode.Generate)]
public class EtudesSystem : Entity, IUnlockHandler, ISubscriber, IUnlockValueHandler, ICompanionChangeHandler, ISubscriber<IBaseUnitEntity>, IPartyHandler, IAreaHandler, IQuestObjectiveHandler, ITimeOfDayChangedHandler, ISummonPoolHandler, ISubscriber<IMechanicEntity>, IAreaPartHandler, IAdditiveAreaSwitchHandler, IUnitCombatHandler, IHashable, IOwlPackable<EtudesSystem>
{
	public enum EtudeState
	{
		Unknown,
		Started,
		Completed,
		PreStarted,
		PreCompleted
	}

	public class EtudesDataGameStateAdapter : DictionaryConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			writer.WriteStartArray();
			foreach (KeyValuePair<BlueprintEtudeReference, EtudeState> item in ((Dictionary<BlueprintEtudeReference, EtudeState>)value)?.Where(IsItemValid))
			{
				serializer.Serialize(writer, item);
			}
			writer.WriteEndArray();
		}

		public static Hash128 GetHash128(Dictionary<BlueprintEtudeReference, EtudeState> obj)
		{
			int val = 0;
			foreach (KeyValuePair<BlueprintEtudeReference, EtudeState> item in obj.Where(IsItemValid))
			{
				val ^= SimpleBlueprintHasher.GetHash128((BlueprintEtude)item.Key).GetHashCode();
				EtudeState obj2 = item.Value;
				val ^= EnumHasher<EtudeState>.GetHash128(ref obj2).GetHashCode();
			}
			Hash128 result = default(Hash128);
			result.Append(ref val);
			return result;
		}

		private static bool IsItemValid(KeyValuePair<BlueprintEtudeReference, EtudeState> item)
		{
			return item.Key.Get()?.IsGameState() ?? false;
		}
	}

	public const string ID = "etudes-system-id";

	private const bool ReplayLogStackTrace = false;

	public new static readonly EntityRef<EtudesSystem> Ref = new EntityRef<EtudesSystem>("etudes-system-id");

	private bool m_IsUpdateForUnload;

	private BlueprintAreaPart m_AreaPartBeingLoaded;

	private BlueprintAreaPart m_AreaPartBeingExited;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "EtudesSystem",
		OldNames = null,
		Fields = new FieldInfo[12]
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
			new FieldInfo("m_EtudesData", typeof(Dictionary<BlueprintEtudeReference, EtudeState>)),
			new FieldInfo("m_HeldConflictingGroups", typeof(Dictionary<BlueprintEtudeConflictingGroup, BlueprintEtude>))
		}
	};

	[JsonProperty]
	[HasherCustom(Type = typeof(EtudesDataGameStateAdapter))]
	[OwlPackInclude]
	private Dictionary<BlueprintEtudeReference, EtudeState> m_EtudesData { get; set; } = new Dictionary<BlueprintEtudeReference, EtudeState>();


	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<BlueprintEtudeConflictingGroup, BlueprintEtude> m_HeldConflictingGroups { get; set; } = new Dictionary<BlueprintEtudeConflictingGroup, BlueprintEtude>();


	public EtudesTree Etudes { get; private set; }

	public bool ConditionsDirty { get; private set; }

	public BlueprintAreaPart LoadEtudesForAreaPart
	{
		get
		{
			object obj;
			if (!m_IsUpdateForUnload)
			{
				obj = SimpleBlueprintExtendAsObject.Or(m_AreaPartBeingLoaded, null);
				if (obj == null)
				{
					return Game.Instance.CurrentlyLoadedAreaPart;
				}
			}
			else
			{
				obj = null;
			}
			return (BlueprintAreaPart)obj;
		}
	}

	public BlueprintAreaPart AreaPartBeingExited => m_AreaPartBeingExited;

	public bool EtudeIsNotStarted(BlueprintEtude etude)
	{
		return !m_EtudesData.ContainsKey(etude.ToReference<BlueprintEtudeReference>());
	}

	public bool EtudeIsCompleted(BlueprintEtude etude)
	{
		if (!m_EtudesData.TryGetValue(etude.ToReference<BlueprintEtudeReference>(), out var value))
		{
			return false;
		}
		if (value != EtudeState.Completed)
		{
			return value == EtudeState.PreCompleted;
		}
		return true;
	}

	public bool EtudeIsPreCompleted(BlueprintEtude etude)
	{
		if (!m_EtudesData.TryGetValue(etude.ToReference<BlueprintEtudeReference>(), out var value))
		{
			return false;
		}
		return value == EtudeState.PreCompleted;
	}

	public bool EtudeIsPlaying(BlueprintEtude etude)
	{
		return Etudes.Get(etude)?.IsPlaying ?? false;
	}

	public EtudeState GetSavedState(BlueprintEtude bp)
	{
		m_EtudesData.TryGetValue(bp.ToReference<BlueprintEtudeReference>(), out var value);
		return value;
	}

	protected EtudesSystem(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public EtudesSystem()
		: base("etudes-system-id", isInGame: true)
	{
	}

	protected override void OnPrepareOrPrePostLoad()
	{
		base.OnPrepareOrPrePostLoad();
		Etudes = Facts.EnsureFactProcessor<EtudesTree>();
	}

	public void MarkConditionsDirty()
	{
		ConditionsDirty = true;
	}

	public void ClearConditionsDirty()
	{
		ConditionsDirty = false;
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		Etudes = Facts.EnsureFactProcessor<EtudesTree>();
		Etudes.RestoreTreeStructure();
		FixupActorChanges();
	}

	protected override void OnDidPostLoad()
	{
		Etudes.FixupEtudesTree(this);
		Etudes.CheckEtudeHierarchyForErrors();
	}

	public void OnAreaBeginUnloading()
	{
		if (!Game.Instance.IsUnloading)
		{
			m_IsUpdateForUnload = true;
			UpdateEtudes();
			m_IsUpdateForUnload = false;
		}
	}

	public BlueprintEtude GetConflictingGroupTask(BlueprintEtudeConflictingGroup conflictingGroup)
	{
		if (!conflictingGroup)
		{
			return null;
		}
		return m_HeldConflictingGroups.Get(conflictingGroup);
	}

	public void SetConflictingGroupTask(BlueprintEtudeConflictingGroup conflictingGroup, Etude e)
	{
		if ((bool)conflictingGroup)
		{
			if (e != null)
			{
				m_HeldConflictingGroups[conflictingGroup] = e.Blueprint;
			}
			else
			{
				m_HeldConflictingGroups.Remove(conflictingGroup);
			}
		}
	}

	public void StartEtude(BlueprintEtude bp, string source = "")
	{
		EtudeState savedState = GetSavedState(bp);
		string text = ((!string.IsNullOrEmpty(source)) ? (". From: " + source) : "");
		switch (savedState)
		{
		case EtudeState.Started:
			PFLog.Etudes.Log(bp, $"Cannot start etude {bp}: already started" + text);
			return;
		case EtudeState.Completed:
			PFLog.Etudes.Log(bp, $"Cannot start etude {bp}: already completed" + text);
			return;
		}
		if (!bp.Parent.IsEmpty())
		{
			switch (GetSavedState(bp.Parent.Get()))
			{
			case EtudeState.Unknown:
			case EtudeState.PreStarted:
				if (bp.StartsParent)
				{
					StartEtude(bp.Parent, "child etude " + bp.name + text);
					if (GetSavedState(bp) == EtudeState.Started)
					{
						return;
					}
				}
				if (GetSavedState(bp.Parent) != EtudeState.Started)
				{
					m_EtudesData[bp.ToReference<BlueprintEtudeReference>()] = EtudeState.PreStarted;
					GameHistoryLog.Instance.EtudeEvent(null, "Etude[" + bp.NameSafe() + "]:PreStarted");
					PFLog.Etudes.Log(bp, $"Starting etude {bp}: parent not started, marking prestart" + text);
					return;
				}
				break;
			case EtudeState.Completed:
			case EtudeState.PreCompleted:
				PFLog.Etudes.Log(bp, $"Cannot start etude {bp}: parent already completed" + text);
				return;
			}
		}
		StartEtudeInternal(bp, source);
	}

	public void StartEtudeImmediately(BlueprintEtude bp, string source = "")
	{
		StartEtude(bp, source);
		UpdateEtudes();
	}

	private void StartEtudeInternal(BlueprintEtude bp, string source = "")
	{
		string text = ((!string.IsNullOrEmpty(source)) ? (". From: " + source) : "");
		EtudeState savedState = GetSavedState(bp);
		if (savedState == EtudeState.Completed)
		{
			return;
		}
		PFLog.Etudes.Log("Starting etude: " + bp.name + text);
		if (Etudes.Get(bp) != null)
		{
			PFLog.Etudes.Error(bp, $"Cannot start etude {bp}: already started" + text);
			return;
		}
		Etude etude = (bp.Parent.IsEmpty() ? null : Etudes.Get(bp.Parent.Get()));
		if (etude != null && etude.CompletionInProgress)
		{
			PFLog.Etudes.Log(bp, $"Cannot start etude {bp}: parent is CompletionInProgress" + text);
			return;
		}
		Facts.Add(new Etude(bp, etude));
		if (savedState == EtudeState.PreCompleted)
		{
			MarkEtudeCompleted(bp, "EtudesSystem, because etude was PreCompleted");
			return;
		}
		m_EtudesData[bp.ToReference<BlueprintEtudeReference>()] = EtudeState.Started;
		foreach (BlueprintEtudeReference item in bp.StartsWith)
		{
			if (!item.IsEmpty())
			{
				StartEtudeInternal(item.Get(), "startsWith in " + bp.name + " " + bp.AssetGuid);
			}
		}
		foreach (KeyValuePair<BlueprintEtudeReference, EtudeState> item2 in m_EtudesData.Where((KeyValuePair<BlueprintEtudeReference, EtudeState> p) => p.Key.Get().Parent.Is(bp) && p.Value == EtudeState.PreStarted).ToTempList())
		{
			StartEtudeInternal(item2.Key, "parent " + bp.name + " " + bp.AssetGuid + ", as prestarted child");
		}
		MarkConditionsDirty();
	}

	public void MarkEtudeCompleted(BlueprintEtude bp, string source = "")
	{
		PFLog.Etudes.Log("Completing etude: " + bp.name + " " + ((!string.IsNullOrEmpty(source)) ? ("From: " + source) : string.Empty));
		Etude etude = Etudes.Get(bp);
		m_EtudesData[bp.ToReference<BlueprintEtudeReference>()] = ((etude == null) ? EtudeState.PreCompleted : EtudeState.Completed);
		if (bp.CompletesParent && !bp.Parent.IsEmpty())
		{
			MarkEtudeCompleted(bp.Parent.Get(), "child etude " + bp.name + " completes parent");
			return;
		}
		if (etude != null)
		{
			etude.MarkCompleted();
		}
		else
		{
			GameHistoryLog.Instance.EtudeEvent(null, "Etude[" + bp.NameSafe() + "]:PreCompleted");
		}
		MarkConditionsDirty();
	}

	public void InternalMarkCompleted(BlueprintEtude bp)
	{
		m_EtudesData[bp.ToReference<BlueprintEtudeReference>()] = EtudeState.Completed;
	}

	public void ForceUpdateEtudes(BlueprintAreaPart areaPartBeingLoaded)
	{
		m_AreaPartBeingLoaded = areaPartBeingLoaded;
		UpdateEtudes();
		m_AreaPartBeingLoaded = null;
	}

	public void UpdateEtudes()
	{
		PFLog.Etudes.Log("Updating etude system");
		ClearConditionsDirty();
		HashSet<BlueprintAreaMechanics> equals = (m_AreaPartBeingLoaded ? null : GetActiveAdditionalMechanics(Game.Instance.CurrentlyLoadedArea).ToHashSet());
		Etudes.MaybeDeactivateCompletedEtudes();
		Etudes.SelectPlayingEtudes();
		if (m_AreaPartBeingLoaded != null)
		{
			return;
		}
		if (!GetActiveAdditionalMechanics(Game.Instance.CurrentlyLoadedArea).ToHashSet().SetEquals(equals))
		{
			PFLog.Etudes.Log("Etude system causing mechanics reload");
			Game.ReloadAreaMechanic(clearFx: false);
			LoadingProcess.Instance.StartLoadingProcess("OnEtudesUpdate", delegate
			{
				EventBus.RaiseEvent(delegate(IEtudesUpdateHandler h)
				{
					h.OnEtudesUpdate();
				});
			}, LoadingProcessTag.ReloadMechanics);
		}
		else
		{
			EventBus.RaiseEvent(delegate(IEtudesUpdateHandler h)
			{
				h.OnEtudesUpdate();
			});
		}
	}

	public void FixupActorChanges()
	{
		foreach (KeyValuePair<BlueprintEtudeConflictingGroup, BlueprintEtude> item in m_HeldConflictingGroups.ToTempList())
		{
			if (!item.Value.ConflictingGroups.HasReference(item.Key))
			{
				SetConflictingGroupTask(item.Key, null);
				PFLog.Etudes.Log($"Fixed conflicting group {item.Key}: no longer held by {item.Value}");
			}
		}
		foreach (Etude rawFact in Etudes.RawFacts)
		{
			if (!rawFact.IsPlaying)
			{
				continue;
			}
			foreach (BlueprintEtudeConflictingGroupReference conflictingGroup in rawFact.Blueprint.ConflictingGroups)
			{
				BlueprintEtudeConflictingGroup blueprintEtudeConflictingGroup = conflictingGroup.Get();
				if (blueprintEtudeConflictingGroup != null)
				{
					BlueprintEtude conflictingGroupTask = GetConflictingGroupTask(blueprintEtudeConflictingGroup);
					if (conflictingGroupTask == null || conflictingGroupTask.Priority < rawFact.Blueprint.Priority)
					{
						PFLog.Etudes.Log($"Fixed conflicting group {blueprintEtudeConflictingGroup}: should be held by {rawFact} (@{rawFact.Blueprint.Priority})");
						SetConflictingGroupTask(blueprintEtudeConflictingGroup, rawFact);
					}
				}
			}
		}
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public void HandleUnlock(BlueprintUnlockableFlag flag)
	{
		MarkConditionsDirty();
	}

	public void HandleLock(BlueprintUnlockableFlag flag)
	{
		MarkConditionsDirty();
	}

	public void HandleFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		MarkConditionsDirty();
	}

	public void HandleRecruit()
	{
		MarkConditionsDirty();
	}

	public void HandleUnrecruit()
	{
		MarkConditionsDirty();
	}

	public void HandleAddCompanion()
	{
		MarkConditionsDirty();
	}

	public void HandleCompanionActivated()
	{
		MarkConditionsDirty();
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
		MarkConditionsDirty();
	}

	public void HandleCapitalModeChanged()
	{
	}

	public string GetDebugInfo(BlueprintEtude bp)
	{
		Etude etude = Etudes.Get(bp);
		EtudeState savedState = GetSavedState(bp);
		return $"[{bp.name}] is {savedState}: IsStarted={etude?.IsAttached} IsPlaying={etude?.IsPlaying} Completion={etude?.CompletionInProgress}";
	}

	public IEnumerable<BlueprintAreaMechanics> GetActiveAdditionalMechanics(BlueprintArea area)
	{
		foreach (Etude rawFact in Etudes.RawFacts)
		{
			if (!rawFact.IsPlaying)
			{
				continue;
			}
			foreach (BlueprintAreaMechanicsReference addedAreaMechanic in rawFact.Blueprint.AddedAreaMechanics)
			{
				if (!addedAreaMechanic.IsEmpty() && addedAreaMechanic.Get().Area.Is(area))
				{
					yield return addedAreaMechanic.Get();
				}
			}
		}
	}

	private IEnumerable<BlueprintEtude> GetEtudesByState(EtudeState state)
	{
		return from x in m_EtudesData
			where x.Value == state
			select x.Key.Get();
	}

	public IEnumerable<BlueprintEtude> GetStartedEtudes()
	{
		return GetEtudesByState(EtudeState.Started);
	}

	public IEnumerable<BlueprintEtude> GetCompletedEtudes()
	{
		return GetEtudesByState(EtudeState.Completed);
	}

	public void OnAreaDidLoad()
	{
	}

	public void HandleQuestObjectiveStarted(QuestObjective objective)
	{
		MarkConditionsDirty();
	}

	public void HandleQuestObjectiveBecameVisible(QuestObjective objective)
	{
		MarkConditionsDirty();
	}

	public void HandleQuestObjectiveCompleted(QuestObjective objective)
	{
		MarkConditionsDirty();
	}

	public void HandleQuestObjectiveFailed(QuestObjective objective)
	{
		MarkConditionsDirty();
	}

	public void OnTimeOfDayChanged()
	{
		MarkConditionsDirty();
	}

	public void HandleUnitAdded(ISummonPool pool)
	{
		MarkConditionsDirty();
	}

	public void HandleUnitRemoved(ISummonPool pool)
	{
		MarkConditionsDirty();
	}

	public void HandleLastUnitRemoved(ISummonPool pool)
	{
	}

	public void OnAreaPartChanged(BlueprintAreaPart previous)
	{
		m_AreaPartBeingExited = SimpleBlueprintExtendAsObject.Or(previous, Game.Instance.CurrentlyLoadedArea);
		UpdateEtudes();
		m_AreaPartBeingExited = null;
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		if (!Game.Instance.IsUnloading)
		{
			m_IsUpdateForUnload = true;
			UpdateEtudes();
			m_IsUpdateForUnload = false;
		}
	}

	public void OnAdditiveAreaDidActivated()
	{
	}

	public void HandleUnitJoinCombat()
	{
		MarkConditionsDirty();
	}

	public void HandleUnitLeaveCombat()
	{
		MarkConditionsDirty();
	}

	public void UnstartEtude(BlueprintEtude bp, bool markPreStarted = false)
	{
		if (bp.IsReadOnly)
		{
			PFLog.Default.Error($"Cannot unstart etude {this} as it is read-only.");
			return;
		}
		Etude etude = Etudes.Get(bp);
		if (etude == null)
		{
			m_EtudesData.Remove(bp.ToReference<BlueprintEtudeReference>());
		}
		else
		{
			foreach (Etude item in etude.Children.ToTempList())
			{
				UnstartEtude(item.Blueprint, markPreStarted: true);
			}
		}
		foreach (KeyValuePair<BlueprintEtudeReference, EtudeState> item2 in m_EtudesData.Where((KeyValuePair<BlueprintEtudeReference, EtudeState> p) => p.Value == EtudeState.Completed).ToTempList())
		{
			if (IsDescendant(bp, item2.Key))
			{
				m_EtudesData[item2.Key] = EtudeState.PreCompleted;
			}
		}
		if (etude != null)
		{
			Etudes.Remove(etude);
			m_EtudesData.Remove(bp.ToReference<BlueprintEtudeReference>());
		}
		if (markPreStarted)
		{
			m_EtudesData[bp.ToReference<BlueprintEtudeReference>()] = EtudeState.PreStarted;
		}
		static bool IsDescendant(BlueprintEtude a, BlueprintEtude b)
		{
			if (!b.Parent.Is(a))
			{
				if (!b.Parent.IsEmpty())
				{
					return IsDescendant(a, b.Parent);
				}
				return false;
			}
			return true;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = EtudesDataGameStateAdapter.GetHash128(m_EtudesData);
		result.Append(ref val2);
		Dictionary<BlueprintEtudeConflictingGroup, BlueprintEtude> heldConflictingGroups = m_HeldConflictingGroups;
		if (heldConflictingGroups != null)
		{
			int val3 = 0;
			foreach (KeyValuePair<BlueprintEtudeConflictingGroup, BlueprintEtude> item in heldConflictingGroups)
			{
				Hash128 hash = default(Hash128);
				Hash128 val4 = SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val4);
				Hash128 val5 = SimpleBlueprintHasher.GetHash128(item.Value);
				hash.Append(ref val5);
				val3 ^= hash.GetHashCode();
			}
			result.Append(ref val3);
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		EtudesSystem source = new EtudesSystem(default(OwlPackConstructorParameter));
		result = Unsafe.As<EtudesSystem, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<EtudesSystem>(OwlPackTypeInfo);
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
		Dictionary<BlueprintEtudeReference, EtudeState> value2 = m_EtudesData;
		formatter.Field(10, "m_EtudesData", ref value2, state);
		Dictionary<BlueprintEtudeConflictingGroup, BlueprintEtude> value3 = m_HeldConflictingGroups;
		formatter.Field(11, "m_HeldConflictingGroups", ref value3, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<EtudesSystem>();
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
				m_EtudesData = formatter.ReadPackable<Dictionary<BlueprintEtudeReference, EtudeState>>(state);
				break;
			case 11:
				m_HeldConflictingGroups = formatter.ReadPackable<Dictionary<BlueprintEtudeConflictingGroup, BlueprintEtude>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
