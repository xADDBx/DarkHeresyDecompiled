using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.View.Mechadendrites;
using Kingmaker.Visual.CharacterSystem;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharacterVisualSettingsVM : ViewModel, ICharGenVisualHandler, ISubscriber
{
	public readonly BaseUnitEntity Unit;

	private readonly DollState m_DollState;

	public readonly CharacterVisualSettingsEntityVM Cloth;

	public readonly CharacterVisualSettingsEntityVM Helmet;

	public readonly CharacterVisualSettingsEntityVM HelmetAboveAll;

	public readonly CharacterVisualSettingsEntityVM Backpack;

	public readonly TextureSelectorVM OutfitMainColorSelector;

	private readonly Action m_DisposeAction;

	private CharacterVisualSettingsVM(Action disposeAction)
	{
		m_DisposeAction = disposeAction;
		UISounds.Instance.Sounds.Inventory.InventoryVisualSettingsShow.Play();
	}

	public CharacterVisualSettingsVM(DollState dollState, Action disposeAction)
		: this(disposeAction)
	{
		m_DollState = dollState;
		if (dollState != null)
		{
			OutfitMainColorSelector = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Paged);
			CreateOutfitColorSelector(secondary: false);
			EventBus.Subscribe(this).AddTo(this);
			Cloth = new CharacterVisualSettingsEntityVM(m_DollState.ShowCloth, SwitchCloth).AddTo(this);
			Helmet = new CharacterVisualSettingsEntityVM(m_DollState.ShowHelm, SwitchHelmet).AddTo(this);
			HelmetAboveAll = new CharacterVisualSettingsEntityVM(m_DollState.ShowHelmAboveAll, SwitchHelmetAboveAll).AddTo(this);
			Backpack = new CharacterVisualSettingsEntityVM(m_DollState.ShowBackpack, SwitchBackpack).AddTo(this);
			Helmet.SetValue(m_DollState.ShowHelm && m_DollState.ShowCloth);
			HelmetAboveAll.SetValue(m_DollState.ShowHelmAboveAll);
			Backpack.SetValue(m_DollState.ShowBackpack && m_DollState.ShowCloth);
			Helmet.SetLock(!m_DollState.ShowCloth);
			Backpack.SetLock(!m_DollState.ShowCloth && Unit.HasMechadendrites());
		}
	}

	public CharacterVisualSettingsVM(BaseUnitEntity unit, Action disposeAction)
		: this(disposeAction)
	{
		Unit = unit;
		if (unit != null)
		{
			OutfitMainColorSelector = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Paged);
			CreateOutfitColorSelector(secondary: false);
			Helmet = new CharacterVisualSettingsEntityVM(Unit.UISettings.ShowHelm, SwitchHelmet).AddTo(this);
			HelmetAboveAll = new CharacterVisualSettingsEntityVM(Unit.UISettings.ShowHelmAboveAll, SwitchHelmetAboveAll).AddTo(this);
			Backpack = new CharacterVisualSettingsEntityVM(Unit.UISettings.ShowBackpack, SwitchBackpack).AddTo(this);
			Backpack.SetLock(Unit.HasMechadendrites());
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		UISounds.Instance.Sounds.Inventory.InventoryVisualSettingsHide.Play();
	}

	public void Close()
	{
		m_DisposeAction?.Invoke();
	}

	private void SwitchCloth()
	{
		bool showCloth = !m_DollState.ShowCloth;
		Game.Instance.GameCommandQueue.CharGenSwitchCloth(showCloth);
	}

	void ICharGenVisualHandler.HandleShowCloth(bool showCloth)
	{
		if (m_DollState != null)
		{
			Helmet.SetValue(m_DollState.ShowCloth);
			Backpack.SetValue(m_DollState.ShowCloth);
			Helmet.SetLock(!m_DollState.ShowCloth);
			Backpack.SetLock(!m_DollState.ShowCloth && Unit.HasMechadendrites());
			RefreshTextureSelector(OutfitMainColorSelector, secondary: false);
			OutfitMainColorSelector.SetActiveState(m_DollState.ShowCloth);
		}
	}

	private void SwitchHelmet()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowHelm = !m_DollState.ShowHelm;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowHelm = !Unit.UISettings.ShowHelm;
		}
	}

	private void SwitchHelmetAboveAll()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowHelmAboveAll = !m_DollState.ShowHelmAboveAll;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowHelmAboveAll = !Unit.UISettings.ShowHelmAboveAll;
		}
	}

	private void SwitchBackpack()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowBackpack = !m_DollState.ShowBackpack;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowBackpack = !Unit.UISettings.ShowBackpack;
		}
	}

	private void CreateOutfitColorSelector(bool secondary)
	{
		RefreshTextureSelector(OutfitMainColorSelector, secondary);
		string title = (secondary ? UIStrings.Instance.CharGen.SecondaryClothColor : UIStrings.Instance.CharGen.PrimaryClothColor);
		OutfitMainColorSelector.SetTitle(title);
		OutfitMainColorSelector.SetNoItemsDescription((m_DollState == null) ? UIStrings.Instance.CharacterSheet.VisualSettingsDisabledForCharacter : UIStrings.Instance.CharacterSheet.VisualSettingsEnableClothes);
	}

	private void RefreshTextureSelector(TextureSelectorVM selector, bool secondary)
	{
		if (!TryGetDollColors(m_DollState, out var colorsProfile, out var colorPreset, secondary) && !TryGetUnitColors(Unit, out colorsProfile, out colorPreset, secondary))
		{
			selector.SetActiveState(state: false);
			return;
		}
		ObservableList<TextureSelectorItemVM> entitiesCollection = selector.SelectionGroup.EntitiesCollection;
		int count = colorPreset.IndexPairs.Count;
		for (int i = 0; i < count; i++)
		{
			RampColorPreset.IndexSet preset = colorPreset.IndexPairs[i];
			int primaryIndex = preset.PrimaryIndex;
			if (primaryIndex < 0 || primaryIndex >= colorsProfile.Ramps.Count)
			{
				continue;
			}
			Texture2D item = colorsProfile.Ramps[primaryIndex];
			CharGenAppearanceComponentFactory.GetTextureSelectorItemVM(entitiesCollection, i, item, delegate
			{
				if (m_DollState != null)
				{
					SetDollEquipmentColor(m_DollState, preset);
				}
				else if (Unit != null)
				{
					SetUnitEquipmentColor(Unit, preset);
				}
			});
			bool num = GetCurrentEquipmentRampIndex(secondary: false) == preset.PrimaryIndex;
			bool flag = GetCurrentEquipmentRampIndex(secondary: true) == preset.SecondaryIndex;
			if (num && flag)
			{
				selector.SelectionGroup.TrySelectEntity(entitiesCollection[i]);
			}
		}
		selector.SelectionGroup.ClearFromIndex(count);
		selector.SetActiveState(count > 0);
	}

	private bool TryGetDollColors(DollState dollState, out CharacterColorsProfile colorsProfile, out RampColorPreset colorPreset, bool secondary)
	{
		if (dollState?.Clothes == null)
		{
			colorsProfile = null;
			colorPreset = null;
			return false;
		}
		colorsProfile = CharGenUtility.GetClothesColorsProfile(m_DollState.Clothes, out colorPreset, secondary);
		if (colorsProfile != null)
		{
			return colorPreset != null;
		}
		return false;
	}

	private bool TryGetUnitColors(BaseUnitEntity unit, out CharacterColorsProfile colorsProfile, out RampColorPreset colorPreset, bool secondary)
	{
		if (unit?.View == null || !unit.View.CharacterAvatar)
		{
			colorsProfile = null;
			colorPreset = null;
			return false;
		}
		List<EquipmentEntity> equipmentEntities = unit.View.CharacterAvatar.EquipmentEntities;
		colorsProfile = CharGenUtility.GetClothesColorsProfile(equipmentEntities, out colorPreset, secondary);
		if (colorsProfile != null)
		{
			return colorPreset != null;
		}
		return false;
	}

	private int GetCurrentEquipmentRampIndex(bool secondary)
	{
		if (m_DollState != null)
		{
			if (!secondary)
			{
				return m_DollState.EquipmentRampIndex;
			}
			return m_DollState.EquipmentRampIndexSecondary;
		}
		if (Unit?.ViewSettings.Doll != null)
		{
			if (!secondary)
			{
				return Unit.ViewSettings.Doll.ClothesPrimaryIndex;
			}
			return Unit.ViewSettings.Doll.ClothesSecondaryIndex;
		}
		return -1;
	}

	private void SetDollEquipmentColor(DollState dollState, RampColorPreset.IndexSet rampIndex)
	{
		if (dollState == null)
		{
			PFLog.UI.Error("Failed to set equipment color. Reason: dollState is null");
		}
		else if (m_DollState.EquipmentRampIndex != rampIndex.PrimaryIndex || m_DollState.EquipmentRampIndexSecondary != rampIndex.SecondaryIndex)
		{
			Game.Instance.GameCommandQueue.CharGenSetEquipmentColor(rampIndex.PrimaryIndex, rampIndex.SecondaryIndex);
		}
	}

	private void SetUnitEquipmentColor(BaseUnitEntity unit, RampColorPreset.IndexSet rampIndex)
	{
		if (unit == null)
		{
			PFLog.UI.Error("Failed to set equipment color. Reason: unit is null");
		}
		else
		{
			Game.Instance.GameCommandQueue.SetEquipmentColor(Unit, rampIndex);
		}
	}
}
