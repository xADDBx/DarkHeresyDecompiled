using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code.View.UI.MVVM;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CharacterInfoVM : ViewModel
{
	private readonly ReactiveProperty<MenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<MenuEntityVM>();

	private readonly ReactiveProperty<BaseUnitEntity> m_Unit;

	private readonly ReactiveProperty<bool> m_HasModifierUpgrade = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<CharInfoAbilitiesTabVM> m_AbilitiesVM = new ReactiveProperty<CharInfoAbilitiesTabVM>();

	private readonly ReactiveProperty<CharInfoArchetypeTabVM> m_ArchetypeVM = new ReactiveProperty<CharInfoArchetypeTabVM>();

	private readonly ReactiveProperty<CharInfoConvictionsTabVM> m_ConvictionsVM = new ReactiveProperty<CharInfoConvictionsTabVM>();

	private readonly ReactiveProperty<CharInfoCharacteristicsTabVM> m_CharacteristicsVM = new ReactiveProperty<CharInfoCharacteristicsTabVM>();

	private ViewModel m_CurrentWindow;

	public readonly MenuVM MenuVM;

	public readonly PartyVM PartyVM;

	public readonly CharInfoBigPortraitVM PortraitVM;

	public ReadOnlyReactiveProperty<bool> HasModifierUpgrade => m_HasModifierUpgrade;

	public ReadOnlyReactiveProperty<CharInfoAbilitiesTabVM> AbilitiesVM => m_AbilitiesVM;

	public ReadOnlyReactiveProperty<CharInfoArchetypeTabVM> ArchetypeVM => m_ArchetypeVM;

	public ReadOnlyReactiveProperty<CharInfoConvictionsTabVM> ConvictionsVM => m_ConvictionsVM;

	public ReadOnlyReactiveProperty<CharInfoCharacteristicsTabVM> CharacteristicsVM => m_CharacteristicsVM;

	public CharacterInfoVM(CharInfoPageType type, ReactiveProperty<BaseUnitEntity> unit)
	{
		CharacterInfoVM characterInfoVM = this;
		m_Unit = unit;
		List<MenuEntityVM> list = new List<MenuEntityVM>
		{
			GetMenuEntity(CharInfoPageType.Convictions),
			GetMenuEntity(CharInfoPageType.Characteristics),
			GetMenuEntity(CharInfoPageType.Archetype),
			GetMenuEntity(CharInfoPageType.Abilities)
		};
		m_SelectedMenuEntity.Value = list.FirstOrDefault((MenuEntityVM e) => e.EnumId == (int)type);
		MenuVM = new MenuVM(list, m_SelectedMenuEntity).AddTo(this);
		m_SelectedMenuEntity.Subscribe(delegate(MenuEntityVM e)
		{
			characterInfoVM.HandleSelectedMenu(e.EnumId);
		}).AddTo(this);
		new CharInfoSkillsAndWeaponsVM(m_Unit).AddTo(this);
		PartyVM = new PartyVM().AddTo(this);
		PortraitVM = new CharInfoBigPortraitVM(m_Unit).AddTo(this);
		m_Unit.Subscribe(delegate
		{
			characterInfoVM.CheckModifiers();
		}).AddTo(this);
		Metrics.Interface.InterfaceType(InterfaceMetricsEvent.InterfaceTypes.CharScreen).InterfaceState(InterfaceMetricsEvent.InterfaceStates.Open).Send();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Metrics.Interface.InterfaceType(InterfaceMetricsEvent.InterfaceTypes.CharScreen).InterfaceState(InterfaceMetricsEvent.InterfaceStates.Close).Send();
	}

	private MenuEntityVM GetMenuEntity(CharInfoPageType type)
	{
		return new MenuEntityVM(UIStrings.Instance.CharacterSheet.GetMenuLabel(type), (int)type);
	}

	public void SetSelected(CharInfoPageType type)
	{
		MenuEntityVM value = MenuVM.EntitiesCollection.FirstOrDefault((MenuEntityVM e) => e.EnumId == (int)type);
		m_SelectedMenuEntity.Value = value;
	}

	private void HandleSelectedMenu(int selectedId)
	{
		switch ((CharInfoPageType)selectedId)
		{
		case CharInfoPageType.Abilities:
			HandleOpenAbilities();
			break;
		case CharInfoPageType.Archetype:
			HandleOpenArchetype();
			break;
		case CharInfoPageType.Convictions:
			HandleOpenConvictions();
			break;
		case CharInfoPageType.Characteristics:
			HandleOpenCharacteristics();
			break;
		case CharInfoPageType.Summary:
		case CharInfoPageType.Features:
		case CharInfoPageType.PsykerPowers:
		case CharInfoPageType.LevelProgression:
		case CharInfoPageType.Biography:
		case CharInfoPageType.FactionsReputation:
		case CharInfoPageType.Modifiers:
			Debug.Log("This page is unsupported yet");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		CheckModifiers();
	}

	private void HandleOpenAbilities()
	{
		if (!CurrentWindowIs(m_AbilitiesVM.Value))
		{
			m_CurrentWindow?.Dispose();
			m_AbilitiesVM.Value = new CharInfoAbilitiesTabVM(m_Unit, this);
			m_CurrentWindow = m_AbilitiesVM.Value;
		}
	}

	private void HandleOpenArchetype()
	{
		if (!CurrentWindowIs(m_ArchetypeVM.Value))
		{
			m_CurrentWindow?.Dispose();
			m_ArchetypeVM.Value = new CharInfoArchetypeTabVM(m_Unit);
			m_CurrentWindow = m_ArchetypeVM.Value;
		}
	}

	private void HandleOpenConvictions()
	{
		if (!CurrentWindowIs(m_ConvictionsVM.Value))
		{
			m_CurrentWindow?.Dispose();
			m_ConvictionsVM.Value = new CharInfoConvictionsTabVM(m_Unit);
			m_CurrentWindow = m_ConvictionsVM.Value;
		}
	}

	private void HandleOpenCharacteristics()
	{
		if (!CurrentWindowIs(m_CharacteristicsVM.Value))
		{
			m_CurrentWindow?.Dispose();
			m_CharacteristicsVM.Value = new CharInfoCharacteristicsTabVM(m_Unit);
			m_CurrentWindow = m_CharacteristicsVM.Value;
		}
	}

	private bool CurrentWindowIs(ViewModel other)
	{
		if (m_CurrentWindow != null)
		{
			return m_CurrentWindow == other;
		}
		return false;
	}

	public void CheckModifiers()
	{
		PartAbilityModifiers partAbilityModifiers = m_Unit.Value.GetOrCreate<PartAbilityModifiers>();
		List<BlueprintAbilityModifier> availableAndFreeModifiers = partAbilityModifiers.KnownModifiers.Where((BlueprintAbilityModifier m) => partAbilityModifiers.AddedModifiers.All((PartAbilityModifiers.AddedEntry a) => a.Modifier != m)).ToList();
		if (availableAndFreeModifiers.Count == 0)
		{
			m_HasModifierUpgrade.Value = false;
			return;
		}
		List<(Ability, List<BlueprintAbilityTag>)> list = (from a in m_Unit.Value.Abilities.Visible
			where !a.Blueprint.IsWeaponAbility
			select (Ability: a, Tags: a.Blueprint.Tags.Select((BpRef<BlueprintAbilityTag> tag) => tag.Blueprint.Reference().Blueprint).ToList()) into a
			where !partAbilityModifiers.AddedModifiers.Any((PartAbilityModifiers.AddedEntry applied) => applied.IsAddedManually && applied.Ability == a.Ability.Blueprint) && availableAndFreeModifiers.Any((BlueprintAbilityModifier modifier) => partAbilityModifiers.IsSuitableModifier(modifier, a.Ability))
			select a).ToList();
		List<BlueprintAbilityTag> list2 = new List<BlueprintAbilityTag>
		{
			ConfigRoot.Instance.AbilityRoot.WeaponSingleShotTag,
			ConfigRoot.Instance.AbilityRoot.WeaponBurstTag,
			ConfigRoot.Instance.AbilityRoot.WeaponAoETag
		}.Where((BlueprintAbilityTag t) => !partAbilityModifiers.AddedModifiers.Any((PartAbilityModifiers.AddedEntry applied) => applied.IsAddedManually && applied.AbilityTag == t) && availableAndFreeModifiers.Any((BlueprintAbilityModifier modifier) => partAbilityModifiers.IsSuitableModifier(modifier, ConfigRoot.Instance.AbilityRoot.AttackAbilityTag))).ToList();
		m_HasModifierUpgrade.Value = list.Count > 0 || list2.Count > 0;
	}
}
