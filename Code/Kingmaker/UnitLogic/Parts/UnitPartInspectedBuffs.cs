using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Inspect;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartInspectedBuffs : BaseUnitPart, IHashable, IOwlPackable<UnitPartInspectedBuffs>
{
	[OwlPackable(OwlPackableMode.Generate)]
	public struct CasterInspectionInfo : IHashable, IOwlPackable, IOwlPackable<CasterInspectionInfo>
	{
		[JsonProperty]
		[OwlPackInclude]
		public EntityRef<MechanicEntity> Caster;

		[JsonProperty]
		[OwlPackInclude]
		public bool CheckPassed;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "CasterInspectionInfo",
			Fields = new FieldInfo[2]
			{
				new FieldInfo("Caster", typeof(EntityRef<MechanicEntity>)),
				new FieldInfo("CheckPassed", typeof(bool))
			}
		};

		public Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			EntityRef<MechanicEntity> obj = Caster;
			Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
			result.Append(ref val);
			result.Append(ref CheckPassed);
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			CasterInspectionInfo source = default(CasterInspectionInfo);
			result = Unsafe.As<CasterInspectionInfo, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<CasterInspectionInfo>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "Caster", ref Caster, state);
			formatter.UnmanagedField(1, "CheckPassed", ref CheckPassed, state);
			formatter.EndObject();
		}

		public void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state) where TFormatter : IInputFormatter
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<CasterInspectionInfo>();
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
					Caster = formatter.ReadPackable<EntityRef<MechanicEntity>>(state);
					break;
				case 1:
					CheckPassed = formatter.ReadUnmanaged<bool>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartInspectedBuffs",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("m_InspectedCasters", typeof(List<CasterInspectionInfo>))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private List<CasterInspectionInfo> m_InspectedCasters { get; set; } = new List<CasterInspectionInfo>();


	private bool MakeCheck(BaseUnitEntity caster)
	{
		InspectUnitsManager.UnitInfo info = Game.Instance.Player.InspectUnitsManager.GetInfo(caster.BlueprintForInspection);
		if (info == null)
		{
			return false;
		}
		int dC = info.DC;
		StatType statType = StatType.SkillTechUse;
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			if (item.LifeState.IsConscious && item.Actor.GetStatBase(statType) > 0 && GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(item, statType, dC, null, SkillCheckType.Inspect), null, allowPartyCheckInCamp: false).RollResult >= dC)
			{
				return true;
			}
		}
		return false;
	}

	public List<Buff> GetBuffs(UnitInspectInfoByPart inspectInfo)
	{
		List<Buff> list;
		if (inspectInfo != null)
		{
			inspectInfo.ActiveBuffsPart = new UnitInspectInfoByPart.ActiveBuffsPartData();
			list = inspectInfo.ActiveBuffsPart.ActiveBuffs;
		}
		else
		{
			list = new List<Buff>();
		}
		if (!InspectUnitsHelper.IsInspectAllow(base.Owner))
		{
			return null;
		}
		Dictionary<MechanicEntity, List<Buff>> dictionary = new Dictionary<MechanicEntity, List<Buff>>();
		foreach (Buff buff in base.Owner.Buffs)
		{
			if (buff.Name.Empty() || buff.Blueprint.IsHiddenInUI)
			{
				continue;
			}
			MechanicEntity maybeCaster = buff.Context.MaybeCaster;
			if (maybeCaster == null)
			{
				list.Add(buff);
				continue;
			}
			if (!dictionary.ContainsKey(maybeCaster))
			{
				dictionary.Add(maybeCaster, new List<Buff>());
			}
			dictionary[maybeCaster].Add(buff);
		}
		foreach (KeyValuePair<MechanicEntity, List<Buff>> buffList in dictionary)
		{
			if (!(buffList.Key is BaseUnitEntity baseUnitEntity))
			{
				continue;
			}
			bool flag = Game.Instance.Player.AllCharacters.Contains(baseUnitEntity);
			if (!flag)
			{
				if (m_InspectedCasters.FindIndex((CasterInspectionInfo info) => info.Caster == buffList.Key) == -1)
				{
					flag = true;
					m_InspectedCasters.Add(new CasterInspectionInfo
					{
						Caster = baseUnitEntity,
						CheckPassed = flag
					});
				}
				else
				{
					flag = true;
				}
			}
			if (flag)
			{
				list.AddRange(buffList.Value.ToArray());
			}
		}
		return list;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<CasterInspectionInfo> inspectedCasters = m_InspectedCasters;
		if (inspectedCasters != null)
		{
			for (int i = 0; i < inspectedCasters.Count; i++)
			{
				CasterInspectionInfo obj = inspectedCasters[i];
				Hash128 val2 = StructHasher<CasterInspectionInfo>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartInspectedBuffs source = new UnitPartInspectedBuffs();
		result = Unsafe.As<UnitPartInspectedBuffs, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartInspectedBuffs>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<CasterInspectionInfo> value = m_InspectedCasters;
		formatter.Field(0, "m_InspectedCasters", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartInspectedBuffs>();
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
				m_InspectedCasters = formatter.ReadPackable<List<CasterInspectionInfo>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
