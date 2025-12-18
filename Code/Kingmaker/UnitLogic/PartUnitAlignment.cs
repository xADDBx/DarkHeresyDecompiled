using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[OwlPackable(OwlPackableMode.Generate)]
public class PartUnitAlignment : BaseUnitPart, IAlignmentRankShiftHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable, IOwlPackable<PartUnitAlignment>
{
	public interface IOwner : IEntityPartOwner<PartUnitAlignment>, IEntityPartOwner
	{
		PartUnitAlignment Alignment { get; }
	}

	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<AlignmentAxis, int> m_AxisToMarkMap = new Dictionary<AlignmentAxis, int>
	{
		{
			AlignmentAxis.Monodominance,
			0
		},
		{
			AlignmentAxis.Xanthite,
			0
		},
		{
			AlignmentAxis.Xenophilia,
			0
		},
		{
			AlignmentAxis.Torian,
			0
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private Dictionary<AlignmentAxis, int> m_AxisToRankMap = new Dictionary<AlignmentAxis, int>
	{
		{
			AlignmentAxis.Monodominance,
			0
		},
		{
			AlignmentAxis.Xanthite,
			0
		},
		{
			AlignmentAxis.Xenophilia,
			0
		},
		{
			AlignmentAxis.Torian,
			0
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private AlignmentMix m_Mix;

	[JsonProperty]
	[OwlPackInclude]
	public List<AlignmentShiftHistoryEntry> ShiftHistory = new List<AlignmentShiftHistoryEntry>();

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitAlignment",
		OldNames = null,
		Fields = new FieldInfo[4]
		{
			new FieldInfo("m_AxisToMarkMap", typeof(Dictionary<AlignmentAxis, int>)),
			new FieldInfo("m_AxisToRankMap", typeof(Dictionary<AlignmentAxis, int>)),
			new FieldInfo("m_Mix", typeof(AlignmentMix)),
			new FieldInfo("ShiftHistory", typeof(List<AlignmentShiftHistoryEntry>))
		}
	};

	public bool CanHaveMarkInAxis(AlignmentAxis axis, int mark)
	{
		if (mark >= 4 && m_AxisToMarkMap.Any((KeyValuePair<AlignmentAxis, int> x) => x.Key != axis && x.Value >= 4))
		{
			return false;
		}
		switch (axis)
		{
		case AlignmentAxis.Monodominance:
			if (mark >= 2)
			{
				return m_AxisToMarkMap[AlignmentAxis.Torian] < 2;
			}
			break;
		case AlignmentAxis.Torian:
			if (mark >= 2)
			{
				return m_AxisToMarkMap[AlignmentAxis.Monodominance] < 2;
			}
			break;
		case AlignmentAxis.Xanthite:
			if (mark >= 2)
			{
				return m_AxisToMarkMap[AlignmentAxis.Xenophilia] < 2;
			}
			break;
		case AlignmentAxis.Xenophilia:
			if (mark >= 2)
			{
				return m_AxisToMarkMap[AlignmentAxis.Xanthite] < 2;
			}
			break;
		}
		return true;
	}

	public AlignmentMix GetAlignmentMix()
	{
		if (m_AxisToMarkMap[AlignmentAxis.Monodominance] >= 3 && m_AxisToMarkMap[AlignmentAxis.Xanthite] >= 3)
		{
			return AlignmentMix.XanthiteMonodominance;
		}
		if (m_AxisToMarkMap[AlignmentAxis.Monodominance] >= 3 && m_AxisToMarkMap[AlignmentAxis.Xenophilia] >= 3)
		{
			return AlignmentMix.XenophiliaMonodominance;
		}
		if (m_AxisToMarkMap[AlignmentAxis.Torian] >= 3 && m_AxisToMarkMap[AlignmentAxis.Xanthite] >= 3)
		{
			return AlignmentMix.XanthiteTorian;
		}
		if (m_AxisToMarkMap[AlignmentAxis.Torian] >= 3 && m_AxisToMarkMap[AlignmentAxis.Xenophilia] >= 3)
		{
			return AlignmentMix.XenophiliaTorian;
		}
		return AlignmentMix.None;
	}

	public void HandleAlignmentRankShift(AlignmentShift shift)
	{
	}

	private void TryUpdateMix()
	{
		if (m_Mix != 0)
		{
			return;
		}
		AlignmentMix alignmentMix = GetAlignmentMix();
		if (m_Mix != alignmentMix)
		{
			m_Mix = alignmentMix;
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IAlignmentReachMixHandler>)delegate(IAlignmentReachMixHandler h)
			{
				h.HandleAlignmentReachedMix(m_Mix);
			}, isCheckRuntime: true);
		}
	}

	public int GetAlignmentMark(AlignmentAxis axis)
	{
		return m_AxisToMarkMap[axis];
	}

	public int GetAlignmentRank(AlignmentAxis axis)
	{
		return m_AxisToRankMap[axis];
	}

	public void SetMark(AlignmentAxis axis, int mark, BlueprintScriptableObject source)
	{
		if (CanHaveMarkInAxis(axis, mark))
		{
			int rankForMark = ConfigRoot.Instance.AlignmentMarksRoot.GetRankForMark(axis, mark);
			AlignmentShiftExtension.ApplyShiftTo(new AlignmentShift
			{
				Axis = axis,
				Value = rankForMark - GetAlignmentRank(axis)
			}, base.Owner, source);
		}
	}

	public void ApplyShift(AlignmentShift shift, BlueprintScriptableObject source)
	{
		AlignmentShiftHistoryEntry alignmentShiftHistoryEntry = new AlignmentShiftHistoryEntry
		{
			Axis = shift.Axis,
			Rank = shift.Value,
			Source = source
		};
		PFLog.Alignment.Log($"{base.Owner} had alignment shift: {shift.Axis} {shift.Value}");
		Metrics.Alignment.Type(shift.Axis).Value(shift.Value).CharacterLevel(base.Owner.Progression.CharacterLevel)
			.Send();
		int num = GetAlignmentRank(shift.Axis) + shift.Value;
		int alignmentRank = GetAlignmentRank(shift.Axis);
		m_AxisToRankMap[shift.Axis] = num;
		int currentMark = ConfigRoot.Instance.AlignmentMarksRoot.GetMarkForRank(shift.Axis, num);
		int markForRank = ConfigRoot.Instance.AlignmentMarksRoot.GetMarkForRank(shift.Axis, alignmentRank);
		if (currentMark != markForRank && CanHaveMarkInAxis(shift.Axis, currentMark))
		{
			alignmentShiftHistoryEntry.AchievedNewMark = true;
			List<BlueprintMechanicEntityFact> factsOnMark = ConfigRoot.Instance.AlignmentMarksRoot.GetFactsOnMark(shift.Axis, currentMark);
			m_AxisToMarkMap[shift.Axis] = currentMark;
			PFLog.Alignment.Log($"{base.Owner} reach alignment Mark: {shift.Axis} {currentMark}");
			alignmentShiftHistoryEntry.NewFacts = factsOnMark;
			foreach (BlueprintMechanicEntityFact item in factsOnMark)
			{
				if (!base.Owner.Facts.Contains(item))
				{
					EntityFact entityFact = base.Owner.AddFact(item);
					if (entityFact == null)
					{
						break;
					}
					entityFact.AddSource(ConfigRoot.Instance.AlignmentMarksRoot, currentMark);
				}
			}
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IAlignmentReachMarkHandler>)delegate(IAlignmentReachMarkHandler h)
			{
				h.HandleAlignmentMarkShift(shift.Axis, currentMark);
			}, isCheckRuntime: true);
		}
		ShiftHistory.Add(alignmentShiftHistoryEntry);
		TryUpdateMix();
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IAlignmentRankShiftHandler>)delegate(IAlignmentRankShiftHandler h)
		{
			h.HandleAlignmentRankShift(shift);
		}, isCheckRuntime: true);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<AlignmentAxis, int> axisToMarkMap = m_AxisToMarkMap;
		if (axisToMarkMap != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<AlignmentAxis, int> item in axisToMarkMap)
			{
				Hash128 hash = default(Hash128);
				AlignmentAxis obj = item.Key;
				Hash128 val3 = UnmanagedHasher<AlignmentAxis>.GetHash128(ref obj);
				hash.Append(ref val3);
				int obj2 = item.Value;
				Hash128 val4 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		Dictionary<AlignmentAxis, int> axisToRankMap = m_AxisToRankMap;
		if (axisToRankMap != null)
		{
			int val5 = 0;
			foreach (KeyValuePair<AlignmentAxis, int> item2 in axisToRankMap)
			{
				Hash128 hash2 = default(Hash128);
				AlignmentAxis obj3 = item2.Key;
				Hash128 val6 = UnmanagedHasher<AlignmentAxis>.GetHash128(ref obj3);
				hash2.Append(ref val6);
				int obj4 = item2.Value;
				Hash128 val7 = UnmanagedHasher<int>.GetHash128(ref obj4);
				hash2.Append(ref val7);
				val5 ^= hash2.GetHashCode();
			}
			result.Append(ref val5);
		}
		result.Append(ref m_Mix);
		List<AlignmentShiftHistoryEntry> shiftHistory = ShiftHistory;
		if (shiftHistory != null)
		{
			for (int i = 0; i < shiftHistory.Count; i++)
			{
				Hash128 val8 = ClassHasher<AlignmentShiftHistoryEntry>.GetHash128(shiftHistory[i]);
				result.Append(ref val8);
			}
		}
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartUnitAlignment source = new PartUnitAlignment();
		result = Unsafe.As<PartUnitAlignment, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartUnitAlignment>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_AxisToMarkMap", ref m_AxisToMarkMap, state);
		formatter.Field(1, "m_AxisToRankMap", ref m_AxisToRankMap, state);
		formatter.EnumField(2, "m_Mix", ref m_Mix, state);
		formatter.Field(3, "ShiftHistory", ref ShiftHistory, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitAlignment>();
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
				m_AxisToMarkMap = formatter.ReadPackable<Dictionary<AlignmentAxis, int>>(state);
				break;
			case 1:
				m_AxisToRankMap = formatter.ReadPackable<Dictionary<AlignmentAxis, int>>(state);
				break;
			case 2:
				m_Mix = formatter.ReadEnum<AlignmentMix>(state);
				break;
			case 3:
				ShiftHistory = formatter.ReadPackable<List<AlignmentShiftHistoryEntry>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
