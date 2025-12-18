using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

[JsonObject(MemberSerialization.OptIn)]
[OwlPackable(OwlPackableMode.Generate)]
public class Quest : EntityFact<QuestBook>, IHashable, IOwlPackable<Quest>
{
	[GameStateInclude]
	private readonly Dictionary<BlueprintQuestObjective, QuestObjective> m_Objectives = new Dictionary<BlueprintQuestObjective, QuestObjective>();

	[JsonProperty]
	[OwlPackInclude]
	private int m_NextObjectiveOrder = 1;

	[JsonProperty]
	[OwlPackInclude]
	private QuestState m_State;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsInUiSelected;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsViewed;

	private bool m_IsSilentFailInProgress;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Quest",
		OldNames = null,
		Fields = new FieldInfo[13]
		{
			new FieldInfo("m_ComponentsData", typeof(Dictionary<string, List<IEntityFactComponentSavableData>>)),
			new FieldInfo("m_Components", typeof(List<EntityFactComponent>)),
			new FieldInfo("m_Sources", typeof(List<EntityFactSource>)),
			new FieldInfo("m_ChildrenFacts", typeof(List<EntityFactRef>)),
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_Blueprint", typeof(BlueprintFact)),
			new FieldInfo("IsActive", typeof(bool)),
			new FieldInfo("ChildOf", typeof(EntityFactRef)),
			new FieldInfo("m_NextObjectiveOrder", typeof(int)),
			new FieldInfo("m_State", typeof(QuestState)),
			new FieldInfo("m_IsInUiSelected", typeof(bool)),
			new FieldInfo("m_IsViewed", typeof(bool)),
			new FieldInfo("PersistentObjectives", typeof(List<QuestObjective>))
		}
	};

	public bool IsViewed
	{
		get
		{
			return m_IsViewed;
		}
		set
		{
			if (m_IsViewed != value)
			{
				m_IsViewed = value;
				EventBus.RaiseEvent(delegate(ISetQuestViewedHandler l)
				{
					l.HandleSetQuestViewed(this);
				});
			}
		}
	}

	[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
	[UsedImplicitly]
	[GameStateIgnore("This adapter uses LINQ, so original m_Objectives used for serialization")]
	[OwlPackInclude]
	private List<QuestObjective> PersistentObjectives
	{
		get
		{
			return m_Objectives.Select((KeyValuePair<BlueprintQuestObjective, QuestObjective> objective) => objective.Value).ToList();
		}
		set
		{
			m_Objectives.Clear();
			foreach (QuestObjective item in value)
			{
				if (item.Blueprint == null)
				{
					PFLog.Default.Warning("Failed to load objective blueprint. Quest: " + item.Blueprint);
				}
				else
				{
					m_Objectives.Add(item.Blueprint, item);
				}
			}
		}
	}

	public QuestState State
	{
		get
		{
			if (m_State != QuestState.Updated)
			{
				return m_State;
			}
			return QuestState.Started;
		}
	}

	public TimeSpan? TimeToFail
	{
		get
		{
			QuestState state = State;
			if (state == QuestState.Completed || state == QuestState.Failed)
			{
				return null;
			}
			IEnumerable<TimeSpan?> source = (from o in Objectives
				where o.IsVisible && o.State == QuestObjectiveState.Started && o.TimeToFail.HasValue
				select o.TimeToFail).ToList();
			if (source.Any())
			{
				return source.Min();
			}
			return null;
		}
	}

	public bool IsFakeFail
	{
		get
		{
			TimeSpan? timeToFail = TimeToFail;
			if (timeToFail.HasValue)
			{
				return Objectives.FirstOrDefault(delegate(QuestObjective o)
				{
					TimeSpan? timeToFail2 = o.TimeToFail;
					TimeSpan? timeSpan = timeToFail;
					if (timeToFail2.HasValue != timeSpan.HasValue)
					{
						return false;
					}
					return !timeToFail2.HasValue || timeToFail2.GetValueOrDefault() == timeSpan.GetValueOrDefault();
				})?.Blueprint.IsFakeFail ?? false;
			}
			return false;
		}
	}

	public bool IsInUiSelected
	{
		get
		{
			return m_IsInUiSelected;
		}
		set
		{
			m_IsInUiSelected = value;
		}
	}

	public bool NeedToAttention => Objectives.Any((QuestObjective o) => o.NeedToAttention && (o.ParentObjective?.NeedToAttention ?? true));

	public IEnumerable<QuestObjective> Objectives
	{
		get
		{
			foreach (KeyValuePair<BlueprintQuestObjective, QuestObjective> objective in m_Objectives)
			{
				yield return objective.Value;
			}
		}
	}

	public new BlueprintQuest Blueprint => (BlueprintQuest)base.Blueprint;

	public Quest(BlueprintQuest blueprintQuest)
		: base((BlueprintFact)blueprintQuest)
	{
		foreach (BlueprintQuestObjective allObjective in blueprintQuest.AllObjectives)
		{
			m_Objectives[allObjective] = new QuestObjective(this, allObjective);
		}
	}

	protected Quest(JsonConstructorMark _)
	{
	}

	protected Quest()
	{
	}

	public bool IsRelatesToCompanion(BlueprintUnit companion)
	{
		return Blueprint.GetComponents<QuestRelatesToCompanionStory>().Any((QuestRelatesToCompanionStory c) => c.Companion == companion);
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		foreach (KeyValuePair<BlueprintQuestObjective, QuestObjective> objective in m_Objectives)
		{
			base.Owner.Facts.Add(objective.Value);
		}
	}

	protected override void OnDetach()
	{
		foreach (KeyValuePair<BlueprintQuestObjective, QuestObjective> objective in m_Objectives)
		{
			base.Owner.Facts.Remove(objective.Value);
		}
		base.OnDetach();
	}

	protected override void OnPostLoad()
	{
		foreach (BlueprintQuestObjective allObjective in Blueprint.AllObjectives)
		{
			if (!m_Objectives.ContainsKey(allObjective))
			{
				QuestObjective questObjective2 = (m_Objectives[allObjective] = new QuestObjective(this, allObjective));
				QuestObjective fact = questObjective2;
				base.Owner.Facts.Add(fact);
			}
		}
	}

	protected override void OnDispose()
	{
		m_Objectives.Clear();
	}

	[CanBeNull]
	public QuestObjective TryGetObjective(BlueprintQuestObjective blueprintObjective)
	{
		if (blueprintObjective == null)
		{
			UberDebug.LogError("Error: Quest " + Blueprint.name + " has a null addendum");
			return null;
		}
		m_Objectives.TryGetValue(blueprintObjective, out var value);
		return value;
	}

	private QuestObjective GetObjective(BlueprintQuestObjective blueprintObjective)
	{
		return TryGetObjective(blueprintObjective) ?? throw new Exception("Can't find objective in quest");
	}

	public void OnObjectiveStateChange(QuestObjective objective)
	{
		if (m_State == QuestState.Started)
		{
			EventBus.RaiseEvent(delegate(IQuestHandler l)
			{
				l.HandleQuestUpdated(this);
			});
		}
		if (m_State == QuestState.None && objective.State != 0 && !objective.Blueprint.IsAddendum)
		{
			m_State = QuestState.Started;
			CallComponents(delegate(IQuestLogic logic)
			{
				logic.OnStarted();
			});
			EventBus.RaiseEvent(delegate(IQuestHandler l)
			{
				l.HandleQuestStarted(this);
			});
		}
		switch (objective.State)
		{
		case QuestObjectiveState.None:
			PFLog.Default.Error("Objective can't change state to None");
			break;
		case QuestObjectiveState.Started:
			objective.Order = m_NextObjectiveOrder++;
			break;
		case QuestObjectiveState.Completed:
			OnObjectiveFinished(objective);
			break;
		case QuestObjectiveState.Failed:
			OnObjectiveFinished(objective);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void OnObjectiveOpened(QuestObjective objective)
	{
		switch (objective.State)
		{
		case QuestObjectiveState.None:
			if (objective.CanStart())
			{
				objective.Start();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case QuestObjectiveState.Started:
		case QuestObjectiveState.Completed:
		case QuestObjectiveState.Failed:
			break;
		}
	}

	private void OnObjectiveFinished(QuestObjective objective)
	{
		bool flag = objective.State == QuestObjectiveState.Completed;
		bool flag2 = objective.Blueprint.NextObjectives.Empty();
		if (objective.Blueprint.IsFinishParent && (!flag || flag2))
		{
			QuestObjective parentObjective = objective.ParentObjective;
			if (objective.Blueprint.IsAddendum && parentObjective != null)
			{
				if (flag)
				{
					parentObjective.Complete();
				}
				else
				{
					parentObjective.Fail();
				}
			}
			else
			{
				OnQuestFinished(flag);
			}
		}
		else
		{
			if (!flag)
			{
				return;
			}
			foreach (BlueprintQuestObjective nextObjective in objective.Blueprint.NextObjectives)
			{
				QuestObjective objective2 = GetObjective(nextObjective);
				if (objective2 != null)
				{
					OnObjectiveOpened(objective2);
				}
			}
		}
	}

	public void FailQuest(bool doFailSilently)
	{
		if (doFailSilently)
		{
			using (new BlueprintQuest.SilentQuestNotificationOverride(Blueprint, QuestNotificationState.Failed))
			{
				m_IsSilentFailInProgress = true;
				OnQuestFinished(completed: false);
				m_IsSilentFailInProgress = false;
				return;
			}
		}
		OnQuestFinished(completed: false);
	}

	protected virtual void OnQuestFinished(bool completed)
	{
		m_State = (completed ? QuestState.Completed : QuestState.Failed);
		if (completed)
		{
			Experience.TryGain(Blueprint);
			CallComponents(delegate(IQuestLogic logic)
			{
				logic.OnCompleted();
			});
			EventBus.RaiseEvent(delegate(IQuestHandler l)
			{
				l.HandleQuestCompleted(this);
			});
		}
		else
		{
			CallComponents(delegate(IQuestLogic logic)
			{
				logic.OnFailed();
			});
			EventBus.RaiseEvent(delegate(IQuestHandler l)
			{
				l.HandleQuestFailed(this);
			});
		}
		m_Objectives.ForEach(delegate(KeyValuePair<BlueprintQuestObjective, QuestObjective> pair)
		{
			QuestObjective value = pair.Value;
			if (m_IsSilentFailInProgress)
			{
				using (new BlueprintQuestObjective.SilentQuestNotificationOverride(value.Blueprint, QuestNotificationState.Failed))
				{
					value.TryFailOnQuestFinished();
					return;
				}
			}
			value.TryFailOnQuestFinished();
		});
	}

	public void Uncomplete(BlueprintQuestObjective makeStarted, IEnumerable<BlueprintQuestObjective> remove)
	{
		m_State = QuestState.Started;
		foreach (BlueprintQuestObjective item in remove)
		{
			m_Objectives.Get(item)?.Reset();
		}
		QuestObjective questObjective = m_Objectives.Get(makeStarted);
		questObjective.Reset();
		questObjective.Start();
	}

	public void Remove()
	{
		m_State = QuestState.None;
		foreach (BlueprintQuestObjective objective in Blueprint.Objectives)
		{
			m_Objectives.Get(objective)?.Reset();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<BlueprintQuestObjective, QuestObjective> objectives = m_Objectives;
		if (objectives != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintQuestObjective, QuestObjective> item in objectives)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<QuestObjective>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		result.Append(ref m_NextObjectiveOrder);
		result.Append(ref m_State);
		result.Append(ref m_IsInUiSelected);
		result.Append(ref m_IsViewed);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Quest source = new Quest();
		result = Unsafe.As<Quest, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Quest>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_ComponentsData", ref m_ComponentsData, state);
		List<EntityFactComponent> value = base.m_Components;
		formatter.Field(1, "m_Components", ref value, state);
		formatter.Field(2, "m_Sources", ref m_Sources, state);
		formatter.Field(3, "m_ChildrenFacts", ref m_ChildrenFacts, state);
		string value2 = base.UniqueId;
		formatter.StringField(4, "UniqueId", ref value2, state);
		formatter.Field(5, "m_Blueprint", ref m_Blueprint, state);
		bool value3 = base.IsActive;
		formatter.UnmanagedField(6, "IsActive", ref value3, state);
		EntityFactRef value4 = base.ChildOf;
		formatter.Field(7, "ChildOf", ref value4, state);
		formatter.UnmanagedField(8, "m_NextObjectiveOrder", ref m_NextObjectiveOrder, state);
		formatter.EnumField(9, "m_State", ref m_State, state);
		formatter.UnmanagedField(10, "m_IsInUiSelected", ref m_IsInUiSelected, state);
		formatter.UnmanagedField(11, "m_IsViewed", ref m_IsViewed, state);
		List<QuestObjective> value5 = PersistentObjectives;
		formatter.Field(12, "PersistentObjectives", ref value5, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Quest>();
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
				m_ComponentsData = formatter.ReadPackable<Dictionary<string, List<IEntityFactComponentSavableData>>>(state);
				break;
			case 1:
				base.m_Components = formatter.ReadPackable<List<EntityFactComponent>>(state);
				break;
			case 2:
				m_Sources = formatter.ReadPackable<List<EntityFactSource>>(state);
				break;
			case 3:
				m_ChildrenFacts = formatter.ReadPackable<List<EntityFactRef>>(state);
				break;
			case 4:
				base.UniqueId = formatter.ReadString(state);
				break;
			case 5:
				m_Blueprint = formatter.ReadPackable<BlueprintFact>(state);
				break;
			case 6:
				base.IsActive = formatter.ReadUnmanaged<bool>(state);
				break;
			case 7:
				base.ChildOf = formatter.ReadPackable<EntityFactRef>(state);
				break;
			case 8:
				m_NextObjectiveOrder = formatter.ReadUnmanaged<int>(state);
				break;
			case 9:
				m_State = formatter.ReadEnum<QuestState>(state);
				break;
			case 10:
				m_IsInUiSelected = formatter.ReadUnmanaged<bool>(state);
				break;
			case 11:
				m_IsViewed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 12:
				PersistentObjectives = formatter.ReadPackable<List<QuestObjective>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
