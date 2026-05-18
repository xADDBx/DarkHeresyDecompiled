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
	private static readonly IReadOnlyList<VisualSettingsToggle> m_EmptyToggles = new List<VisualSettingsToggle>();

	private readonly Action m_DisposeAction;

	private readonly DollState m_DollState;

	public readonly BaseUnitEntity Unit;

	public readonly IReadOnlyList<VisualSettingsToggle> ToggleVMs;

	public readonly TextureSelectorVM OutfitMainColorSelector;

	private CharacterVisualSettingsVM(Action disposeAction)
	{
		m_DisposeAction = disposeAction;
		ServiceWindowsSounds.Instance.Inventory.VisualSettingsShow.Play();
	}

	public CharacterVisualSettingsVM(DollState dollState, Action disposeAction)
		: this(disposeAction)
	{
		m_DollState = dollState;
		if (dollState == null)
		{
			ToggleVMs = m_EmptyToggles;
			return;
		}
		OutfitMainColorSelector = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Color);
		CreateOutfitColorSelector(secondary: false);
		List<VisualSettingsToggle> list = new List<VisualSettingsToggle>();
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		list.Add(new VisualSettingsToggle(new CharacterVisualSettingsEntityVM(m_DollState.ShowHelm, SwitchHelmet).SetValue(m_DollState.ShowHelm && m_DollState.ShowCloth).SetLock(!m_DollState.ShowCloth).AddTo(this), characterSheet.VisualSettingsShowHelmet));
		list.Add(new VisualSettingsToggle(new CharacterVisualSettingsEntityVM(m_DollState.ShowArmor, SwitchArmor).SetValue(m_DollState.ShowArmor && m_DollState.ShowCloth).SetLock(!m_DollState.ShowCloth).AddTo(this), characterSheet.VisualSettingsShowArmor));
		if (!Unit.HasMechadendrites())
		{
			list.Add(new VisualSettingsToggle(new CharacterVisualSettingsEntityVM(m_DollState.ShowBackpack, SwitchBackpack).SetValue(m_DollState.ShowBackpack && m_DollState.ShowCloth).SetLock(!m_DollState.ShowCloth || Unit.HasMechadendrites()).AddTo(this), characterSheet.VisualSettingsShowBackpack));
		}
		list.Add(new VisualSettingsToggle(new CharacterVisualSettingsEntityVM(m_DollState.ShowCloak, SwitchCloak).SetValue(m_DollState.ShowCloak && m_DollState.ShowCloth).SetLock(!m_DollState.ShowCloth).AddTo(this), characterSheet.VisualSettingsShowCloak));
		list.Add(new VisualSettingsToggle(new CharacterVisualSettingsEntityVM(m_DollState.ShowGloves, SwitchGloves).SetValue(m_DollState.ShowGloves && m_DollState.ShowCloth).SetLock(!m_DollState.ShowCloth).AddTo(this), characterSheet.VisualSettingsShowGloves));
		list.Add(new VisualSettingsToggle(new CharacterVisualSettingsEntityVM(m_DollState.ShowBoots, SwitchBoots).SetValue(m_DollState.ShowBoots && m_DollState.ShowCloth).SetLock(!m_DollState.ShowCloth).AddTo(this), characterSheet.VisualSettingsShowBoots));
		ToggleVMs = list;
		EventBus.Subscribe(this).AddTo(this);
	}

	public CharacterVisualSettingsVM(BaseUnitEntity unit, Action disposeAction)
		: this(disposeAction)
	{
		Unit = unit;
		if (unit == null)
		{
			ToggleVMs = m_EmptyToggles;
			return;
		}
		OutfitMainColorSelector = new TextureSelectorVM(new SelectionGroupRadioVM<TextureSelectorItemVM>(new ObservableList<TextureSelectorItemVM>()), TextureSelectorType.Color);
		CreateOutfitColorSelector(secondary: false);
		List<VisualSettingsToggle> list = new List<VisualSettingsToggle>();
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		CharacterVisualSettingsEntityVM entity = new CharacterVisualSettingsEntityVM(unit.UISettings.ShowHelm, SwitchHelmet).AddTo(this);
		list.Add(new VisualSettingsToggle(entity, characterSheet.VisualSettingsShowHelmet));
		CharacterVisualSettingsEntityVM entity2 = new CharacterVisualSettingsEntityVM(unit.UISettings.ShowArmor, SwitchArmor).AddTo(this);
		list.Add(new VisualSettingsToggle(entity2, characterSheet.VisualSettingsShowArmor));
		if (!unit.HasMechadendrites())
		{
			CharacterVisualSettingsEntityVM entity3 = new CharacterVisualSettingsEntityVM(unit.UISettings.ShowBackpack, SwitchBackpack).AddTo(this);
			list.Add(new VisualSettingsToggle(entity3, characterSheet.VisualSettingsShowBackpack));
		}
		CharacterVisualSettingsEntityVM entity4 = new CharacterVisualSettingsEntityVM(unit.UISettings.ShowCloak, SwitchCloak).AddTo(this);
		list.Add(new VisualSettingsToggle(entity4, characterSheet.VisualSettingsShowCloak));
		CharacterVisualSettingsEntityVM entity5 = new CharacterVisualSettingsEntityVM(unit.UISettings.ShowGloves, SwitchGloves).AddTo(this);
		list.Add(new VisualSettingsToggle(entity5, characterSheet.VisualSettingsShowGloves));
		CharacterVisualSettingsEntityVM entity6 = new CharacterVisualSettingsEntityVM(unit.UISettings.ShowBoots, SwitchBoots).AddTo(this);
		list.Add(new VisualSettingsToggle(entity6, characterSheet.VisualSettingsShowBoots));
		ToggleVMs = list;
	}

	public void Close()
	{
		m_DisposeAction?.Invoke();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		ServiceWindowsSounds.Instance.Inventory.VisualSettingsHide.Play();
	}

	private void SwitchCloth()
	{
		bool showCloth = !m_DollState.ShowCloth;
		Game.Instance.GameCommandQueue.CharGenSwitchCloth(showCloth);
	}

	void ICharGenVisualHandler.HandleShowCloth(bool showCloth)
	{
		if (m_DollState == null)
		{
			return;
		}
		foreach (VisualSettingsToggle toggleVM in ToggleVMs)
		{
			toggleVM.EntityVM.SetValue(m_DollState.ShowCloth).SetLock(!m_DollState.ShowCloth);
		}
		RefreshTextureSelector(OutfitMainColorSelector, secondary: false);
		OutfitMainColorSelector.SetActiveState(m_DollState.ShowCloth);
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

	private void SwitchCloak()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowCloak = !m_DollState.ShowCloak;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowCloak = !Unit.UISettings.ShowCloak;
		}
	}

	private void SwitchGloves()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowGloves = !m_DollState.ShowGloves;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowGloves = !Unit.UISettings.ShowGloves;
		}
	}

	private void SwitchBoots()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowBoots = !m_DollState.ShowBoots;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowBoots = !Unit.UISettings.ShowBoots;
		}
	}

	private void SwitchHood()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowHood = !m_DollState.ShowHood;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowHood = !Unit.UISettings.ShowHood;
		}
	}

	private void SwitchArmor()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowArmor = !m_DollState.ShowArmor;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowArmor = !Unit.UISettings.ShowArmor;
		}
	}

	private void SwitchBaseHeadwear()
	{
		if (m_DollState != null)
		{
			m_DollState.ShowBaseHeadwear = !m_DollState.ShowBaseHeadwear;
		}
		if (Unit != null)
		{
			Unit.UISettings.ShowBaseHeadwear = !Unit.UISettings.ShowBaseHeadwear;
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
