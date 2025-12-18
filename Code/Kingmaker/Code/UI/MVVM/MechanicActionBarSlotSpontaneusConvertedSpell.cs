using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using Owlcat.UI;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[OwlPackable(OwlPackableMode.Generate)]
public class MechanicActionBarSlotSpontaneusConvertedSpell : MechanicActionBarSlot, IHashable, IOwlPackable<MechanicActionBarSlotSpontaneusConvertedSpell>
{
	[JsonProperty]
	[OwlPackInclude]
	public AbilityData Spell;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MechanicActionBarSlotSpontaneusConvertedSpell",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_UnitRef", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("Spell", typeof(AbilityData))
		}
	};

	public override MechanicEntityFact AbilityFact => Spell?.Fact;

	public override string KeyName => Spell?.Blueprint?.name;

	protected override bool IsNotAvailable
	{
		get
		{
			if (Spell == null)
			{
				return true;
			}
			return !Spell.IsAvailable;
		}
	}

	public override bool IsDisabled(int resourceCount)
	{
		return !base.Unit.LifeState.IsConscious;
	}

	public override void OnClick()
	{
		base.OnClick();
		if (IsPossibleActive)
		{
			TrySetAbilityToPrediction(state: true);
			if (Spell.TargetAnchor != 0)
			{
				Game.Instance.Controllers.SelectedAbilityHandler.SetAbility(Spell);
			}
			else
			{
				UnitCommandsRunner.TryUnitUseAbility(Spell, base.Unit);
			}
		}
	}

	public override void OnHover(bool state)
	{
		base.OnHover(state);
		EventBus.RaiseEvent(delegate(IAbilityTargetHoverUIHandler h)
		{
			h.HandleAbilityTargetHover(Spell, state);
		});
	}

	public override int GetResource()
	{
		return Spell.GetAvailableForCastCount();
	}

	public override int GetResourceCost()
	{
		return Spell?.GetResourceCost() ?? (-1);
	}

	public override int GetResourceAmount()
	{
		return Spell?.GetResourceAmount() ?? (-1);
	}

	public override Sprite GetIcon()
	{
		return Spell.Icon;
	}

	public override string GetTitle()
	{
		return Spell.Name;
	}

	public override string GetDescription()
	{
		return Spell.ShortenedDescription;
	}

	public override Sprite GetForeIcon()
	{
		return (Spell.ConvertedFrom ?? Spell).Blueprint.Icon;
	}

	public override bool IsCasting()
	{
		if (base.Unit.Commands.Current is UnitUseAbility unitUseAbility)
		{
			if (unitUseAbility.Ability.Blueprint != Spell.Blueprint)
			{
				return Spell.Blueprint.SameAbility(unitUseAbility.Ability.Blueprint.Parent);
			}
			return true;
		}
		return false;
	}

	public override object GetContentData()
	{
		return Spell;
	}

	protected override string WarningMessage(Vector3 castPosition)
	{
		string text = base.WarningMessage(castPosition);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return Spell?.GetUnavailableReason(castPosition);
	}

	protected override bool CanUseIfTurnBased()
	{
		if (base.CanUseIfTurnBased())
		{
			return Spell != null;
		}
		return false;
	}

	public override TooltipBaseTemplate GetTooltipTemplate()
	{
		return CombatLogTooltipService.CreateTooltipTemplateAbility(Spell);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<AbilityData>.GetHash128(Spell);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MechanicActionBarSlotSpontaneusConvertedSpell source = new MechanicActionBarSlotSpontaneusConvertedSpell();
		result = Unsafe.As<MechanicActionBarSlotSpontaneusConvertedSpell, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MechanicActionBarSlotSpontaneusConvertedSpell>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UnitRef", ref m_UnitRef, state);
		formatter.Field(1, "Spell", ref Spell, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MechanicActionBarSlotSpontaneusConvertedSpell>();
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
				Spell = formatter.ReadPackable<AbilityData>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
