using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Persistence;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SaveSlotGroupVM : VirtualListElementVMBase
{
	private readonly ReactiveCommand<SaveSlotVM> m_OnAddSave = new ReactiveCommand<SaveSlotVM>();

	public readonly SaveSlotsExpandableTitleVM ExpandableTitleVM;

	public readonly string GameId;

	public readonly string GameName;

	public readonly string CharacterName;

	private bool m_IsFirst;

	private readonly ReactiveProperty<bool> m_IsExpanded = new ReactiveProperty<bool>(value: false);

	private readonly Action m_DeleteAll;

	public Observable<SaveSlotVM> OnAddSave => m_OnAddSave;

	public List<SaveSlotVM> SaveLoadSlots { get; private set; }

	public bool IsFirst
	{
		get
		{
			return m_IsFirst;
		}
		set
		{
			m_IsFirst = value;
			if (m_IsFirst)
			{
				ExpandableTitleVM.Expand();
			}
			else
			{
				ExpandableTitleVM.Collapse();
			}
		}
	}

	public ReadOnlyReactiveProperty<bool> IsExpanded => m_IsExpanded;

	public VirtualListElementVMBase LastElement
	{
		get
		{
			if (!SaveLoadSlots.Any())
			{
				return ExpandableTitleVM;
			}
			return SaveLoadSlots.Last();
		}
	}

	public SaveSlotGroupVM(SaveSlotVM slot, Action deleteAll)
	{
		GameId = slot.GameId.CurrentValue;
		GameName = slot.GameName.CurrentValue;
		CharacterName = slot.CharacterName.CurrentValue;
		m_DeleteAll = deleteAll;
		AddDisposable(IsExpanded.Subscribe(ExpandChanged));
		string characterName = CharacterName;
		AddDisposable(ExpandableTitleVM = new SaveSlotsExpandableTitleVM(characterName, SwitchExpand, defaultExpanded: true, HandleDeleteAllSlotsInGroup, (slot.Reference.Type == SaveInfo.SaveType.IronMan) ? slot.Reference : null));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleDeleteAllSlotsInGroup()
	{
		m_DeleteAll?.Invoke();
	}

	public List<VirtualListElementVMBase> GetAll()
	{
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		list.Add(ExpandableTitleVM);
		list.AddRange(SaveLoadSlots);
		return list;
	}

	public void HandleNewSave(SaveSlotVM slot)
	{
		if (SaveLoadSlots == null)
		{
			List<SaveSlotVM> list2 = (SaveLoadSlots = new List<SaveSlotVM>());
		}
		slot.Active.Value = IsExpanded.CurrentValue;
		SaveLoadSlots.Add(slot);
		m_OnAddSave.Execute(slot);
	}

	public void RemoveSlot(SaveSlotVM slot)
	{
		SaveLoadSlots?.Remove(slot);
	}

	private void SwitchExpand(bool expand)
	{
		m_IsExpanded.Value = expand;
	}

	private void ExpandChanged(bool expand)
	{
		SaveLoadSlots?.ForEach(delegate(SaveSlotVM f)
		{
			f.SetAvailableAndActive(expand);
		});
	}
}
