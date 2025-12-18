using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Code.View.Bridge.Services;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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
public class MechanicActionBarSlotItem : MechanicActionBarSlot, IHashable, IOwlPackable<MechanicActionBarSlotItem>
{
	[JsonProperty]
	[OwlPackInclude]
	public ItemEntityUsable Item;

	public Ability Ability;

	public static readonly TypeInfo OwlPackTypeInfo = new TypeInfo
	{
		Name = "MechanicActionBarSlotItem",
		OldNames = null,
		Fields = new FieldInfo[2]
		{
			new FieldInfo("m_UnitRef", typeof(EntityRef<BaseUnitEntity>)),
			new FieldInfo("Item", typeof(ItemEntityUsable))
		}
	};

	public override MechanicEntityFact AbilityFact => Ability;

	public override string KeyName => Item?.Blueprint?.name;

	protected override bool IsNotAvailable => !(Ability?.Data.IsAvailable ?? false);

	public override bool IsBad()
	{
		if (!base.IsBad() && Item?.Wielder != null)
		{
			return Item?.Wielder != base.Unit;
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
		if (Ability == null)
		{
			return;
		}
		AbilityData itemAbility = Ability.Data;
		if (itemAbility.TargetAnchor != 0)
		{
			Game.Instance.Controllers.SelectedAbilityHandler.SetAbility(itemAbility);
			return;
		}
		if (TurnController.IsInTurnBasedCombat())
		{
			UnitCommandsRunner.CancelMoveCommand();
		}
		UnitCommandsRunner.TryUnitUseAbility(itemAbility, base.Unit);
		EventBus.RaiseEvent(delegate(IAbilityOwnerTargetSelectionHandler h)
		{
			h.HandleOwnerAbilitySelected(itemAbility);
		});
	}

	public override bool IsDisabled(int resourceCount)
	{
		if (resourceCount != 0)
		{
			return base.IsDisabled(resourceCount);
		}
		return true;
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
			h.HandleAbilityTargetHover(Ability.Data, state);
		});
		if (state)
		{
			if (Ability.Data.TargetAnchor == AbilityTargetAnchor.Owner)
			{
				EventBus.RaiseEvent(delegate(IShowAoEAffectedUIHandler h)
				{
					h.HandleAoEMove(base.Unit.Position, Ability.Data);
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

	public override int ActionPointCost()
	{
		if (Ability != null)
		{
			return Ability.Data?.CalculateActionPointCost() ?? (-1);
		}
		return base.ActionPointCost();
	}

	public override int GetResource()
	{
		if (!base.Unit.Body.QuickSlots.Any((UsableSlot s) => s.HasItem && s.Item == Item))
		{
			if (!Item.Blueprint.SpendCharges)
			{
				return -1;
			}
			return 0;
		}
		if (!Item.Blueprint.SpendCharges)
		{
			return -1;
		}
		if (Item.Count <= 1)
		{
			return Item.Charges;
		}
		return Item.Count;
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
		return Item.Icon;
	}

	public override string GetTitle()
	{
		return Item.Name;
	}

	public override string GetDescription()
	{
		return Ability?.Blueprint.GetShortenedDescription() ?? Item.Description;
	}

	public override bool IsCasting()
	{
		if (base.Unit.Commands.Current is UnitUseAbility unitUseAbility)
		{
			return unitUseAbility.Ability.SourceItem == Item;
		}
		return false;
	}

	public override object GetContentData()
	{
		return Item;
	}

	protected override bool CanUseIfTurnBased()
	{
		if (!base.CanUseIfTurnBased())
		{
			return false;
		}
		return Ability.Data != null;
	}

	public override TooltipBaseTemplate GetTooltipTemplate()
	{
		return CombatLogTooltipService.CreateTooltipTemplateItem(Item);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<ItemEntityUsable>.GetHash128(Item);
		result.Append(ref val2);
		return result;
	}

	public static void CreateForDeserialization<TPossiblyBase>(ref TPossiblyBase result)
	{
		MechanicActionBarSlotItem source = new MechanicActionBarSlotItem();
		result = Unsafe.As<MechanicActionBarSlotItem, TPossiblyBase>(ref source);
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
		ushort type = state.TypeLibrary.RegisterType<MechanicActionBarSlotItem>(OwlPackTypeInfo);
		formatter.StartObject(type, OwlPackTypeInfo.Name, objectId);
		formatter.Field(0, "m_UnitRef", ref m_UnitRef, state);
		formatter.Field(1, "Item", ref Item, state);
		formatter.EndObject();
	}

	public override void Deserialize<TFormatter>(TFormatter formatter, uint objectId, DeserializerState state)
	{
		state.References.Register(objectId, this);
		TypeInfo typeInfo = state.TypeLibrary.GetTypeInfo<MechanicActionBarSlotItem>();
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
				Item = formatter.ReadPackable<ItemEntityUsable>(state);
				break;
			}
		}
		formatter.LeaveObject();
	}
}
