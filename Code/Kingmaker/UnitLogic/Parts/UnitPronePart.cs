using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[OwlPackable(OwlPackableMode.Generate)]
public class UnitPronePart : UnitPart, IUnitFeaturesHandler<EntitySubscriber>, IUnitFeaturesHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitFeaturesHandler, EntitySubscriber>, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, IEventTag<IUnitLifeStateChanged, EntitySubscriber>, IHashable, IOwlPackable<UnitPronePart>
{
	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "UnitPronePart",
		OldNames = null,
		Fields = new FieldInfo[0]
	};

	private void Update()
	{
		if ((bool)base.Owner.View)
		{
			if (base.Owner.IsProne)
			{
				base.Owner.View.EnterProneState();
			}
			else
			{
				base.Owner.View.LeaveProneState();
			}
		}
		if (base.Owner.IsProne)
		{
			base.Owner.Commands.InterruptAllInterruptible();
		}
		base.Owner.Wake(1f);
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		Update();
	}

	public void HandleFeatureAdded(FeatureCountableFlag feature)
	{
		Update();
	}

	public void HandleFeatureRemoved(FeatureCountableFlag feature)
	{
		Update();
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
		UnitPronePart source = new UnitPronePart();
		result = Unsafe.As<UnitPronePart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<UnitPronePart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<UnitPronePart>();
		List<byte> mappingForType = state.GetMappingForType(OwlPackTypeInfo, typeInfo);
		formatter.EnterObject();
		for (int i = 0; i < typeInfo.Fields.Length; i++)
		{
			formatter.ReadFieldHeader(typeInfo, out var fieldID, out var size);
			if (mappingForType[fieldID] == byte.MaxValue)
			{
				formatter.SkipField(size);
			}
		}
		formatter.LeaveObject();
	}
}
