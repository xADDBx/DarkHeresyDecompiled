using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SaveSlotCollectionVM : ViewModel
{
	private readonly ReactiveProperty<SaveLoadMode> m_Mode;

	public readonly ReadOnlyReactiveProperty<SaveSlotVM> SelectedSaveSlot;

	public readonly ObservableList<VirtualListElementVMBase> AllTitlesAndSlots = new ObservableList<VirtualListElementVMBase>();

	private readonly ObservableList<SaveSlotVM> m_AllSlots = new ObservableList<SaveSlotVM>();

	private readonly Action m_BackButtonAction;

	public ReadOnlyReactiveProperty<SaveLoadMode> Mode => m_Mode;

	private ObservableList<SaveSlotGroupVM> SaveSlotGroups { get; } = new ObservableList<SaveSlotGroupVM>();


	public SaveSlotCollectionVM(ReactiveProperty<SaveLoadMode> mode, ReadOnlyReactiveProperty<SaveSlotVM> selectedSaveSlot = null, Action backButtonAction = null)
	{
		m_Mode = mode;
		SelectedSaveSlot = selectedSaveSlot;
		m_BackButtonAction = backButtonAction;
	}

	public void HandleNewSave(SaveSlotVM slot)
	{
		SaveSlotGroupVM saveSlotGroupVM = SaveSlotGroups.FirstOrDefault((SaveSlotGroupVM group) => group.GameId == slot.GameId.CurrentValue);
		if (saveSlotGroupVM != null)
		{
			int index = AllTitlesAndSlots.IndexOf(saveSlotGroupVM.LastElement) + 1;
			AllTitlesAndSlots.Insert(index, slot);
			saveSlotGroupVM.HandleNewSave(slot);
		}
		else
		{
			saveSlotGroupVM = new SaveSlotGroupVM(slot, DeleteAllSlots(slot.GameId.CurrentValue)).AddTo(this);
			if (SaveSlotGroups.Empty())
			{
				saveSlotGroupVM.IsFirst = true;
			}
			SaveSlotGroups.Add(saveSlotGroupVM);
			saveSlotGroupVM.HandleNewSave(slot);
			AllTitlesAndSlots.Add(saveSlotGroupVM.ExpandableTitleVM);
			AllTitlesAndSlots.Add(slot);
		}
		m_AllSlots.Add(slot);
	}

	private Action DeleteAllSlots(string groupID)
	{
		return delegate
		{
			List<SaveSlotVM> slotsInGroup = m_AllSlots.Where((SaveSlotVM s) => s.GameId.CurrentValue == groupID).ToList();
			SaveSlotGroupVM saveSlotGroupVM = SaveSlotGroups.FirstOrDefault((SaveSlotGroupVM g) => g.GameId == slotsInGroup.FirstOrDefault()?.GameId.CurrentValue);
			if (saveSlotGroupVM != null)
			{
				for (int num = saveSlotGroupVM.SaveLoadSlots.Count - 1; num >= 0; num--)
				{
					saveSlotGroupVM.SaveLoadSlots[num].DeleteWithoutBox();
				}
				foreach (SaveSlotVM item in slotsInGroup)
				{
					saveSlotGroupVM.TryHandleDeleteSave(item);
					AllTitlesAndSlots.Remove(item);
					m_AllSlots.Remove(item);
				}
				AllTitlesAndSlots.Remove(saveSlotGroupVM.ExpandableTitleVM);
				saveSlotGroupVM.Dispose();
				SaveSlotGroups.Remove(saveSlotGroupVM);
				if (saveSlotGroupVM.IsFirst && SaveSlotGroups.Any())
				{
					SaveSlotGroups.First().IsFirst = true;
				}
				Game.Instance.SaveManager.UpdateSaveListAsync();
			}
		};
	}

	public void HandleDeleteSave(SaveSlotVM slot)
	{
		SaveSlotGroupVM saveSlotGroupVM = SaveSlotGroups.FirstOrDefault((SaveSlotGroupVM group) => group.GameId == slot.GameId.CurrentValue);
		if (saveSlotGroupVM != null && saveSlotGroupVM.TryHandleDeleteSave(slot))
		{
			AllTitlesAndSlots.Remove(saveSlotGroupVM.ExpandableTitleVM);
			saveSlotGroupVM.Dispose();
			SaveSlotGroups.Remove(saveSlotGroupVM);
			if (saveSlotGroupVM.IsFirst && SaveSlotGroups.Any())
			{
				SaveSlotGroups.First().IsFirst = true;
			}
		}
		AllTitlesAndSlots.Remove(slot);
		m_AllSlots.Remove(slot);
		slot.Dispose();
	}

	public void OnBack()
	{
		m_BackButtonAction?.Invoke();
	}

	public void RefreshDLC()
	{
		foreach (SaveSlotVM allSlot in m_AllSlots)
		{
			allSlot.CheckDLC();
		}
	}
}
