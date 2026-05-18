using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BuffVM : ViewModel, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitBuffHandler<EntitySubscriber>, IEventTag<IUnitBuffHandler, EntitySubscriber>, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, IEntitySubscriber
{
	private readonly ReactiveProperty<bool> m_ShowNonStackNotification = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<int> m_Rank = new ReactiveProperty<int>();

	private readonly UnitPartNonStackBonuses m_NonStackBonus;

	public ReadOnlyReactiveProperty<bool> ShowNonStackNotification => m_ShowNonStackNotification;

	public ReadOnlyReactiveProperty<int> Rank => m_Rank;

	public Buff Buff { get; }

	public bool DealsDamage { get; }

	public BuffGroupType Group { get; private set; }

	public int SortOrder { get; private set; }

	public Sprite Icon { get; }

	public BuffVM(Buff buff)
	{
		Buff = buff;
		m_NonStackBonus = Buff?.Owner?.GetOptional<UnitPartNonStackBonuses>();
		Icon = buff.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		m_ShowNonStackNotification.Value = ShowNonStackWarning();
		m_Rank.Value = buff.Rank;
		DealsDamage = buff.Blueprint?.GetComponent<DOTLogicVisual>() != null;
		SetGroup();
		SetSortOrder();
		EventBus.Subscribe(this).AddTo(this);
	}

	public IEntity GetSubscribingEntity()
	{
		return Buff?.Owner;
	}

	public bool IsSpecialBuff()
	{
		return Buff.Blueprint.BuffUISettings?.ShowInSpecial ?? false;
	}

	public bool IsImportantBuff()
	{
		return Buff.Blueprint.BuffUISettings?.CheckImportantBuffConditions(GetBuffTargetType()) ?? false;
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (ShouldHandle(slot.Owner))
		{
			m_ShowNonStackNotification.Value = ShowNonStackWarning();
		}
	}

	public void HandleBuffDidAdded(Buff buff, MechanicEntity caster)
	{
		if (ShouldHandle(buff.Owner))
		{
			m_ShowNonStackNotification.Value = ShowNonStackWarning();
			UpdateRank();
		}
	}

	public void HandleBuffDidRemoved(Buff buff, MechanicEntity caster)
	{
		if (ShouldHandle(buff.Owner))
		{
			m_ShowNonStackNotification.Value = ShowNonStackWarning();
			UpdateRank();
		}
	}

	public void HandleBuffRankIncreased(Buff buff, int delta, MechanicEntity caster)
	{
		if (ShouldHandle(buff.Owner))
		{
			UpdateRank();
		}
	}

	public void HandleBuffRankDecreased(Buff buff, int delta, MechanicEntity caster)
	{
		if (ShouldHandle(buff.Owner))
		{
			UpdateRank();
		}
	}

	private bool ShowNonStackWarning()
	{
		if (m_NonStackBonus != null)
		{
			return m_NonStackBonus.ShouldShowWarning(Buff);
		}
		return false;
	}

	private void SetGroup()
	{
		if (Buff == null)
		{
			return;
		}
		BuffUISettings buffUISettings = Buff.Blueprint.BuffUISettings;
		if (buffUISettings != null)
		{
			BuffGroupType group = buffUISettings.GetGroup(GetBuffTargetType());
			if (group != BuffGroupType.None)
			{
				Group = group;
				return;
			}
		}
		if (Buff.Blueprint.IsDOTVisual)
		{
			Group = BuffGroupType.DOT;
			return;
		}
		if (Buff.Blueprint.IsCriticalEffect)
		{
			Group = BuffGroupType.CriticalEffect;
			return;
		}
		BaseUnitEntity owner = Buff.Owner;
		bool flag = (owner != null && owner.IsEnemy(Buff.Context.MaybeCaster)) || Buff.Blueprint.CriticalEffect;
		Group = (flag ? BuffGroupType.Negative : BuffGroupType.Positive);
	}

	private BuffTargetType GetBuffTargetType()
	{
		BaseUnitEntity owner = Buff.Owner;
		if (owner != null && owner.IsPlayerEnemy)
		{
			return BuffTargetType.Enemy;
		}
		BaseUnitEntity owner2 = Buff.Owner;
		if (owner2 != null && owner2.IsPlayerFaction)
		{
			return BuffTargetType.Ally;
		}
		return BuffTargetType.All;
	}

	private void SetSortOrder()
	{
		if (Buff != null)
		{
			if (Buff.Blueprint.HasBuffOverrideUIOrder)
			{
				SortOrder = 0;
			}
			else if (!Buff.Blueprint.IsDOTVisual)
			{
				SortOrder = 1;
			}
			else
			{
				SortOrder = 2;
			}
		}
	}

	private void UpdateRank()
	{
		m_Rank.Value = Buff.Rank;
	}

	private bool ShouldHandle(MechanicEntity owner)
	{
		return Buff.Owner == owner;
	}
}
