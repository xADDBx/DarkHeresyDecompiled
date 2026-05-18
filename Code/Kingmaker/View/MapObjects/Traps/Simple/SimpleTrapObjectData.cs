using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.View.MapObjects.Traps.Simple.Strategies;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.Traps.Simple;

[OwlPackable(OwlPackableMode.Generate)]
public class SimpleTrapObjectData : TrapObjectData, IHashable, IOwlPackable<SimpleTrapObjectData>
{
	[JsonProperty]
	[OwlPackInclude]
	public string ViewName;

	[JsonProperty]
	[OwlPackInclude]
	public int PerceptionDC;

	[JsonProperty]
	[OwlPackInclude]
	public float PerceptionRadius;

	[JsonProperty]
	[OwlPackInclude]
	public BlueprintTrapSettings TrapSettings;

	[JsonProperty]
	[OwlPackInclude]
	private int m_DisableDC;

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SimpleTrapObjectData",
		OldNames = null,
		Fields = new FieldInfo[24]
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
			new FieldInfo("IsNewInGame", typeof(bool)),
			new FieldInfo("TrapActive", typeof(bool)),
			new FieldInfo("ScriptZoneId", typeof(string)),
			new FieldInfo("ViewName", typeof(string)),
			new FieldInfo("PerceptionDC", typeof(int)),
			new FieldInfo("PerceptionRadius", typeof(float)),
			new FieldInfo("TrapSettings", typeof(BlueprintTrapSettings)),
			new FieldInfo("m_DisableDC", typeof(int))
		}
	};

	public new ISimpleTrapEntityConfig Config => (ISimpleTrapEntityConfig)base.Config;

	public override string Name => Config.NameInLog;

	public override int DisableDC
	{
		get
		{
			return m_DisableDC;
		}
		set
		{
			m_DisableDC = value;
		}
	}

	public SimpleTrapObjectInfo Info => Config.Info;

	public override bool IsHiddenWhenInactive => !Info.DoNotHideWhenInactive;

	public override int DisableTriggerMargin => ConfigRoot.Instance.BlueprintTrapSettingsRoot.DisableDCMargin;

	protected override StatType DisarmSkill => Info.DisarmSkill;

	public SimpleTrapObjectData(ISimpleTrapEntityConfig config)
		: base(config)
	{
	}

	protected SimpleTrapObjectData(OwlPackConstructorParameter _)
		: base(_)
	{
	}

	protected override IEntityView CreateViewForData()
	{
		return SimpleTrapObjectView.CreateView(this, base.UniqueId, base.ScriptZoneId);
	}

	public override bool CanTrigger()
	{
		return true;
	}

	public override bool CanUnitDisable(BaseUnitEntity unit)
	{
		return true;
	}

	public override void RunTrapActions()
	{
		SimpleTrapCastSpell.Invoke(this);
		base.IsInGame = false;
		float barkDuration = UtilityBark.GetBarkDuration(Info.TrapTriggeredText);
		BaseUnitEntity baseUnitEntity = ContextData<BlueprintTrap.ElementsData>.Current?.TriggeringUnit;
		BarkPlayer.Bark(baseUnitEntity, Info.TrapTriggeredText, VoiceOverType.Bark, baseUnitEntity?.VoGuid, barkDuration, baseUnitEntity);
		ActionList triggerActions = Config.TriggerActions;
		if (triggerActions != null && triggerActions.HasActions)
		{
			triggerActions.Run();
		}
	}

	public override void RunDisableActions(BaseUnitEntity unit)
	{
		Experience.GainForSkillCheck((Info.DisableDifficulty == TrapDisableDifficulty.Easy) ? SkillCheckDifficulty.Easy : SkillCheckDifficulty.Hard, unit);
		ActionList disarmActions = Config.DisarmActions;
		if (disarmActions != null && disarmActions.HasActions)
		{
			using (ContextData<InteractingUnitData>.Request().Setup(unit))
			{
				disarmActions.Run();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ViewName);
		result.Append(ref PerceptionDC);
		result.Append(ref PerceptionRadius);
		Hash128 val2 = SimpleBlueprintHasher.GetHash128(TrapSettings);
		result.Append(ref val2);
		result.Append(ref m_DisableDC);
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		SimpleTrapObjectData source = new SimpleTrapObjectData(default(OwlPackConstructorParameter));
		result = Unsafe.As<SimpleTrapObjectData, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SimpleTrapObjectData>(OwlPackTypeInfo);
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
		bool value6 = base.TrapActive;
		formatter.UnmanagedField(17, "TrapActive", ref value6, state);
		string value7 = base.ScriptZoneId;
		formatter.StringField(18, "ScriptZoneId", ref value7, state);
		formatter.StringField(19, "ViewName", ref ViewName, state);
		formatter.UnmanagedField(20, "PerceptionDC", ref PerceptionDC, state);
		formatter.UnmanagedField(21, "PerceptionRadius", ref PerceptionRadius, state);
		formatter.Field(22, "TrapSettings", ref TrapSettings, state);
		formatter.UnmanagedField(23, "m_DisableDC", ref m_DisableDC, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SimpleTrapObjectData>();
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
			case 17:
				base.TrapActive = formatter.ReadUnmanaged<bool>(state);
				break;
			case 18:
				base.ScriptZoneId = formatter.ReadString(state);
				break;
			case 19:
				ViewName = formatter.ReadString(state);
				break;
			case 20:
				PerceptionDC = formatter.ReadUnmanaged<int>(state);
				break;
			case 21:
				PerceptionRadius = formatter.ReadUnmanaged<float>(state);
				break;
			case 22:
				TrapSettings = formatter.ReadPackable<BlueprintTrapSettings>(state);
				break;
			case 23:
				m_DisableDC = formatter.ReadUnmanaged<int>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
