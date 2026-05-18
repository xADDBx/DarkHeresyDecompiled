using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

[OwlPackable(OwlPackableMode.Generate)]
public sealed class InitiativePlaceholderEntity : MechanicEntity, IPartyCombatHandler, ISubscriber, IUnitDeathHandler, ITurnEndHandler, ISubscriber<IMechanicEntity>, IInitiativeDelegate, ICombatParticipant, IHashable, IOwlPackable<InitiativePlaceholderEntity>
{
	private static readonly List<InitiativePlaceholderEntity> AllList = new List<InitiativePlaceholderEntity>();

	[JsonProperty]
	[OwlPackInclude]
	public MechanicEntity CorrespondingEnemy;

	[JsonProperty]
	[OwlPackInclude]
	public bool MarkedForDisposeOnTurnEnd;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "InitiativePlaceholderEntity",
		OldNames = null,
		Fields = new FieldInfo[18]
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
			new FieldInfo("m_ViewHandlingOnDisposePolicyOverride", typeof(ViewHandlingOnDisposePolicyType?)),
			new FieldInfo("m_Initiative", typeof(Initiative)),
			new FieldInfo("m_OriginalBlueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("m_Blueprint", typeof(BlueprintMechanicEntityFact)),
			new FieldInfo("MainFact", typeof(MechanicEntityFact)),
			new FieldInfo("Delegate", typeof(MechanicEntity)),
			new FieldInfo("CorrespondingEnemy", typeof(MechanicEntity)),
			new FieldInfo("Index", typeof(int)),
			new FieldInfo("MarkedForDisposeOnTurnEnd", typeof(bool))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public MechanicEntity Delegate { get; private set; }

	[JsonProperty]
	[OwlPackInclude]
	public int Index { get; private set; }

	public override bool NeedsView => false;

	public override bool IsInCombat => Delegate.IsInCombat;

	public static ReadonlyList<InitiativePlaceholderEntity> All => AllList;

	MechanicEntity IInitiativeDelegate.Delegate => Delegate;

	[NotNull]
	public static InitiativePlaceholderEntity Ensure(MechanicEntity entity, int index)
	{
		return All.FirstItem((InitiativePlaceholderEntity i) => i.Delegate == entity && i.Index == index) ?? Entity.Initialize(new InitiativePlaceholderEntity(entity, index));
	}

	public InitiativePlaceholderEntity(MechanicEntity @delegate, int index)
		: base(ConstructUniqueId(@delegate, index), @delegate.IsInGame, @delegate.Blueprint)
	{
		Delegate = @delegate;
		Index = index;
	}

	private InitiativePlaceholderEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		AllList.Add(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		AllList.Remove(this);
	}

	private static string ConstructUniqueId(MechanicEntity entity, int index)
	{
		return entity.UniqueId + "_ip" + index;
	}

	protected override MechanicEntityFact CreateMainFact(BlueprintMechanicEntityFact blueprint)
	{
		return null;
	}

	protected override IEntityView CreateViewForData()
	{
		return null;
	}

	void IPartyCombatHandler.HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!inCombat)
		{
			Dispose();
		}
	}

	public void HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (unitEntity == CorrespondingEnemy)
		{
			MarkedForDisposeOnTurnEnd = true;
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (MarkedForDisposeOnTurnEnd)
		{
			Dispose();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicEntity>.GetHash128(Delegate);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<MechanicEntity>.GetHash128(CorrespondingEnemy);
		result.Append(ref val3);
		int val4 = Index;
		result.Append(ref val4);
		result.Append(ref MarkedForDisposeOnTurnEnd);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		InitiativePlaceholderEntity source = new InitiativePlaceholderEntity(default(OwlPackConstructorParameter));
		result = Unsafe.As<InitiativePlaceholderEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<InitiativePlaceholderEntity>(OwlPackTypeInfo);
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
		formatter.Field(10, "m_Initiative", ref m_Initiative, state);
		formatter.Field(11, "m_OriginalBlueprint", ref m_OriginalBlueprint, state);
		formatter.Field(12, "m_Blueprint", ref m_Blueprint, state);
		MechanicEntityFact value2 = base.MainFact;
		formatter.Field(13, "MainFact", ref value2, state);
		MechanicEntity value3 = Delegate;
		formatter.Field(14, "Delegate", ref value3, state);
		formatter.Field(15, "CorrespondingEnemy", ref CorrespondingEnemy, state);
		int value4 = Index;
		formatter.UnmanagedField(16, "Index", ref value4, state);
		formatter.UnmanagedField(17, "MarkedForDisposeOnTurnEnd", ref MarkedForDisposeOnTurnEnd, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<InitiativePlaceholderEntity>();
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
			case 10:
				m_Initiative = formatter.ReadPackable<Initiative>(state);
				break;
			case 11:
				m_OriginalBlueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 12:
				m_Blueprint = formatter.ReadPackable<BlueprintMechanicEntityFact>(state);
				break;
			case 13:
				base.MainFact = formatter.ReadPackable<MechanicEntityFact>(state);
				break;
			case 14:
				Delegate = formatter.ReadPackable<MechanicEntity>(state);
				break;
			case 15:
				CorrespondingEnemy = formatter.ReadPackable<MechanicEntity>(state);
				break;
			case 16:
				Index = formatter.ReadUnmanaged<int>(state);
				break;
			case 17:
				MarkedForDisposeOnTurnEnd = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
