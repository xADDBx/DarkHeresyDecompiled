using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[OwlPackable(OwlPackableMode.Generate)]
public class Etude : EntityFact<EtudesSystem>, IHashable, IOwlPackable<Etude>
{
	public new readonly List<Etude> Children = new List<Etude>();

	public readonly HashSet<Etude> ComplitionBlockers = new HashSet<Etude>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Etude",
		OldNames = null,
		Fields = new FieldInfo[12]
		{
			new FieldInfo("m_ComponentsData", typeof(Dictionary<string, List<IEntityFactComponentSavableData>>)),
			new FieldInfo("m_Components", typeof(List<EntityFactComponent>)),
			new FieldInfo("m_Sources", typeof(List<EntityFactSource>)),
			new FieldInfo("m_ChildrenFacts", typeof(List<EntityFactRef>)),
			new FieldInfo("UniqueId", typeof(string)),
			new FieldInfo("m_Blueprint", typeof(BlueprintFact)),
			new FieldInfo("IsActive", typeof(bool)),
			new FieldInfo("ChildOf", typeof(EntityFactRef)),
			new FieldInfo("IsCompleted", typeof(bool)),
			new FieldInfo("CompletionInProgress", typeof(bool)),
			new FieldInfo("ActivationTime", typeof(TimeSpan)),
			new FieldInfo("IsPaused", typeof(bool))
		}
	};

	public Etude Parent => (Etude)base.SourceFact;

	public new BlueprintEtude Blueprint => (BlueprintEtude)base.Blueprint;

	public bool IsPlaying => base.IsActive;

	[JsonProperty]
	[OwlPackInclude]
	public bool IsCompleted { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool CompletionInProgress { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public TimeSpan ActivationTime { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool IsPaused { get; private set; }

	public Etude(BlueprintEtude blueprint, [CanBeNull] Etude parent)
		: base((BlueprintFact)blueprint)
	{
		if (parent != null)
		{
			AddSource(parent);
		}
	}

	public Etude(JsonConstructorMark _)
	{
	}

	protected Etude()
	{
	}

	public void MarkCompleted()
	{
		if (CompletionInProgress)
		{
			return;
		}
		CompletionInProgress = true;
		foreach (Etude child in Children)
		{
			child.MarkCompleted();
		}
		GameCoreHistoryLog.Instance.EtudeEvent(null, ToString());
	}

	public void SetPaused(bool isPaused)
	{
		if (IsPaused != isPaused)
		{
			IsPaused = isPaused;
			base.Owner.MarkConditionsDirty();
		}
	}

	public void FinishCompletion()
	{
		if (!CompletionInProgress)
		{
			PFLog.Etudes.Error(Blueprint, $"Cannot complete etude {this}: complete not started");
			return;
		}
		if (IsPlaying)
		{
			PFLog.Etudes.Error(Blueprint, $"Cannot complete etude {this}: still playing");
			return;
		}
		if (IsCompleted)
		{
			PFLog.Etudes.Warning(Blueprint, $"Cannot complete etude {this}: already completed");
			return;
		}
		IsCompleted = true;
		Metrics.Etude.EtudeState(EtudeMetricsEvent.EtudeStates.Complete).Id(Blueprint.AssetGuid).Send();
		GameCoreHistoryLog.Instance.EtudeEvent(null, ToString());
	}

	protected override void OnActivate()
	{
		ActivationTime = Game.Instance.Controllers.TimeController.GameTime;
		base.Owner.MarkConditionsDirty();
		base.OnActivate();
		base.Owner.Facts.EnsureFactProcessor<EtudesTree>().AddToPlaying(this);
		base.Owner.Etudes.TryShutDownExclusionGroupsOf(Blueprint);
		PFLog.Etudes.Log("Etude playing: " + Blueprint.name);
		if (IsCompleted || CompletionInProgress)
		{
			PFLog.Etudes.Error(Blueprint, $"Cannot activate etude {this}: already completed");
			Deactivate();
		}
		Metrics.Etude.EtudeState(EtudeMetricsEvent.EtudeStates.Start).Id(Blueprint.AssetGuid).Send();
		GameCoreHistoryLog.Instance.EtudeEvent(null, ToString());
	}

	protected override void OnDeactivate()
	{
		base.Owner.MarkConditionsDirty();
		base.OnDeactivate();
		PFLog.Etudes.Log("Etude stopping: " + Blueprint.name);
		base.Owner.Facts.EnsureFactProcessor<EtudesTree>().RemoveFromPlaying(this);
		GameCoreHistoryLog.Instance.EtudeEvent(null, "Etude[" + Blueprint.NameSafe() + "]:stoping");
	}

	private string GetStatusString()
	{
		if (!IsCompleted)
		{
			if (!CompletionInProgress)
			{
				if (!IsPlaying)
				{
					if (!base.IsAttached)
					{
						return "None";
					}
					return "Started";
				}
				return "IsPlaying";
			}
			return "CompletionInProgress";
		}
		return "IsCompleted";
	}

	public override string ToString()
	{
		return "Etude[" + Blueprint.NameSafe() + "]:" + GetStatusString();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = IsCompleted;
		result.Append(ref val2);
		bool val3 = CompletionInProgress;
		result.Append(ref val3);
		TimeSpan val4 = ActivationTime;
		result.Append(ref val4);
		bool val5 = IsPaused;
		result.Append(ref val5);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Etude source = new Etude();
		result = Unsafe.As<Etude, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Etude>(OwlPackTypeInfo);
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
		bool value5 = IsCompleted;
		formatter.UnmanagedField(8, "IsCompleted", ref value5, state);
		bool value6 = CompletionInProgress;
		formatter.UnmanagedField(9, "CompletionInProgress", ref value6, state);
		TimeSpan value7 = ActivationTime;
		formatter.Field(10, "ActivationTime", ref value7, state);
		bool value8 = IsPaused;
		formatter.UnmanagedField(11, "IsPaused", ref value8, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Etude>();
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
				IsCompleted = formatter.ReadUnmanaged<bool>(state);
				break;
			case 9:
				CompletionInProgress = formatter.ReadUnmanaged<bool>(state);
				break;
			case 10:
				ActivationTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 11:
				IsPaused = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
