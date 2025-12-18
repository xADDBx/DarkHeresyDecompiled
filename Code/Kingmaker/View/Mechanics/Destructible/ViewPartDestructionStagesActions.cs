using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using OwlPack.Runtime;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Destructible;

[OwlPackable(OwlPackableMode.Generate)]
public class ViewPartDestructionStagesActions : ViewBasedPart<DestructionStagesActionsSettings>, IDestructionStagesManager, IHashable, IOwlPackable<ViewPartDestructionStagesActions>
{
	public new static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "ViewPartDestructionStagesActions",
		OldNames = null,
		Fields = new FieldInfo[1]
		{
			new FieldInfo("SourceType", typeof(string))
		}
	};

	[CanBeNull]
	public ActionsHolder OnBecameDamaged => base.Settings?.OnBecameDamaged;

	[CanBeNull]
	public ActionsHolder OnBecameDestroyed => base.Settings?.OnBecameDestroyed;

	public string name => ((AbstractEntityPartComponent)base.Source).Or(null)?.name ?? "<uninitialized-view-based-part>";

	public IEnumerable<DestructionStage> Stages
	{
		get
		{
			ActionsHolder onBecameDestroyed = OnBecameDestroyed;
			if (onBecameDestroyed != null && onBecameDestroyed.HasActions)
			{
				yield return DestructionStage.Destroyed;
			}
		}
	}

	public void ChangeStage(DestructionStage stage, bool onLoad)
	{
		if (onLoad)
		{
			return;
		}
		ActionsHolder actions = GetActions(stage);
		if (actions == null || !actions.HasActions)
		{
			return;
		}
		MechanicEntity mechanicEntity = base.Owner as MechanicEntity;
		MechanicsContext mechanicsContext = (base.Owner as MechanicEntity)?.MainFact.MaybeContext;
		using MechanicsContext mechanicsContext2 = ((mechanicsContext == null && mechanicEntity != null) ? MechanicsContext.Claim(mechanicEntity.Blueprint, mechanicEntity) : null);
		if (mechanicsContext == null)
		{
			mechanicsContext = mechanicsContext2;
		}
		using ((mechanicEntity != null) ? ContextData<MechanicEntityData>.Request().Setup(mechanicEntity) : null)
		{
			using (mechanicsContext?.SetScope())
			{
				actions.Run();
			}
		}
	}

	[CanBeNull]
	private ActionsHolder GetActions(DestructionStage stage)
	{
		return stage switch
		{
			DestructionStage.Whole => null, 
			DestructionStage.Destroyed => OnBecameDestroyed, 
			_ => throw new ArgumentOutOfRangeException("stage", stage, null), 
		};
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
		ViewPartDestructionStagesActions source = new ViewPartDestructionStagesActions();
		result = Unsafe.As<ViewPartDestructionStagesActions, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<ViewPartDestructionStagesActions>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		string value = base.SourceType;
		formatter.StringField(0, "SourceType", ref value, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<ViewPartDestructionStagesActions>();
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
