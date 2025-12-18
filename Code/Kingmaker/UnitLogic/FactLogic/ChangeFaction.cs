using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Code.GameCore.Blueprints;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b274525bad69a5c44b6075bb99e0eeaa")]
public class ChangeFaction : UnitFactComponentDelegate
{
	[OwlPackable(OwlPackableMode.Generate)]
	public class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		[JsonProperty]
		[OwlPackInclude]
		public BlueprintFaction OriginalFaction;

		[JsonProperty]
		[OwlPackInclude]
		public string OriginalGroupId;

		[JsonProperty]
		[OwlPackInclude]
		public List<BlueprintFaction> OriginalAttackFactions;

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[3]
			{
				new FieldInfo("OriginalFaction", typeof(BlueprintFaction)),
				new FieldInfo("OriginalGroupId", typeof(string)),
				new FieldInfo("OriginalAttackFactions", typeof(List<BlueprintFaction>))
			}
		};

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			Hash128 val2 = SimpleBlueprintHasher.GetHash128(OriginalFaction);
			result.Append(ref val2);
			result.Append(OriginalGroupId);
			List<BlueprintFaction> originalAttackFactions = OriginalAttackFactions;
			if (originalAttackFactions != null)
			{
				for (int i = 0; i < originalAttackFactions.Count; i++)
				{
					Hash128 val3 = SimpleBlueprintHasher.GetHash128(originalAttackFactions[i]);
					result.Append(ref val3);
				}
			}
			return result;
		}

		public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
		{
			ComponentData source = new ComponentData();
			result = Unsafe.As<ComponentData, TPossiblyBase>(ref source);
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
			ushort type = state.TypeLibrary.RegisterType<ComponentData>(OwlPackTypeInfo);
			formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
			formatter.Field(0, "OriginalFaction", ref OriginalFaction, state);
			formatter.StringField(1, "OriginalGroupId", ref OriginalGroupId, state);
			formatter.Field(2, "OriginalAttackFactions", ref OriginalAttackFactions, state);
			formatter.EndObject();
		}

		public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
		{
			state.References.Register(objectId, this);
			TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ComponentData>();
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
					OriginalFaction = formatter.ReadPackable<BlueprintFaction>(state);
					break;
				case 1:
					OriginalGroupId = formatter.ReadString(state);
					break;
				case 2:
					OriginalAttackFactions = formatter.ReadPackable<List<BlueprintFaction>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	private enum ChangeType
	{
		ToNeutrals,
		ToCaster,
		ToCustom
	}

	[SerializeField]
	[UsedImplicitly]
	private ChangeType m_Type;

	[SerializeField]
	[UsedImplicitly]
	[ShowIf("ToCustom")]
	private BlueprintFactionReference m_Faction;

	[SerializeField]
	[UsedImplicitly]
	private bool m_AllowDirectControl;

	private bool ToCustom => m_Type == ChangeType.ToCustom;

	protected override void OnActivate()
	{
		ComponentData componentData = RequestSavableData<ComponentData>();
		componentData.OriginalFaction = base.Owner.Faction.Blueprint;
		componentData.OriginalGroupId = base.Owner.CombatGroup.Id;
		componentData.OriginalAttackFactions = base.Owner.Faction.AttackFactions.ToList();
		BlueprintFaction faction;
		string id;
		switch (m_Type)
		{
		case ChangeType.ToNeutrals:
			faction = ConfigRoot.Instance.SystemMechanics.FactionNeutrals;
			id = base.Owner.UniqueId;
			break;
		case ChangeType.ToCaster:
		{
			MechanicEntity mechanicEntity = base.Fact.MaybeContext?.MaybeCaster;
			if (mechanicEntity == null)
			{
				PFLog.Default.Error("Caster is missing");
				return;
			}
			BlueprintFaction blueprintFaction = mechanicEntity.GetFactionOptional()?.Blueprint;
			if (blueprintFaction != null)
			{
				string text = mechanicEntity.GetCombatGroupOptional()?.Id;
				if (text != null)
				{
					faction = blueprintFaction;
					id = text;
					break;
				}
			}
			PFLog.Default.Error("Caster's faction or combat group is missing");
			return;
		}
		case ChangeType.ToCustom:
			faction = m_Faction.Get();
			id = base.Owner.UniqueId;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		base.Owner.CombatGroup.Id = id;
		base.Owner.Faction.Set(faction);
		base.Owner.CombatGroup.ResetFactionSet();
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitFactionHandler>)delegate(IUnitFactionHandler h)
		{
			h.HandleFactionChanged();
		}, isCheckRuntime: true);
		base.Owner.Commands.InterruptAllInterruptible();
	}

	protected override void OnActivateOrPostLoad()
	{
		if (!m_AllowDirectControl)
		{
			base.Owner.PreventDirectControl.Retain();
		}
	}

	protected override void OnDeactivate()
	{
		if (!m_AllowDirectControl)
		{
			base.Owner.PreventDirectControl.Release();
		}
		ComponentData componentData = RequestSavableData<ComponentData>();
		base.Owner.CombatGroup.Id = componentData.OriginalGroupId;
		base.Owner.Faction.Set(componentData.OriginalFaction);
		foreach (BlueprintFaction originalAttackFaction in componentData.OriginalAttackFactions)
		{
			base.Owner.Faction.AttackFactions.Add(originalAttackFaction);
		}
		base.Owner.CombatGroup.ResetFactionSet();
		EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IUnitFactionHandler>)delegate(IUnitFactionHandler h)
		{
			h.HandleFactionChanged();
		}, isCheckRuntime: true);
		base.Owner.Commands.InterruptAllInterruptible();
	}
}
