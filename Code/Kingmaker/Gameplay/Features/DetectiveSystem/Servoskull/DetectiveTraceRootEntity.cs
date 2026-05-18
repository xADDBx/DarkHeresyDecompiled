using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[OwlPackable(OwlPackableMode.Generate)]
public class DetectiveTraceRootEntity : MapObjectEntity, IAreaActivationHandler, ISubscriber, IEtudesUpdateHandler, IHashable, IOwlPackable<DetectiveTraceRootEntity>
{
	private List<EntityRef<DetectiveTraceEntity>> m_RootTraces = new List<EntityRef<DetectiveTraceEntity>>();

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "DetectiveTraceRootEntity",
		OldNames = null,
		Fields = new FieldInfo[17]
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
			new FieldInfo("WasHighlightedOnRevealAndNoticed", typeof(bool)),
			new FieldInfo("ViewSettings", typeof(MapObjectViewSettings)),
			new FieldInfo("IsNewInGame", typeof(bool))
		}
	};

	public InteractionPartDetectiveTrace Interaction => GetRequired<InteractionPartDetectiveTrace>();

	public new IDetectiveTraceRootConfig Config => (IDetectiveTraceRootConfig)base.Config;

	public DetectiveTraceRootEntity(IDetectiveTraceRootConfig config)
		: base(config)
	{
	}

	public DetectiveTraceRootEntity(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override IEntityView CreateViewForData()
	{
		return null;
	}

	void IAreaActivationHandler.OnAreaActivated()
	{
		UpdateRootTraces();
	}

	public void OnEtudesUpdate()
	{
		UpdateRootTraces();
	}

	private void UpdateRootTraces()
	{
		m_RootTraces.Clear();
		foreach (DetectiveTraceEntity item in Config.Traces.Dereference().NotNull())
		{
			m_RootTraces.Add(item);
			item.SetRoot(this);
		}
	}

	public void FoundRootTraces()
	{
		foreach (EntityRef<DetectiveTraceEntity> rootTrace in m_RootTraces)
		{
			rootTrace.Entity?.Found();
		}
	}

	public void MarkFollowedToTrueEnd()
	{
		foreach (EntityRef<DetectiveTraceEntity> rootTrace in m_RootTraces)
		{
			rootTrace.Entity?.ForceFollowedToTrueEndAll();
		}
	}

	public void ForceFollowedToTrueEndAll()
	{
		MarkFollowedToTrueEnd();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		DetectiveTraceRootEntity source = new DetectiveTraceRootEntity(default(OwlPackConstructorParameter));
		result = Unsafe.As<DetectiveTraceRootEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<DetectiveTraceRootEntity>(OwlPackTypeInfo);
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
		bool value3 = base.WasHighlightedOnRevealAndNoticed;
		formatter.UnmanagedField(14, "WasHighlightedOnRevealAndNoticed", ref value3, state);
		MapObjectViewSettings value4 = base.ViewSettings;
		formatter.Field(15, "ViewSettings", ref value4, state);
		bool value5 = base.IsNewInGame;
		formatter.UnmanagedField(16, "IsNewInGame", ref value5, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<DetectiveTraceRootEntity>();
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
				base.WasHighlightedOnRevealAndNoticed = formatter.ReadUnmanaged<bool>(state);
				break;
			case 15:
				base.ViewSettings = formatter.ReadPackable<MapObjectViewSettings>(state);
				break;
			case 16:
				base.IsNewInGame = formatter.ReadUnmanaged<bool>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
