using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.Framework.VO;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.Sound;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class PartUnitAsks : EntityPart<AbstractUnitEntity>, IHashable, IOwlPackable<PartUnitAsks>
{
	public interface IOwner : IEntityPartOwner<PartUnitAsks>, IEntityPartOwner
	{
		PartUnitAsks Asks { get; }
	}

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "PartUnitAsks",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("CustomAsks", typeof(BlueprintUnitAsksList)),
			new FieldInfo("OverrideAsks", typeof(BlueprintUnitAsksList))
		}
	};

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public BlueprintUnitAsksList CustomAsks { get; private set; }

	[JsonProperty]
	[CanBeNull]
	[OwlPackInclude]
	public BlueprintUnitAsksList OverrideAsks { get; private set; }

	public BlueprintUnitAsksList List
	{
		get
		{
			if (OverrideAsks != null)
			{
				return OverrideAsks;
			}
			if (CustomAsks != null)
			{
				return CustomAsks;
			}
			if (base.Owner.Blueprint.Asks != null)
			{
				return base.Owner.Blueprint.Asks;
			}
			return VOSettings.Instance.GetAsksByVoGuid(base.Owner.VoGuid);
		}
	}

	protected override void OnAttach()
	{
		SpawningData current = ContextData<SpawningData>.Current;
		if (current != null)
		{
			SetCustom(current.Voice);
		}
	}

	public void SetCustom(BlueprintUnitAsksList asksList)
	{
		CustomAsks = asksList;
	}

	public void SetOverride(BlueprintUnitAsksList asksList)
	{
		OverrideAsks = asksList;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(CustomAsks);
		result.Append(ref val2);
		Hash128 val3 = SimpleBlueprintHasher.GetHash128(OverrideAsks);
		result.Append(ref val3);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		PartUnitAsks source = new PartUnitAsks();
		result = Unsafe.As<PartUnitAsks, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<PartUnitAsks>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		BlueprintUnitAsksList value = CustomAsks;
		formatter.Field(0, "CustomAsks", ref value, state);
		BlueprintUnitAsksList value2 = OverrideAsks;
		formatter.Field(1, "OverrideAsks", ref value2, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<PartUnitAsks>();
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
				CustomAsks = formatter.ReadPackable<BlueprintUnitAsksList>(state);
				break;
			case 1:
				OverrideAsks = formatter.ReadPackable<BlueprintUnitAsksList>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
