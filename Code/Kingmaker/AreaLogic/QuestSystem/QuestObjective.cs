using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

[JsonObject(MemberSerialization.OptIn)]
[OwlPackable(OwlPackableMode.Generate)]
public class QuestObjective : EntityFact<QuestBook>, ITimedEvent, ISubscriber, IHashable, IOwlPackable<QuestObjective>
{
	[CanBeNull]
	private Quest m_Quest;

	[JsonProperty]
	[OwlPackInclude]
	private QuestObjectiveState m_State;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsVisible;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsCollapse;

	[JsonProperty]
	[OwlPackInclude]
	private TimeSpan m_ObjectiveStartTime;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private bool m_NeedToAttention;

	[JsonProperty]
	[OwlPackInclude]
	private bool m_IsViewed;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "QuestObjective",
		OldNames = null,
		Fields = new FieldInfo[15]
		{
			new FieldInfo("m_ComponentsData", typeof(Dictionary<string, List<IEntityFactComponentSavableData>>)),
			new FieldInfo("m_Components", typeof(List<EntityFactComponent>)),
			new FieldInfo("m_Sources", typeof(List<EntityFactSource>)),
			new FieldInfo("m_ChildrenFacts", typeof(List<EntityFactRef>)),
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_Blueprint", typeof(BlueprintFact)),
			new FieldInfo("IsActive", typeof(bool)),
			new FieldInfo("ChildOf", typeof(EntityFactRef)),
			new FieldInfo("m_State", typeof(QuestObjectiveState)),
			new FieldInfo("m_IsVisible", typeof(bool)),
			new FieldInfo("m_IsCollapse", typeof(bool)),
			new FieldInfo("m_ObjectiveStartTime", typeof(TimeSpan)),
			new FieldInfo("Order", typeof(int)),
			new FieldInfo("m_NeedToAttention", typeof(bool)),
			new FieldInfo("m_IsViewed", typeof(bool))
		}
	};

	[CanBeNull]
	public QuestObjective ParentObjective { get; private set; }

	public Quest Quest => m_Quest ?? (m_Quest = base.Manager?.GetNotFromCache<Quest>(Blueprint.Quest));

	[JsonProperty]
	[OwlPackInclude]
	public int Order { get; set; }

	public QuestObjectiveState State => m_State;

	public bool IsVisible
	{
		get
		{
			if (!Blueprint.IsHidden && m_State != 0)
			{
				return m_IsVisible;
			}
			return false;
		}
	}

	public new BlueprintQuestObjective Blueprint => (BlueprintQuestObjective)base.Blueprint;

	public TimeSpan? TimeToFail
	{
		get
		{
			if (Blueprint.AutoFailDays <= 0)
			{
				return null;
			}
			return m_ObjectiveStartTime.Add(TimeSpan.FromDays(Blueprint.AutoFailDays)) - Game.Instance.Player.GameTime;
		}
	}

	public bool NeedToAttention
	{
		get
		{
			if (!IsVisible)
			{
				m_NeedToAttention = false;
				return false;
			}
			if (m_NeedToAttention)
			{
				return true;
			}
			foreach (BlueprintQuestObjective addendum in Blueprint.Addendums)
			{
				QuestObjective questObjective = Quest.TryGetObjective(addendum);
				if (questObjective != null && questObjective.NeedToAttention)
				{
					return true;
				}
			}
			return false;
		}
		set
		{
			m_NeedToAttention = value;
		}
	}

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
				base.EventBus.RaiseEvent(delegate(ISetQuestObjectiveViewedHandler l)
				{
					l.HandleSetQuestObjectiveViewed(this);
				});
			}
		}
	}

	public bool IsCollapse
	{
		get
		{
			return m_IsCollapse;
		}
		set
		{
			m_IsCollapse = value;
		}
	}

	public QuestObjective(Quest quest, BlueprintQuestObjective blueprintQuestObjective)
		: base((BlueprintFact)blueprintQuestObjective)
	{
		base.SuppressActivationOnAttach = true;
		m_IsVisible = !blueprintQuestObjective.IsAddendum;
	}

	protected QuestObjective(JsonConstructorMark _)
	{
	}

	protected QuestObjective()
	{
	}

	public bool CanStart()
	{
		return Blueprint.ComponentsArray.OfType<IQuestObjectiveStartCondition>().All((IQuestObjectiveStartCondition condition) => condition.CanStart());
	}

	private void SetState(QuestObjectiveState state)
	{
		m_State = state;
		NeedToAttention = true;
	}

	public void Start()
	{
		if (Quest.State == QuestState.Completed || Quest.State == QuestState.Failed)
		{
			return;
		}
		SetState(QuestObjectiveState.Started);
		Activate();
		CallComponents(delegate(IQuestObjectiveLogic logic)
		{
			logic.OnStarted();
		});
		Quest.OnObjectiveStateChange(this);
		base.EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
		{
			l.HandleQuestObjectiveStarted(this);
		});
		TryBecameVisible();
		foreach (BlueprintQuestObjective addendum in Blueprint.Addendums)
		{
			QuestObjective questObjective = Quest.TryGetObjective(addendum);
			if (questObjective != null)
			{
				questObjective.ParentObjective = this;
				if (questObjective.State == QuestObjectiveState.None && questObjective.Blueprint.IsAutomaticallyStartingAddendum)
				{
					questObjective.Start();
				}
				questObjective.TryBecameVisible();
			}
		}
		m_ObjectiveStartTime = Game.Instance.Player.GameTime;
	}

	public void Complete()
	{
		if (Quest.State != QuestState.Completed && Quest.State != QuestState.Failed)
		{
			SetState(QuestObjectiveState.Completed);
			Experience.TryGain(Blueprint);
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnCompleted();
			});
			Deactivate();
			base.EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveCompleted(this);
			});
			Quest.OnObjectiveStateChange(this);
			Blueprint.CallComponents(delegate(IQuestObjectiveCallback c)
			{
				c.OnComplete();
			});
		}
	}

	public void Fail()
	{
		if (Quest.State != QuestState.Completed && Quest.State != QuestState.Failed)
		{
			SetState(QuestObjectiveState.Failed);
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnFailed();
			});
			Deactivate();
			base.EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveFailed(this);
			});
			Quest.OnObjectiveStateChange(this);
			Blueprint.CallComponents(delegate(IQuestObjectiveCallback c)
			{
				c.OnFail();
			});
		}
	}

	public void Reset()
	{
		m_IsVisible = !Blueprint.IsAddendum;
		m_State = QuestObjectiveState.None;
		NeedToAttention = false;
		if (base.Active)
		{
			Deactivate();
		}
	}

	public void HandleTimePassed()
	{
	}

	public void TryFailOnQuestFinished()
	{
		if (!Blueprint.IsAddendum && m_State == QuestObjectiveState.Started)
		{
			SetState(QuestObjectiveState.Failed);
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnFailed();
			});
			Deactivate();
			base.EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveFailed(this);
			});
			Blueprint.CallComponents(delegate(IQuestObjectiveCallback c)
			{
				c.OnFail();
			});
		}
	}

	protected override void OnPostLoad()
	{
		foreach (BlueprintQuestObjective addendum in Blueprint.Addendums)
		{
			QuestObjective questObjective = Quest.TryGetObjective(addendum);
			if (questObjective == null)
			{
				PFLog.Quests.Error("Error: Quest Objective " + Blueprint.name + " has a null addendum");
			}
			else
			{
				questObjective.ParentObjective = this;
			}
		}
		base.OnPostLoad();
	}

	private void TryBecameVisible()
	{
		if (!m_IsVisible && !Blueprint.IsHidden && ReadyToBecameVisible())
		{
			m_IsVisible = true;
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnBecameVisible();
			});
			base.EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveBecameVisible(this);
			});
		}
	}

	protected virtual bool ReadyToBecameVisible()
	{
		if (State == QuestObjectiveState.None)
		{
			return false;
		}
		if (ParentObjective != null && ParentObjective.State == QuestObjectiveState.None)
		{
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		string arg = (Blueprint.IsAddendum ? "Addendum" : "Objective");
		return $"{arg}#{State}#" + base.ToString();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_State);
		result.Append(ref m_IsVisible);
		result.Append(ref m_IsCollapse);
		result.Append(ref m_ObjectiveStartTime);
		int val2 = Order;
		result.Append(ref val2);
		result.Append(ref m_IsViewed);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		QuestObjective source = new QuestObjective();
		result = Unsafe.As<QuestObjective, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<QuestObjective>(OwlPackTypeInfo);
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
		formatter.EnumField(8, "m_State", ref m_State, state);
		formatter.UnmanagedField(9, "m_IsVisible", ref m_IsVisible, state);
		formatter.UnmanagedField(10, "m_IsCollapse", ref m_IsCollapse, state);
		formatter.Field(11, "m_ObjectiveStartTime", ref m_ObjectiveStartTime, state);
		int value5 = Order;
		formatter.UnmanagedField(12, "Order", ref value5, state);
		formatter.UnmanagedField(13, "m_NeedToAttention", ref m_NeedToAttention, state);
		formatter.UnmanagedField(14, "m_IsViewed", ref m_IsViewed, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<QuestObjective>();
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
				m_State = formatter.ReadEnum<QuestObjectiveState>(state);
				break;
			case 9:
				m_IsVisible = formatter.ReadUnmanaged<bool>(state);
				break;
			case 10:
				m_IsCollapse = formatter.ReadUnmanaged<bool>(state);
				break;
			case 11:
				m_ObjectiveStartTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 12:
				Order = formatter.ReadUnmanaged<int>(state);
				break;
			case 13:
				m_NeedToAttention = formatter.ReadUnmanaged<bool>(state);
				break;
			case 14:
				m_IsViewed = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
