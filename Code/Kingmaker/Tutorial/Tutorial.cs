using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.Settings;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial;

[OwlPackable(OwlPackableMode.Generate)]
public class Tutorial : EntityFact<TutorialSystem>, IHashable, IOwlPackable<Tutorial>
{
	private bool m_Enabled;

	private readonly CountableFlag m_EnabledByEtude = new CountableFlag();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "Tutorial",
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
			new FieldInfo("TriggeredTimes", typeof(int)),
			new FieldInfo("ShowedTimes", typeof(int)),
			new FieldInfo("LastShowIndex", typeof(int)),
			new FieldInfo("TriggerLogicCounter", typeof(int)),
			new FieldInfo("Banned", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public int TriggeredTimes { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int ShowedTimes { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int LastShowIndex { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int TriggerLogicCounter { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public bool Banned { get; set; }

	public new BlueprintTutorial Blueprint => (BlueprintTutorial)base.Blueprint;

	public bool HasTrigger => Blueprint.GetComponent<TutorialTrigger>() != null;

	public override bool IsEnabled => m_Enabled;

	public bool IsLimitReached
	{
		get
		{
			if (ShowedTimes >= Blueprint.Limit)
			{
				return Blueprint.Limit == 0;
			}
			return true;
		}
	}

	public Tutorial(BlueprintTutorial fact)
		: base((BlueprintFact)fact)
	{
	}

	protected Tutorial()
	{
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		UpdateIsEnabled();
	}

	public bool IsBanned(bool fromTrigger)
	{
		if (!(HasTrigger && fromTrigger))
		{
			return base.Owner.IsTagBanned(Blueprint.Tag);
		}
		if (!Banned)
		{
			return !SettingsRoot.Game.Tutorial.ShowContextTutorial;
		}
		return true;
	}

	public void UpdateIsEnabled()
	{
		m_Enabled = (bool)m_EnabledByEtude && IsLimitReached && !IsBanned(fromTrigger: true);
		UpdateIsActive();
	}

	public void EnableByEtude()
	{
		m_EnabledByEtude.Retain();
		UpdateIsEnabled();
	}

	public void DisableByEtude()
	{
		m_EnabledByEtude.Release();
		UpdateIsEnabled();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		int val2 = TriggeredTimes;
		result.Append(ref val2);
		int val3 = ShowedTimes;
		result.Append(ref val3);
		int val4 = LastShowIndex;
		result.Append(ref val4);
		int val5 = TriggerLogicCounter;
		result.Append(ref val5);
		bool val6 = Banned;
		result.Append(ref val6);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		Tutorial source = new Tutorial();
		result = Unsafe.As<Tutorial, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<Tutorial>(OwlPackTypeInfo);
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
		int value5 = TriggeredTimes;
		formatter.UnmanagedField(8, "TriggeredTimes", ref value5, state);
		int value6 = ShowedTimes;
		formatter.UnmanagedField(9, "ShowedTimes", ref value6, state);
		int value7 = LastShowIndex;
		formatter.UnmanagedField(10, "LastShowIndex", ref value7, state);
		int value8 = TriggerLogicCounter;
		formatter.UnmanagedField(11, "TriggerLogicCounter", ref value8, state);
		bool value9 = Banned;
		formatter.UnmanagedField(12, "Banned", ref value9, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<Tutorial>();
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
				TriggeredTimes = formatter.ReadUnmanaged<int>(state);
				break;
			case 9:
				ShowedTimes = formatter.ReadUnmanaged<int>(state);
				break;
			case 10:
				LastShowIndex = formatter.ReadUnmanaged<int>(state);
				break;
			case 11:
				TriggerLogicCounter = formatter.ReadUnmanaged<int>(state);
				break;
			case 12:
				Banned = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
