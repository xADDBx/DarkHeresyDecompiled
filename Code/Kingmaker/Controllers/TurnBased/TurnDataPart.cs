using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking.Serialization;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Groups;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

[OwlPackable(OwlPackableMode.Generate)]
public class TurnDataPart : EntityPart, IHashable, IOwlPackable<TurnDataPart>
{
	[JsonProperty]
	[OwlPackInclude]
	private int m_GameRound = 1;

	[JsonProperty]
	[GameStateIgnore]
	[OwlPackInclude]
	private UnitGroup[] m_Groups = Array.Empty<UnitGroup>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "TurnDataPart",
		OldNames = null,
		Fields = new FieldInfo[8]
		{
			new FieldInfo("InCombat", typeof(bool)),
			new FieldInfo("EndTurnRequested", typeof(bool)),
			new FieldInfo("IsUltimateAbilityUsedThisRound", typeof(bool)),
			new FieldInfo("LastTurnTime", typeof(TimeSpan)),
			new FieldInfo("m_GameRound", typeof(int)),
			new FieldInfo("CombatRound", typeof(int)),
			new FieldInfo("m_Groups", typeof(UnitGroup[])),
			new FieldInfo("TurnOrder", typeof(TurnOrderQueue))
		}
	};

	[JsonProperty(PropertyName = "m_TbActive")]
	[OwlPackInclude]
	public bool InCombat { get; set; }

	[JsonProperty(PropertyName = "m_EndTurnRequested")]
	[OwlPackInclude]
	public bool EndTurnRequested { get; set; }

	[JsonProperty(PropertyName = "m_IsUltimateAbilityUsedThisRound")]
	[OwlPackInclude]
	public bool IsUltimateAbilityUsedThisRound { get; set; }

	[JsonProperty(PropertyName = "m_LastTurnTime")]
	[OwlPackInclude]
	public TimeSpan LastTurnTime { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public int CombatRound { get; set; }

	[JsonProperty]
	[OwlPackInclude]
	public TurnOrderQueue TurnOrder { get; private set; } = new TurnOrderQueue();


	public int GameRound
	{
		get
		{
			return m_GameRound;
		}
		set
		{
			if (m_GameRound > value)
			{
				PFLog.System.ErrorWithReport("Wow! Current round index is less than next round index (overflow maybe)");
			}
			m_GameRound = Math.Max(0, value);
		}
	}

	protected override void OnPreSave()
	{
		m_Groups = Game.Instance.UnitGroups.Where((UnitGroup i) => i.IsInCombat).ToArray();
	}

	protected override void OnPostLoad()
	{
		UnitGroup[] groups = m_Groups;
		foreach (UnitGroup group in groups)
		{
			Game.Instance.UnitGroups.RestoreGroup(group);
		}
		m_Groups = Array.Empty<UnitGroup>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = InCombat;
		result.Append(ref val2);
		bool val3 = EndTurnRequested;
		result.Append(ref val3);
		bool val4 = IsUltimateAbilityUsedThisRound;
		result.Append(ref val4);
		TimeSpan val5 = LastTurnTime;
		result.Append(ref val5);
		result.Append(ref m_GameRound);
		int val6 = CombatRound;
		result.Append(ref val6);
		Hash128 val7 = ClassHasher<TurnOrderQueue>.GetHash128(TurnOrder);
		result.Append(ref val7);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		TurnDataPart source = new TurnDataPart();
		result = Unsafe.As<TurnDataPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<TurnDataPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		bool value = InCombat;
		formatter.UnmanagedField(0, "InCombat", ref value, state);
		bool value2 = EndTurnRequested;
		formatter.UnmanagedField(1, "EndTurnRequested", ref value2, state);
		bool value3 = IsUltimateAbilityUsedThisRound;
		formatter.UnmanagedField(2, "IsUltimateAbilityUsedThisRound", ref value3, state);
		TimeSpan value4 = LastTurnTime;
		formatter.Field(3, "LastTurnTime", ref value4, state);
		formatter.UnmanagedField(4, "m_GameRound", ref m_GameRound, state);
		int value5 = CombatRound;
		formatter.UnmanagedField(5, "CombatRound", ref value5, state);
		formatter.Field(6, "m_Groups", ref m_Groups, state);
		TurnOrderQueue value6 = TurnOrder;
		formatter.Field(7, "TurnOrder", ref value6, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<TurnDataPart>();
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
				InCombat = formatter.ReadUnmanaged<bool>(state);
				break;
			case 1:
				EndTurnRequested = formatter.ReadUnmanaged<bool>(state);
				break;
			case 2:
				IsUltimateAbilityUsedThisRound = formatter.ReadUnmanaged<bool>(state);
				break;
			case 3:
				LastTurnTime = formatter.ReadPackable<TimeSpan>(state);
				break;
			case 4:
				m_GameRound = formatter.ReadUnmanaged<int>(state);
				break;
			case 5:
				CombatRound = formatter.ReadUnmanaged<int>(state);
				break;
			case 6:
				m_Groups = formatter.ReadPackable<UnitGroup[]>(state);
				break;
			case 7:
				TurnOrder = formatter.ReadPackable<TurnOrderQueue>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
