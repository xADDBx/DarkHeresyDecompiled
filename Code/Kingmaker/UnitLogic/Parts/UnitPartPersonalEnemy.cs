using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.View.Bridge.Facades;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPartPersonalEnemy : BaseUnitPart, IAreaHandler, ISubscriber, IHashable, IOwlPackable<UnitPartPersonalEnemy>
{
	private UnitPersonalEnemyFx m_FxComponent;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPartPersonalEnemy",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("Enemy", typeof(UnitReference))
		}
	};

	[JsonProperty]
	[OwlPackInclude]
	public UnitReference Enemy { get; private set; }

	public bool IsCurrentlyTargetable => SelectionManagerFacade.IsSelected(Enemy.ToAbstractUnitEntity());

	public void Init(BaseUnitEntity enemy)
	{
		Enemy = enemy.FromBaseUnitEntity();
		m_FxComponent = base.Owner.View.gameObject.AddComponent<UnitPersonalEnemyFx>();
		m_FxComponent.Data = this;
	}

	protected override void OnDetach()
	{
		if ((bool)m_FxComponent)
		{
			Object.Destroy(m_FxComponent);
		}
	}

	protected override void OnPostLoad()
	{
		base.EventBus.Subscribe(this);
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		Init(Enemy.ToBaseUnitEntity());
		base.EventBus.Unsubscribe(this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		UnitReference obj = Enemy;
		Hash128 val2 = UnitReferenceHasher.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		UnitPartPersonalEnemy source = new UnitPartPersonalEnemy();
		result = Unsafe.As<UnitPartPersonalEnemy, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPartPersonalEnemy>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		UnitReference value = Enemy;
		formatter.Field(0, "Enemy", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPartPersonalEnemy>();
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
				Enemy = formatter.ReadPackable<UnitReference>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
