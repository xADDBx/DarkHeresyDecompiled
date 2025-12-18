using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Mechanics.Entities;

[OwlPackable(OwlPackableMode.Generate)]
public class ScriptZoneEntity : MapObjectEntity, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IAreaHandler, IHashable, IOwlPackable<ScriptZoneEntity>
{
	public class UnitInfo
	{
		public UnitReference Reference;

		public bool InsideThisTick;

		public bool IsValid
		{
			get
			{
				if (Reference.Entity != null)
				{
					return Reference.Entity.ToBaseUnitEntity().IsInState;
				}
				return false;
			}
		}
	}

	[JsonProperty]
	[OwlPackInclude]
	public bool IsActive = true;

	[JsonProperty]
	[OwlPackInclude]
	public bool WasEntered;

	[JsonProperty]
	[OwlPackInclude]
	private readonly List<UnitReference> m_HiddenUnits = new List<UnitReference>();

	public readonly List<UnitInfo> InsideUnits = new List<UnitInfo>();

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ScriptZoneEntity",
		OldNames = null,
		Fields = new FieldInfo[20]
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
			new FieldInfo("IsActive", typeof(bool)),
			new FieldInfo("WasEntered", typeof(bool)),
			new FieldInfo("m_HiddenUnits", typeof(List<UnitReference>))
		}
	};

	public new ScriptZone View => (ScriptZone)base.View;

	[UsedImplicitly]
	protected ScriptZoneEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public ScriptZoneEntity(MapObjectView view)
		: base(view)
	{
	}

	protected ScriptZoneEntity()
	{
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (!IsActive || !base.IsInGame)
		{
			return;
		}
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (IsInterestedInUnit(allBaseUnit) && ContainsPosition(allBaseUnit.Position))
			{
				TryAddUnit(allBaseUnit);
			}
			if (!IsActive)
			{
				break;
			}
		}
	}

	public void Tick()
	{
		if (!IsActive)
		{
			return;
		}
		foreach (UnitInfo insideUnit in InsideUnits)
		{
			insideUnit.InsideThisTick = false;
		}
		foreach (IScriptZoneShape shape in View.Shapes)
		{
			List<BaseUnitEntity> list = (View.PlayersOnly ? Game.Instance.Player.PartyAndPets : (EntityBoundsHelper.FindUnitsInShape(shape) ?? Game.Instance.EntityPools.AllBaseAwakeUnits));
			using (ProfileScope.New("Tick one shape"))
			{
				foreach (BaseUnitEntity item in list)
				{
					TickUnit(item, shape);
					if (!IsActive)
					{
						break;
					}
				}
			}
		}
		foreach (UnitInfo insideUnit2 in InsideUnits)
		{
			if (!insideUnit2.InsideThisTick)
			{
				OnUnitRemoved(insideUnit2.Reference.ToBaseUnitEntity());
			}
		}
		InsideUnits.RemoveAll((UnitInfo i) => !i.IsValid || !i.InsideThisTick);
	}

	private void TickUnit(BaseUnitEntity unit, IScriptZoneShape shape)
	{
		if (!IsActive || !IsInterestedInUnit(unit) || unit.IsExtra)
		{
			return;
		}
		UnitInfo unitInfo = null;
		foreach (UnitInfo insideUnit in InsideUnits)
		{
			if (insideUnit.Reference == unit)
			{
				unitInfo = insideUnit;
				break;
			}
		}
		if ((unitInfo == null || !unitInfo.InsideThisTick) && shape.Contains(unit.Position) && !AbstractUnitCommand.CommandTargetUntargetable(null, unit))
		{
			if (unitInfo != null)
			{
				unitInfo.InsideThisTick = true;
			}
			else
			{
				TryAddUnit(unit);
			}
		}
	}

	public bool ContainsPosition(Vector3 point)
	{
		for (int i = 0; i < View.Shapes.Count; i++)
		{
			if (View.Shapes[i].Contains(point))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsUnit(AbstractUnitEntity unit)
	{
		if (!IsActive)
		{
			return false;
		}
		foreach (UnitInfo insideUnit in InsideUnits)
		{
			if (insideUnit.Reference.Entity == unit)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsInterestedInUnit(BaseUnitEntity unit)
	{
		if (!View.UseDeads && unit.LifeState.IsDead)
		{
			return false;
		}
		if (View.PlayersOnly)
		{
			if (unit.Faction.IsPlayer)
			{
				UnitPartCompanion companionOptional = unit.GetCompanionOptional();
				if (companionOptional == null || companionOptional.State != CompanionState.Remote)
				{
					goto IL_004f;
				}
			}
			return false;
		}
		goto IL_004f;
		IL_004f:
		if ((bool)unit.GetOptional<UnitPartFollowUnit>())
		{
			return false;
		}
		return true;
	}

	private void TryAddUnit(BaseUnitEntity unit)
	{
		using (ContextData<ScriptZoneTriggerData>.Request().Setup(unit, HoldingState))
		{
			if (View.Blueprint != null && !View.Blueprint.TriggerConditions.Check())
			{
				return;
			}
			InsideUnits.Add(new UnitInfo
			{
				Reference = unit.FromBaseUnitEntity(),
				InsideThisTick = true
			});
			if (View.DisableSameMultipleTriggers)
			{
				if (WasEntered)
				{
					return;
				}
				WasEntered = true;
			}
			OnUnitEnter(unit);
			View.OnUnitEntered.Invoke(unit, View);
			EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IScriptZoneHandler>)delegate(IScriptZoneHandler h)
			{
				h.OnUnitEnteredScriptZone(View);
			}, isCheckRuntime: true);
		}
		if (View.OnceOnly)
		{
			IsActive = false;
		}
	}

	private void RemoveUnit(BaseUnitEntity unit)
	{
		InsideUnits.Remove((UnitInfo i) => i.Reference == unit);
		OnUnitRemoved(unit);
	}

	private void OnUnitRemoved(BaseUnitEntity unit)
	{
		if (View.DisableSameMultipleTriggers && WasEntered)
		{
			if (View.Count > 0)
			{
				return;
			}
			WasEntered = false;
		}
		using (ContextData<ScriptZoneTriggerData>.Request().Setup(unit, HoldingState))
		{
			OnUnitExit(unit);
			View.OnUnitExited.Invoke(unit, View);
			EventBus.RaiseEvent((IBaseUnitEntity)unit, (Action<IScriptZoneHandler>)delegate(IScriptZoneHandler h)
			{
				h.OnUnitExitedScriptZone(View);
			}, isCheckRuntime: true);
		}
	}

	private void OnUnitExit(BaseUnitEntity triggeringUnit)
	{
		if (base.Blueprint != null)
		{
			if (View.Blueprint == null)
			{
				PFLog.Default.Warning("ScriptZone " + View.name + " blueprint not set");
			}
			else
			{
				View.Blueprint.ExitActions.Run();
			}
		}
	}

	private void OnUnitEnter(BaseUnitEntity triggeringUnit)
	{
		if (base.Blueprint != null)
		{
			if (View.Blueprint == null)
			{
				PFLog.Default.Warning("ScriptZone " + View.name + " blueprint not set");
			}
			else
			{
				View.Blueprint.EnterActions.Run();
			}
		}
	}

	public void HandleUnitSpawned()
	{
	}

	void IUnitHandler.HandleUnitDestroyed()
	{
		HandleUnitDestroyed(EventInvokerExtensions.BaseUnitEntity);
	}

	void IUnitHandler.HandleUnitDeath()
	{
		HandleUnitDestroyed(EventInvokerExtensions.BaseUnitEntity);
	}

	private void HandleUnitDestroyed(BaseUnitEntity entityData)
	{
		if (entityData == null || !InsideUnits.HasItem((UnitInfo i) => i.Reference == entityData))
		{
			return;
		}
		if (IsActive)
		{
			RemoveUnit(entityData);
			return;
		}
		InsideUnits.Remove((UnitInfo i) => i.Reference == entityData);
	}

	public void RemoveAll()
	{
		foreach (UnitInfo item in InsideUnits.ToTempList())
		{
			RemoveUnit(item.Reference.ToBaseUnitEntity());
		}
	}

	public void AddHiddenUnit(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			PFLog.Default.Warning("ScriptZoneEntity.AddHiddenUnit: unit is null");
			return;
		}
		UnitReference unitRef = UnitReference.FromIAbstractUnitEntity(unit);
		if (!m_HiddenUnits.Any((UnitReference r) => r == unitRef))
		{
			m_HiddenUnits.Add(unitRef);
			PFLog.Default.Log($"ScriptZoneEntity.AddHiddenUnit: Added unit {unit.CharacterName} (ID: {unit.UniqueId}) to hidden list. Total hidden: {m_HiddenUnits.Count}");
			return;
		}
		PFLog.Default.Log("ScriptZoneEntity.AddHiddenUnit: Unit " + unit.CharacterName + " (ID: " + unit.UniqueId + ") already in hidden list");
	}

	public void RemoveHiddenUnit(BaseUnitEntity unit)
	{
		if (unit == null)
		{
			PFLog.Default.Warning("ScriptZoneEntity.RemoveHiddenUnit: unit is null");
			return;
		}
		UnitReference item = UnitReference.FromIAbstractUnitEntity(unit);
		if (m_HiddenUnits.Remove(item))
		{
			PFLog.Default.Log($"ScriptZoneEntity.RemoveHiddenUnit: Removed unit {unit.CharacterName} (ID: {unit.UniqueId}) from hidden list. Remaining: {m_HiddenUnits.Count}");
			return;
		}
		PFLog.Default.Log("ScriptZoneEntity.RemoveHiddenUnit: Unit " + unit.CharacterName + " (ID: " + unit.UniqueId + ") was not in hidden list");
	}

	public List<BaseUnitEntity> GetHiddenUnits()
	{
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		List<UnitReference> list2 = new List<UnitReference>();
		PFLog.Default.Log($"ScriptZoneEntity.GetHiddenUnits: Checking {m_HiddenUnits.Count} hidden unit references");
		foreach (UnitReference hiddenUnit in m_HiddenUnits)
		{
			BaseUnitEntity baseUnitEntity = hiddenUnit.Entity?.ToBaseUnitEntity();
			if (baseUnitEntity != null && baseUnitEntity.IsInState)
			{
				list.Add(baseUnitEntity);
				PFLog.Default.Log($"ScriptZoneEntity.GetHiddenUnits: Found valid unit {baseUnitEntity.CharacterName} (ID: {baseUnitEntity.UniqueId}), IsDead: {baseUnitEntity.LifeState.IsDead}, IsInGame: {baseUnitEntity.IsInGame}");
			}
			else
			{
				list2.Add(hiddenUnit);
				PFLog.Default.Log("ScriptZoneEntity.GetHiddenUnits: Invalid unit reference " + hiddenUnit.Id + " - unit is null or not in state");
			}
		}
		foreach (UnitReference item in list2)
		{
			m_HiddenUnits.Remove(item);
			PFLog.Default.Log("ScriptZoneEntity.GetHiddenUnits: Removed invalid reference " + item.Id);
		}
		PFLog.Default.Log($"ScriptZoneEntity.GetHiddenUnits: Returning {list.Count} valid units out of {m_HiddenUnits.Count} references");
		return list;
	}

	public void ClearHiddenUnits()
	{
		int count = m_HiddenUnits.Count;
		m_HiddenUnits.Clear();
		PFLog.Default.Log($"ScriptZoneEntity.ClearHiddenUnits: Cleared {count} hidden unit references");
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref IsActive);
		result.Append(ref WasEntered);
		List<UnitReference> hiddenUnits = m_HiddenUnits;
		if (hiddenUnits != null)
		{
			for (int i = 0; i < hiddenUnits.Count; i++)
			{
				UnitReference obj = hiddenUnits[i];
				Hash128 val2 = UnitReferenceHasher.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		return result;
	}

	public new static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		ScriptZoneEntity source = new ScriptZoneEntity();
		result = Unsafe.As<ScriptZoneEntity, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ScriptZoneEntity>(OwlPackTypeInfo);
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
		formatter.UnmanagedField(17, "IsActive", ref IsActive, state);
		formatter.UnmanagedField(18, "WasEntered", ref WasEntered, state);
		List<UnitReference> value6 = m_HiddenUnits;
		formatter.Field(19, "m_HiddenUnits", ref value6, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ScriptZoneEntity>();
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
				IsActive = formatter.ReadUnmanaged<bool>(state);
				break;
			case 18:
				WasEntered = formatter.ReadUnmanaged<bool>(state);
				break;
			case 19:
				Unsafe.AsRef(in m_HiddenUnits) = formatter.ReadPackable<List<UnitReference>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
