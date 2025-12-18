using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

[JsonObject(MemberSerialization.OptIn)]
[OwlPackable(OwlPackableMode.Generate)]
public class QuestBook : Entity, IHashable, IOwlPackable<QuestBook>
{
	public const string ID = "quest-book-id";

	public new static readonly EntityRef<QuestBook> Ref = new EntityRef<QuestBook>("quest-book-id");

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "QuestBook",
		OldNames = null,
		Fields = new FieldInfo[10]
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
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?))
		}
	};

	public IEnumerable<Quest> Quests => from q in Facts.GetAllNotFromCache<Quest>()
		where q.State != QuestState.None
		select q;

	protected QuestBook(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	public QuestBook()
		: base("quest-book-id", isInGame: true)
	{
	}

	public void GiveObjective(BlueprintQuestObjective bpObjective)
	{
		QuestObjective questObjective = EnsureObjective(bpObjective);
		if (questObjective.State != QuestObjectiveState.Started)
		{
			if (questObjective.State != 0)
			{
				PFLog.Default.Warning("Quest objective has invalid state");
				return;
			}
			questObjective.Start();
			Metrics.Quest.Id(questObjective.Blueprint.AssetGuid).State(QuestMetricsEvent.States.Started).Send();
		}
	}

	public void CompleteObjective(BlueprintQuestObjective bpObjective)
	{
		QuestObjective questObjective = EnsureObjective(bpObjective);
		if (questObjective.State == QuestObjectiveState.None)
		{
			questObjective.Start();
		}
		if (questObjective.State != QuestObjectiveState.Started)
		{
			PFLog.Default.Warning("Quest objective has invalid state");
			return;
		}
		questObjective.Complete();
		Metrics.Quest.Id(questObjective.Blueprint.AssetGuid).State(QuestMetricsEvent.States.Completed).Send();
	}

	public void FailObjective(BlueprintQuestObjective bpObjective)
	{
		QuestObjective questObjective = EnsureObjective(bpObjective);
		if (questObjective.State == QuestObjectiveState.None)
		{
			questObjective.Start();
		}
		if (questObjective.State != QuestObjectiveState.Started)
		{
			PFLog.Default.Warning("Quest objective has invalid state");
			return;
		}
		questObjective.Fail();
		Metrics.Quest.Id(questObjective.Blueprint.AssetGuid).State(QuestMetricsEvent.States.Failed).Send();
	}

	public void ResetObjective(BlueprintQuestObjective bpObjective)
	{
		QuestObjective questObjective = EnsureObjective(bpObjective);
		if (questObjective.State != 0)
		{
			questObjective.Reset();
		}
	}

	[CanBeNull]
	private Quest GetQuestInternal(BlueprintQuest quest)
	{
		foreach (EntityFact item in Facts.List)
		{
			if (item is Quest quest2 && quest2.Blueprint == quest)
			{
				return quest2;
			}
		}
		return null;
	}

	public Quest GetQuest(BlueprintQuest quest)
	{
		return GetQuestInternal(quest);
	}

	public QuestState GetQuestState(BlueprintQuest bpQuest)
	{
		return GetQuestInternal(bpQuest)?.State ?? QuestState.None;
	}

	public QuestObjectiveState GetObjectiveState(BlueprintQuestObjective bpObjective)
	{
		Quest questInternal = GetQuestInternal(bpObjective.Quest);
		if (questInternal == null)
		{
			return QuestObjectiveState.None;
		}
		return (questInternal.TryGetObjective(bpObjective) ?? throw new Exception("Can't find objective in quest")).State;
	}

	[CanBeNull]
	public QuestObjective GetObjective(BlueprintQuestObjective bpObjective)
	{
		Quest questInternal = GetQuestInternal(bpObjective.Quest);
		if (questInternal == null)
		{
			return null;
		}
		QuestObjective questObjective = questInternal.TryGetObjective(bpObjective);
		if (questObjective == null)
		{
			PFLog.Default.Error("Objective not found");
		}
		return questObjective;
	}

	private QuestObjective EnsureObjective(BlueprintQuestObjective bpObjective)
	{
		Quest quest = GetQuestInternal(bpObjective.Quest);
		if (quest == null)
		{
			quest = BlueprintQuest.CreateNewQuest(bpObjective.Quest);
			Facts.Add(quest);
		}
		return quest.TryGetObjective(bpObjective) ?? throw new Exception("Can't find objective in quest");
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public void ResetQuest(BlueprintQuest bp, BlueprintQuestObjective start, IEnumerable<BlueprintQuestObjective> reset)
	{
		if ((bool)start)
		{
			GetQuest(bp)?.Uncomplete(start, reset);
		}
		else
		{
			GetQuest(bp)?.Remove();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		QuestBook source = new QuestBook(default(OwlPackConstructorParameter));
		result = Unsafe.As<QuestBook, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<QuestBook>(OwlPackTypeInfo);
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
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<QuestBook>();
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
			}
		}
		formatter.LeaveObject();
	}
}
