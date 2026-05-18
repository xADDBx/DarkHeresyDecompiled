using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Code.Framework;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using Owlcat.UI;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[OwlPackable(OwlPackableMode.Generate)]
public class MechanicActionBarSlotToggleAbility : MechanicActionBarSlot, IOwlPackable<MechanicActionBarSlotToggleAbility>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityFactRef<ToggleAbility> m_AbilityRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MechanicActionBarSlotToggleAbility",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_UnitRef", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("m_AbilityRef", typeof(EntityFactRef<ToggleAbility>))
		}
	};

	[CanBeNull]
	public ToggleAbility MaybeAbility => m_AbilityRef;

	[NotNull]
	public ToggleAbility Ability
	{
		get
		{
			return MaybeAbility ?? throw new NullReferenceException();
		}
		set
		{
			m_AbilityRef = value ?? throw new NullReferenceException();
		}
	}

	protected override bool IsNotAvailable => !(MaybeAbility?.IsRestrictionsPassed ?? false);

	public override MechanicEntityFact AbilityFact => MaybeAbility;

	public override string KeyName => MaybeAbility?.Blueprint.name;

	public override bool IsBad()
	{
		if (!base.IsBad())
		{
			return !(MaybeAbility?.Active ?? false);
		}
		return true;
	}

	public override void OnClick()
	{
		base.OnClick();
		if (IsPossibleActive)
		{
			TrySetAbilityToPrediction(state: true);
			UnitCommandsRunner.TryUnitToggleAbility(base.Unit, Ability);
		}
	}

	public override int GetResource()
	{
		return -1;
	}

	public override int GetResourceCost()
	{
		return -1;
	}

	public override int GetResourceAmount()
	{
		return -1;
	}

	public override Sprite GetIcon()
	{
		return Ability.Icon;
	}

	public override string GetTitle()
	{
		return Ability.Name;
	}

	public override string GetDescription()
	{
		return Ability.Description;
	}

	public override bool IsActive()
	{
		return Ability.Enabled;
	}

	public override bool IsCasting()
	{
		return false;
	}

	public override object GetContentData()
	{
		return Ability;
	}

	public override TooltipBaseTemplate GetTooltipTemplate()
	{
		return CombatLogTooltipService.CreateTooltipTemplateToggleAbility(Ability, base.Unit);
	}

	protected override string WarningMessage(Vector3 castPosition)
	{
		string text = base.WarningMessage(castPosition);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (MaybeAbility != null && !MaybeAbility.CheckRestrictionsPassed(out IAbilityCasterRestriction failedRestriction) && failedRestriction != null)
		{
			return failedRestriction.GetAbilityCasterRestrictionUIText(Ability.Owner);
		}
		return string.Empty;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MechanicActionBarSlotToggleAbility source = new MechanicActionBarSlotToggleAbility();
		result = Unsafe.As<MechanicActionBarSlotToggleAbility, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MechanicActionBarSlotToggleAbility>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UnitRef", ref m_UnitRef, state);
		formatter.Field(1, "m_AbilityRef", ref m_AbilityRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MechanicActionBarSlotToggleAbility>();
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
				m_UnitRef = formatter.ReadPackable<EntityRef<BaseUnitEntity>>(state);
				break;
			case 1:
				m_AbilityRef = formatter.ReadPackable<EntityFactRef<ToggleAbility>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
