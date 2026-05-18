using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Inspect;

[OwlPackable(OwlPackableMode.Generate)]
public class InspectUnitsManager : IHashable, IOwlPackable, IOwlPackable<InspectUnitsManager>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class UnitInfo : IHashable, IOwlPackable, IOwlPackable<UnitInfo>
	{
		public const int MaxKnownPartsCount = 4;

		[JsonProperty]
		[OwlPackInclude]
		private UnitInfoPart m_Parts;

		[JsonProperty]
		[OwlPackInclude]
		private int m_CheckValue;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "UnitInfo",
			OldNames = null,
			Fields = new FieldInfo[4]
			{
				new FieldInfo("Blueprint", typeof(BlueprintUnit)),
				new FieldInfo("m_Parts", typeof(UnitInfoPart)),
				new FieldInfo("m_CheckValue", typeof(int)),
				new FieldInfo("HasUnviewedChange", typeof(bool))
			}
		};

		[JsonProperty]
		[OwlPackInclude]
		public BlueprintUnit Blueprint { get; private set; }

		[JsonProperty]
		[OwlPackInclude]
		public bool HasUnviewedChange { get; private set; }

		public int DC => 10;

		public int SuccessMargin => m_CheckValue - DC;

		public bool Success => SuccessMargin >= 0;

		public int KnownPartsCount
		{
			get
			{
				if (!Success)
				{
					return 0;
				}
				return Math.Min(4, 1 + SuccessMargin / 5);
			}
		}

		public bool IsAllPartsUnlocked => m_Parts == UnitInfoPart.All;

		public bool IsNothingUnlocked => m_Parts == UnitInfoPart.None;

		public int CurrentKnownPartsCount => (IsUnlocked(UnitInfoPart.Base) ? 1 : 0) + (IsUnlocked(UnitInfoPart.Defence) ? 1 : 0) + (IsUnlocked(UnitInfoPart.Offence) ? 1 : 0) + (IsUnlocked(UnitInfoPart.Abilities) ? 1 : 0);

		[JsonConstructor]
		public UnitInfo(BlueprintUnit blueprint)
		{
			Blueprint = blueprint;
		}

		public UnitInfo()
		{
		}

		public bool IsUnlocked(UnitInfoPart part)
		{
			return (m_Parts & part) != 0;
		}

		public void SetCheck(int check, BaseUnitEntity unit)
		{
			if (!IsAllPartsUnlocked)
			{
				int knownPartsCount = KnownPartsCount;
				m_CheckValue = Math.Max(m_CheckValue, check);
				if (knownPartsCount < KnownPartsCount)
				{
					HasUnviewedChange = true;
					TryUnlock(UnitInfoPart.Base, unit);
					TryUnlock(UnitInfoPart.Defence, unit);
					TryUnlock(UnitInfoPart.Offence, unit);
					TryUnlock(UnitInfoPart.Abilities, unit);
				}
			}
		}

		public void TryUnlock(UnitInfoPart part, BaseUnitEntity unit)
		{
			if (CurrentKnownPartsCount < KnownPartsCount && !IsUnlocked(part))
			{
				m_Parts |= part;
			}
		}

		public void MarkViewed()
		{
			HasUnviewedChange = false;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = SimpleBlueprintHasher.GetHash128(Blueprint);
			result.Append(ref val);
			result.Append(ref m_Parts);
			result.Append(ref m_CheckValue);
			bool val2 = HasUnviewedChange;
			result.Append(ref val2);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			UnitInfo source = new UnitInfo();
			result = Unsafe.As<UnitInfo, TPossiblyBase>(ref source);
		}

		public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
		{
			(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
			var (objectId, _) = orRegister;
			if (orRegister.isRef)
			{
				formatter.ObjectRef(objectId);
				return;
			}
			ushort type = state.TypeLibrary.RegisterType<UnitInfo>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			BlueprintUnit value = Blueprint;
			formatter.Field(0, "Blueprint", ref value, state);
			formatter.EnumField(1, "m_Parts", ref m_Parts, state);
			formatter.UnmanagedField(2, "m_CheckValue", ref m_CheckValue, state);
			bool value2 = HasUnviewedChange;
			formatter.UnmanagedField(3, "HasUnviewedChange", ref value2, state);
			formatter.EndObject();
		}

		public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitInfo>();
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
					Blueprint = formatter.ReadPackable<BlueprintUnit>(state);
					break;
				case 1:
					m_Parts = formatter.ReadEnum<UnitInfoPart>(state);
					break;
				case 2:
					m_CheckValue = formatter.ReadUnmanaged<int>(state);
					break;
				case 3:
					HasUnviewedChange = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InspectUnitsManager",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_UnitInfos", typeof(List<UnitInfo>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private List<UnitInfo> m_UnitInfos { get; set; } = new List<UnitInfo>();


	public IEnumerable<UnitInfo> UnitInfos => m_UnitInfos;

	[CanBeNull]
	public UnitInfo GetInfo(BlueprintUnit unit)
	{
		return m_UnitInfos.FirstItem((UnitInfo i) => i.Blueprint == unit);
	}

	[CanBeNull]
	public UnitInfo GetInfo(BaseUnitEntity unit)
	{
		return GetInfo(unit.BlueprintForInspection);
	}

	public static UnitInfo GetInfoForce(BaseUnitEntity unit)
	{
		return new UnitInfo(unit.BlueprintForInspection);
	}

	public bool TryMakeKnowledgeCheck(BaseUnitEntity unit)
	{
		bool num = InspectUnitsHelper.IsInspectAllow(unit);
		bool result = false;
		if (!num)
		{
			return result;
		}
		BlueprintUnit blueprintForInspection = unit.BlueprintForInspection;
		UnitInfo info = GetInfo(blueprintForInspection);
		if (info == null)
		{
			info = new UnitInfo(blueprintForInspection);
			m_UnitInfos.Add(info);
		}
		if (info.KnownPartsCount == 4)
		{
			return result;
		}
		int dC = info.DC;
		StatType statType = StatType.SkillLoreXenos;
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (!item.LifeState.IsConscious)
			{
				continue;
			}
			if (item.Actor.GetStatBase(statType) > 0)
			{
				RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(item, statType, dC, null, SkillCheckType.Inspect));
				if (rulePerformSkillCheck.ResultIsSuccess)
				{
					result = true;
				}
				info.SetCheck(rulePerformSkillCheck.RollResult, item);
			}
			if (info.IsAllPartsUnlocked)
			{
				break;
			}
		}
		EventBus.RaiseEvent(delegate(IKnowledgeHandler h)
		{
			h.HandleKnowledgeUpdated(info);
		});
		return result;
	}

	public bool TryMakeKnowledgeCheck(BaseUnitEntity unit, BaseUnitEntity inspector)
	{
		bool num = InspectUnitsHelper.IsInspectAllow(unit);
		bool result = false;
		if (!num)
		{
			return result;
		}
		BlueprintUnit blueprintForInspection = unit.BlueprintForInspection;
		UnitInfo info = GetInfo(blueprintForInspection);
		if (info == null)
		{
			info = new UnitInfo(blueprintForInspection);
			m_UnitInfos.Add(info);
		}
		int dC = info.DC;
		StatType statType = StatType.SkillLoreXenos;
		if (!inspector.LifeState.IsConscious)
		{
			return result;
		}
		if (inspector.Actor.GetStatBase(statType) > 0)
		{
			RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(inspector, statType, dC, null, SkillCheckType.Inspect));
			if (rulePerformSkillCheck.ResultIsSuccess)
			{
				result = true;
			}
			info.SetCheck(rulePerformSkillCheck.RollResult, inspector);
		}
		EventBus.RaiseEvent(delegate(IKnowledgeHandler h)
		{
			h.HandleKnowledgeUpdated(info);
		});
		return result;
	}

	public void ForceRevealUnitInfo(BaseUnitEntity unit)
	{
		if (!InspectUnitsHelper.IsInspectAllow(unit))
		{
			return;
		}
		BlueprintUnit blueprintForInspection = unit.BlueprintForInspection;
		UnitInfo info = GetInfo(blueprintForInspection);
		if (info == null)
		{
			info = new UnitInfo(blueprintForInspection);
			m_UnitInfos.Add(info);
		}
		if (info.KnownPartsCount != 4)
		{
			EventBus.RaiseEvent(delegate(IKnowledgeHandler h)
			{
				h.HandleKnowledgeUpdated(info);
			});
			info.SetCheck(100, Game.Instance.Player.MainCharacterEntity);
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<UnitInfo> unitInfos = m_UnitInfos;
		if (unitInfos != null)
		{
			for (int i = 0; i < unitInfos.Count; i++)
			{
				Hash128 val = ClassHasher<UnitInfo>.GetHash128(unitInfos[i]);
				result.Append(ref val);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InspectUnitsManager source = new InspectUnitsManager();
		result = Unsafe.As<InspectUnitsManager, TPossiblyBase>(ref source);
	}

	public virtual void Serialize<TFormatter>(TFormatter formatter, SerializerState state) where TFormatter : IOutputFormatter
	{
		(uint id, bool isRef) orRegister = state.References.GetOrRegister(this);
		var (objectId, _) = orRegister;
		if (orRegister.isRef)
		{
			formatter.ObjectRef(objectId);
			return;
		}
		ushort type = state.TypeLibrary.RegisterType<InspectUnitsManager>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<UnitInfo> value = m_UnitInfos;
		formatter.Field(0, "m_UnitInfos", ref value, state);
		formatter.EndObject();
	}

	public virtual void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InspectUnitsManager>();
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
				m_UnitInfos = formatter.ReadPackable<List<UnitInfo>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
