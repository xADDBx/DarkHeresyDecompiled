using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Spawners;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Interaction;

[OwlPackable(OwlPackableMode.Generate)]
public class SpawnerInteractionPart : ViewBasedPart, IUnitInitializer, IHashable, IOwlPackable<SpawnerInteractionPart>
{
	public class Wrapper : IUnitInteraction
	{
		public SpawnerInteraction Source;

		public int Distance
		{
			get
			{
				if (Source.OverrideDistance != 0)
				{
					return Source.OverrideDistance;
				}
				return 2;
			}
		}

		public bool IsApproach => Source.TriggerOnApproach;

		public float ApproachCooldown => Source.Cooldown;

		public bool MainPlayerPreferred => true;

		public bool IsDialog => Source.IsDialog;

		public bool AllowInCombat => Source.AllowInCombat;

		public virtual bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target)
		{
			if (target.IsHelpless)
			{
				return false;
			}
			if (Source.TriggerOnApproach && Source.TriggerOnParty)
			{
				if (!initiator.IsDirectlyControllable)
				{
					return false;
				}
				if (initiator.IsPet)
				{
					return false;
				}
				if (initiator.GetOptional<UnitPartSummonedMonster>() != null)
				{
					return false;
				}
			}
			if (Source.Conditions.Get() == null || !Source.Conditions.Get().Conditions.HasConditions)
			{
				return true;
			}
			using (ContextData<InteractingUnitData>.Request().Setup(initiator))
			{
				using (ContextData<ClickedUnitData>.Request().Setup(target))
				{
					return Source.Conditions.Get().Conditions.Check();
				}
			}
		}

		public AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
		{
			return Source.Interact(user, target);
		}
	}

	private readonly List<Wrapper> m_Components = new List<Wrapper>();

	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "SpawnerInteractionPart",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("SourceType", typeof(string))
		}
	};

	public override bool ShouldCheckSourceComponent => false;

	public override void SetSource(IAbstractEntityPartComponent source)
	{
		base.SetSource(source);
		m_Components.Add(new Wrapper
		{
			Source = (SpawnerInteraction)source
		});
	}

	public void OnSpawn(AbstractUnitEntity unit)
	{
	}

	public void OnInitialize(AbstractUnitEntity unit)
	{
		PartUnitInteractions orCreate = unit.GetOrCreate<PartUnitInteractions>();
		foreach (Wrapper component in m_Components)
		{
			orCreate.AddInteraction(component);
		}
	}

	public void OnDispose(AbstractUnitEntity unit)
	{
		PartUnitInteractions orCreate = unit.GetOrCreate<PartUnitInteractions>();
		foreach (Wrapper component in m_Components)
		{
			orCreate.RemoveInteraction(component);
		}
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
		SpawnerInteractionPart source = new SpawnerInteractionPart();
		result = Unsafe.As<SpawnerInteractionPart, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<SpawnerInteractionPart>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<SpawnerInteractionPart>();
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
				base.SourceType = formatter.ReadString(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
