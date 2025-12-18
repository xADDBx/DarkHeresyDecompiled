using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using Owlcat.UI;
using OwlPack.Runtime;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

[OwlPackable(OwlPackableMode.Generate)]
public class MechanicActionBarSlotAbility : MechanicActionBarSlot, IHashable, IOwlPackable<MechanicActionBarSlotAbility>
{
	[JsonProperty]
	[OwlPackInclude]
	private EntityFactRef<Ability> m_AbilityRef;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MechanicActionBarSlotAbility",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_UnitRef", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("m_AbilityRef", typeof(EntityFactRef<Ability>))
		}
	};

	[CanBeNull]
	public Ability Replacement { get; set; }

	public AbilityData Ability
	{
		get
		{
			return Replacement?.Data ?? OriginalAbility;
		}
		set
		{
			m_AbilityRef = value.Fact;
		}
	}

	[CanBeNull]
	public AbilityData OriginalAbility => m_AbilityRef.Fact?.MaybeData;

	public override MechanicEntityFact AbilityFact => m_AbilityRef.Fact;

	private bool IsVariantAbility => Ability?.IsVariable ?? false;

	public override string KeyName => OriginalAbility?.Blueprint.Name;

	public override bool IsPossibleActive => CanActivateAbility();

	protected override bool IsNotAvailable
	{
		get
		{
			if (Ability == null)
			{
				return true;
			}
			if (Ability.Caster == null)
			{
				return true;
			}
			if (IsVariantAbility)
			{
				return Ability.IsOnCooldown;
			}
			return !Ability.IsAvailable;
		}
	}

	public override bool IsBad()
	{
		if (!base.IsBad() && Ability?.Fact != null)
		{
			return Ability?.Caster == null;
		}
		return true;
	}

	public override bool IsDisabled(int resourceCount)
	{
		if (base.Unit.LifeState.IsConscious)
		{
			if (base.Unit.IsInCombat)
			{
				return base.Unit.View.IsMoving();
			}
			return false;
		}
		return true;
	}

	public override void OnClick()
	{
		base.OnClick();
		if (!IsPossibleActive)
		{
			return;
		}
		TrySetAbilityToPrediction(state: true);
		if (Ability.TargetAnchor != 0)
		{
			Game.Instance.Controllers.SelectedAbilityHandler.SetAbility(Ability);
			return;
		}
		if (TurnController.IsInTurnBasedCombat())
		{
			UnitCommandsRunner.CancelMoveCommand();
		}
		UnitCommandsRunner.TryUnitUseAbility(Ability, base.Unit);
		EventBus.RaiseEvent(delegate(IAbilityOwnerTargetSelectionHandler h)
		{
			h.HandleOwnerAbilitySelected(Ability);
		});
	}

	public override void OnHover(bool state)
	{
		base.OnHover(state);
		if (Ability == null)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IAbilityTargetHoverUIHandler h)
		{
			h.HandleAbilityTargetHover(Ability, state);
		});
		if (state)
		{
			if (Ability.TargetAnchor == AbilityTargetAnchor.Owner)
			{
				EventBus.RaiseEvent(delegate(IShowAoEAffectedUIHandler h)
				{
					h.HandleAoEMove(base.Unit.Position, Ability);
				});
			}
		}
		else
		{
			EventBus.RaiseEvent(delegate(IShowAoEAffectedUIHandler h)
			{
				h.HandleAoECancel();
			});
		}
	}

	public override int GetResource()
	{
		if (Ability == null)
		{
			return 0;
		}
		FakeResourceCharges component = Ability.Blueprint.GetComponent<FakeResourceCharges>();
		MechanicsContext mechanicsContext = Ability.Fact?.Context;
		if (component != null && mechanicsContext != null)
		{
			return component.Charges.Calculate(mechanicsContext);
		}
		if (IsVariantAbility)
		{
			return -1;
		}
		return Ability.GetAvailableForCastCount();
	}

	public override int GetResourceCost()
	{
		if (IsVariantAbility)
		{
			return -1;
		}
		return Ability?.GetResourceCost() ?? 0;
	}

	public override int GetResourceAmount()
	{
		if (IsVariantAbility)
		{
			return -1;
		}
		return Ability?.GetResourceAmount() ?? 0;
	}

	public override int ActionPointCost()
	{
		if (IsVariantAbility)
		{
			return -1;
		}
		return Ability?.CalculateActionPointCost() ?? 0;
	}

	public override Sprite GetIcon()
	{
		return Ability?.Icon;
	}

	public override string GetTitle()
	{
		return Ability?.Name;
	}

	public override string GetDescription()
	{
		return Ability?.ShortenedDescription;
	}

	public override bool HasWeaponAbilityGroup()
	{
		if (Ability == null)
		{
			return false;
		}
		foreach (BlueprintAbilityGroup abilityGroup in Ability.Blueprint.AbilityGroups)
		{
			if (abilityGroup.NameSafe() == "WeaponAttackAbilityGroup")
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsCasting()
	{
		if (base.Unit?.Commands?.Current is UnitUseAbility unitUseAbility)
		{
			if (unitUseAbility.Ability.Blueprint == Ability.Blueprint || Ability.Blueprint.SameAbility(unitUseAbility.Ability.Blueprint.Parent))
			{
				return unitUseAbility.Result == AbstractUnitCommand.ResultType.None;
			}
			return false;
		}
		return false;
	}

	public override object GetContentData()
	{
		return Ability;
	}

	protected override string WarningMessage(Vector3 castPosition)
	{
		string text = base.WarningMessage(castPosition);
		if (string.IsNullOrEmpty(text))
		{
			return Ability?.GetUnavailableReason(castPosition);
		}
		return text;
	}

	public override IEnumerable<AbilityData> GetConvertedAbilityData()
	{
		if (!(Ability != null))
		{
			return base.GetConvertedAbilityData();
		}
		return Ability.GetConversions();
	}

	public override TooltipBaseTemplate GetTooltipTemplate()
	{
		if (!(Ability != null))
		{
			return null;
		}
		return CombatLogTooltipService.CreateTooltipTemplateAbility(Ability);
	}

	private bool CanActivateAbility()
	{
		bool isPossibleActive = base.IsPossibleActive;
		AbilityData ability = Ability;
		if (!isPossibleActive || ability == null)
		{
			return isPossibleActive;
		}
		if (ability.Blueprint.IsMoveUnit)
		{
			MechanicEntity caster = ability.Caster;
			if (caster != null)
			{
				return caster.CanMove;
			}
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityFactRef<Ability> obj = m_AbilityRef;
		Hash128 val2 = StructHasher<EntityFactRef<Kingmaker.UnitLogic.Abilities.Ability>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MechanicActionBarSlotAbility source = new MechanicActionBarSlotAbility();
		result = Unsafe.As<MechanicActionBarSlotAbility, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MechanicActionBarSlotAbility>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UnitRef", ref m_UnitRef, state);
		formatter.Field(1, "m_AbilityRef", ref m_AbilityRef, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MechanicActionBarSlotAbility>();
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
				m_AbilityRef = formatter.ReadPackable<EntityFactRef<Ability>>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
