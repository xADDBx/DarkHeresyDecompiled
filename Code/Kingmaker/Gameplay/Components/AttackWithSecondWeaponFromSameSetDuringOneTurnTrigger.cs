using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Custom/AttackWithSecondWeaponFromSameSetDuringOneTurnTrigger")]
[TypeId("35d1efec5b864ba1aab497e0ceecc0a6")]
public sealed class AttackWithSecondWeaponFromSameSetDuringOneTurnTrigger : UnitFactComponentDelegate, IUnitCommandEndHandler<EntitySubscriber>, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitCommandEndHandler, EntitySubscriber>, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, IEntitySubscriber, IEventTag<ITurnEndHandler, EntitySubscriber>
{
	[OwlPackable(OwlPackableMode.Generate)]
	private sealed class ComponentData : IEntityFactComponentSavableData, IHashable, IOwlPackable<ComponentData>
	{
		[OwlPackInclude]
		private readonly List<int> _weaponSetList = new List<int>();

		public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
		{
			Name = "ComponentData",
			OldNames = null,
			Fields = new FieldInfo[1]
			{
				new FieldInfo("_weaponSetList", typeof(List<int>))
			}
		};

		public int this[int index]
		{
			get
			{
				while (index >= _weaponSetList.Count)
				{
					_weaponSetList.Add(-1);
				}
				return _weaponSetList[index];
			}
			set
			{
				while (index >= _weaponSetList.Count)
				{
					_weaponSetList.Add(-1);
				}
				_weaponSetList[index] = value;
			}
		}

		public void Reset()
		{
			for (int i = 0; i < _weaponSetList.Count; i++)
			{
				_weaponSetList[i] = -1;
			}
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
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
			List<int> value = _weaponSetList;
			formatter.Field(0, "_weaponSetList", ref value, state);
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
					Unsafe.AsRef(in _weaponSetList) = formatter.ReadPackable<List<int>>(state);
					break;
				}
			}
			formatter.LeaveObject();
		}
	}

	public ActionList Actions = new ActionList();

	void IUnitCommandEndHandler.HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (!(command is UnitUseAbility { Ability: { Weapon: { } weapon } }))
		{
			return;
		}
		int? handsEquipmentSetIndex = base.Owner.Body.GetHandsEquipmentSetIndex(weapon);
		if (!handsEquipmentSetIndex.HasValue)
		{
			return;
		}
		int valueOrDefault = handsEquipmentSetIndex.GetValueOrDefault();
		handsEquipmentSetIndex = base.Owner.Body.GetHandsEquipmentSlotIndex(weapon);
		if (handsEquipmentSetIndex.HasValue)
		{
			int valueOrDefault2 = handsEquipmentSetIndex.GetValueOrDefault();
			ComponentData componentData = RequestSavableData<ComponentData>();
			if (componentData[valueOrDefault] < 0)
			{
				componentData[valueOrDefault] = valueOrDefault2;
			}
			else if (componentData[valueOrDefault] != valueOrDefault2)
			{
				componentData[valueOrDefault] = -1;
				Actions.Run();
			}
		}
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		RequestSavableData<ComponentData>().Reset();
	}
}
