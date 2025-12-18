using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartFamiliarLeader : BaseUnitPart, IHashable, IOwlPackable<UnitPartFamiliarLeader>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartFamiliarLeader",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_EquippedFamiliars", typeof(List<FamiliarData>)),
			new FieldInfo("LastEquippedFamiliar", typeof(BlueprintUnit))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	private List<FamiliarData> m_EquippedFamiliars { get; set; } = new List<FamiliarData>();


	[JsonProperty(PropertyName = "m_LastEquippedFamiliar")]
	[OwlPackInclude]
	public BlueprintUnit LastEquippedFamiliar { get; private set; }

	public IEnumerable<AbstractUnitEntity> EquippedFamiliars
	{
		get
		{
			if (m_EquippedFamiliars.Count != 0)
			{
				return m_EquippedFamiliars.Select((FamiliarData i) => i.Unit);
			}
			return Enumerable.Empty<AbstractUnitEntity>();
		}
	}

	[CanBeNull]
	public AbstractUnitEntity FirstFamiliar => m_EquippedFamiliars.FirstItem((FamiliarData i) => i.Unit != null)?.Unit;

	public bool HasEquippedFamiliar(BlueprintUnit blueprint)
	{
		return m_EquippedFamiliars.HasItem((FamiliarData i) => i.Unit?.Blueprint == blueprint);
	}

	public bool HasEquippedFamiliar(BlueprintUnit blueprint, EntityFactSource source)
	{
		return GetEquippedFamiliar(blueprint, source) != null;
	}

	[CanBeNull]
	public FamiliarData GetEquippedFamiliar(BlueprintUnit blueprint, EntityFactSource source)
	{
		return m_EquippedFamiliars.FirstItem((FamiliarData x) => x.Unit?.Blueprint == blueprint && x.Source == source);
	}

	public bool HasEquippedFamiliar(AbstractUnitEntity unit)
	{
		return GetEquippedFamiliar(unit) != null;
	}

	[CanBeNull]
	private FamiliarData GetEquippedFamiliar(AbstractUnitEntity unit)
	{
		return m_EquippedFamiliars.FirstItem((FamiliarData i) => i.Unit == unit);
	}

	public void AddEquippedFamiliar([NotNull] AbstractUnitEntity familiar, EntityFactSource source)
	{
		FamiliarData familiarData = m_EquippedFamiliars.FirstItem((FamiliarData i) => i.Unit == familiar);
		if (familiarData == null)
		{
			familiarData = new FamiliarData(familiar, source);
			m_EquippedFamiliars.Add(familiarData);
			LastEquippedFamiliar = familiar.Blueprint;
		}
	}

	public void RemoveEquippedFamiliar(AbstractUnitEntity familiar)
	{
		FamiliarData equippedFamiliar = GetEquippedFamiliar(familiar);
		if (equippedFamiliar != null)
		{
			m_EquippedFamiliars.Remove(equippedFamiliar);
		}
	}

	public void UpdateFamiliarsVisibility()
	{
		foreach (FamiliarData equippedFamiliar in m_EquippedFamiliars)
		{
			UnitPartFamiliar unitPartFamiliar = equippedFamiliar.Unit?.GetRequired<UnitPartFamiliar>();
			if (unitPartFamiliar != null)
			{
				unitPartFamiliar.UpdateViewVisibility();
				unitPartFamiliar.UpdateIsInGameState(base.Owner.IsInGame);
			}
		}
	}

	protected override void OnApplyPostLoadFixes()
	{
		m_EquippedFamiliars.RemoveAll((FamiliarData i) => i.Unit == null);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<FamiliarData> equippedFamiliars = m_EquippedFamiliars;
		if (equippedFamiliars != null)
		{
			for (int i = 0; i < equippedFamiliars.Count; i++)
			{
				Hash128 val2 = ClassHasher<FamiliarData>.GetHash128(equippedFamiliars[i]);
				result.Append(ref val2);
			}
		}
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(LastEquippedFamiliar);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartFamiliarLeader source = new UnitPartFamiliarLeader();
		result = Unsafe.As<UnitPartFamiliarLeader, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartFamiliarLeader>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		List<FamiliarData> value = m_EquippedFamiliars;
		formatter.Field(0, "m_EquippedFamiliars", ref value, state);
		BlueprintUnit value2 = LastEquippedFamiliar;
		formatter.Field(1, "LastEquippedFamiliar", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartFamiliarLeader>();
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
				m_EquippedFamiliars = formatter.ReadPackable<List<FamiliarData>>(state);
				break;
			case 1:
				LastEquippedFamiliar = formatter.ReadPackable<BlueprintUnit>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
