using System.Collections.Generic;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Mechadendrites;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartWeaponsVM : ActionBarBasePartVM, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, INetRoleSetHandler
{
	private readonly ReactiveProperty<bool> m_CanSwitchSets = new ReactiveProperty<bool>();

	public readonly List<ActionBarPartWeaponSetVM> Sets = new List<ActionBarPartWeaponSetVM>();

	private readonly ReactiveProperty<ActionBarPartWeaponSetVM> m_CurrentSet = new ReactiveProperty<ActionBarPartWeaponSetVM>();

	private readonly ReactiveProperty<int> m_CurrentSetIndex = new ReactiveProperty<int>(-1);

	public ReadOnlyReactiveProperty<bool> CanSwitchSets => m_CanSwitchSets;

	public ReadOnlyReactiveProperty<ActionBarPartWeaponSetVM> CurrentSet => m_CurrentSet;

	public ReadOnlyReactiveProperty<int> CurrentSetIndex => m_CurrentSetIndex;

	public ActionBarPartWeaponsVM()
	{
		EventBus.Subscribe(this).AddTo(this);
		CurrentSet.Subscribe(delegate(ActionBarPartWeaponSetVM currentSet)
		{
			Sets.ForEach(delegate(ActionBarPartWeaponSetVM s)
			{
				s.UpdateIsCurrent(s == currentSet);
			});
		}).AddTo(this);
	}

	protected override void OnUnitChanged()
	{
		RefreshCanSwitchSets();
		for (int i = 0; i < Unit.Entity.Body.HandsEquipmentSets.Count; i++)
		{
			if (i >= Sets.Count)
			{
				Sets.Add(new ActionBarPartWeaponSetVM());
			}
			int index = i;
			Sets[i].InitForUnit(index, Unit, Unit.Entity.Body.HandsEquipmentSets[i], delegate
			{
				SetCurrentEquipmentSet(index);
			});
			if (Unit.Entity.Body.CurrentHandsEquipmentSet == Unit.Entity.Body.HandsEquipmentSets[i])
			{
				m_CurrentSet.Value = Sets[i];
				m_CurrentSetIndex.Value = index;
			}
		}
		if (Unit.Entity.Body.IsCurrentHandsEquipmentSetPolymorphed)
		{
			m_CurrentSet.Value = null;
			m_CurrentSetIndex.Value = -1;
		}
		for (int j = Unit.Entity.Body.HandsEquipmentSets.Count; j < Sets.Count; j++)
		{
			Sets[j].Dispose();
		}
		Sets.RemoveRange(Unit.Entity.Body.HandsEquipmentSets.Count, Sets.Count - Unit.Entity.Body.HandsEquipmentSets.Count);
	}

	protected override void ClearSlots()
	{
		Sets.ForEach(delegate(ActionBarPartWeaponSetVM setVm)
		{
			setVm.Dispose();
		});
		Sets.Clear();
	}

	private void SetCurrentEquipmentSet(int index)
	{
		if (CanSwitchSets.CurrentValue)
		{
			bool isPreparationTurn = Game.Instance.Controllers.TurnController.IsPreparationTurn;
			bool flag = Game.Instance.Controllers.TurnController.TurnBasedModeActive && !Game.Instance.Controllers.TurnController.IsPlayerTurn;
			if (!(!isPreparationTurn && flag) && Unit.Entity != null)
			{
				Game.Instance.GameCommandQueue.SwitchHandEquipment(Unit.Entity, index);
			}
		}
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (slot.Owner == Unit.Entity)
		{
			Sets.ForEach(delegate(ActionBarPartWeaponSetVM s)
			{
				s.UpdateSlots();
			});
		}
	}

	public void ChangeWeaponSet()
	{
		SetCurrentEquipmentSet((Unit.Entity.Body.CurrentHandEquipmentSetIndex == 0) ? 1 : 0);
	}

	void IUnitActiveEquipmentSetHandler.HandleUnitChangeActiveEquipmentSet()
	{
		PartUnitBody body = Unit.Entity.Body;
		m_CurrentSet.Value = Sets.FirstOrDefault((ActionBarPartWeaponSetVM s) => s.HandSet == body.CurrentHandsEquipmentSet);
		m_CurrentSetIndex.Value = body.CurrentHandEquipmentSetIndex;
	}

	void INetRoleSetHandler.HandleRoleSet(string entityId)
	{
		RefreshCanSwitchSets();
	}

	private void RefreshCanSwitchSets()
	{
		m_CanSwitchSets.Value = Unit.Entity.IsDirectlyControllable() && !Unit.Entity.HasMechadendrites();
	}
}
