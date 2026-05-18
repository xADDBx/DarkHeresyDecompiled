using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.DLC;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Portrait;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenPortraitsSelectorVM : BaseCharGenAppearancePageComponentVM, ICharGenPortraitHandler, ISubscriber
{
	private const int FirstScreenAmount = 40;

	private readonly ReactiveProperty<CharGenPortraitTabVM> m_CurrentTab = new ReactiveProperty<CharGenPortraitTabVM>();

	private readonly ReactiveProperty<CharGenCustomPortraitCreatorVM> m_CustomPortraitCreatorVM = new ReactiveProperty<CharGenCustomPortraitCreatorVM>();

	private readonly ObservableList<CharGenPortraitSelectorItemVM> m_AllPortraitsCollection = new ObservableList<CharGenPortraitSelectorItemVM>();

	public readonly SelectionGroupRadioVM<CharGenPortraitTabVM> TabSelector;

	private readonly ReactiveProperty<CharGenPortraitSelectorItemVM> m_SelectedPortrait = new ReactiveProperty<CharGenPortraitSelectorItemVM>();

	private readonly SelectionGroupRadioVM<CharGenPortraitSelectorItemVM> m_SelectorGroupVM;

	private readonly CharGenContext m_CharGenContext;

	private SelectionStatePortrait m_SelectionStatePortrait;

	public readonly ReadOnlyReactiveProperty<PortraitVM> PortraitVM;

	public readonly Dictionary<PortraitCategory, CharGenPortraitGroupVM> PortraitGroupVms = new Dictionary<PortraitCategory, CharGenPortraitGroupVM>();

	public readonly CharGenPortraitGroupVM CustomPortraitGroup = new CharGenPortraitGroupVM();

	public ReadOnlyReactiveProperty<CharGenPortraitTabVM> CurrentTab => m_CurrentTab;

	public ReadOnlyReactiveProperty<CharGenCustomPortraitCreatorVM> CustomPortraitCreatorVM => m_CustomPortraitCreatorVM;

	public ReadOnlyReactiveProperty<CharGenPortraitSelectorItemVM> SelectedPortrait => m_SelectedPortrait;

	public CharGenPortraitsSelectorVM(CharGenContext ctx, SelectionStatePortrait selectionStatePortrait)
	{
		m_CharGenContext = ctx;
		m_SelectionStatePortrait = selectionStatePortrait;
		AddDisposable(EventBus.Subscribe(this));
		List<CharGenPortraitTabVM> visibleCollection = (from CharGenPortraitTab tab in Enum.GetValues(typeof(CharGenPortraitTab))
			select AddDisposableAndReturn(new CharGenPortraitTabVM(tab))).ToList();
		AddDisposable(TabSelector = new SelectionGroupRadioVM<CharGenPortraitTabVM>(visibleCollection, m_CurrentTab, cyclical: true));
		TabSelector.TrySelectFirstValidEntity();
		AddDisposable(CurrentTab.Subscribe(delegate(CharGenPortraitTabVM tab)
		{
			if (tab != null && tab.Tab != CharGenPortraitTab.Custom)
			{
				OnCustomPortraitCreatorClose();
			}
		}));
		AddDisposable(CustomPortraitGroup);
		CollectCharacterPortraits();
		CollectCustomPortraits();
		AddDisposable(m_SelectorGroupVM = new SelectionGroupRadioVM<CharGenPortraitSelectorItemVM>(m_AllPortraitsCollection, m_SelectedPortrait));
		AddDisposable(m_CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
		PortraitVM = m_SelectedPortrait.Select(delegate(CharGenPortraitSelectorItemVM itemVM)
		{
			PortraitVM portraitVM2 = new PortraitVM(itemVM?.PortraitData ?? m_CharGenContext.Doll.Portrait?.Data ?? ConfigRoot.Instance.CharGenRoot.CustomPortrait.Data);
			AddDisposable(portraitVM2);
			return portraitVM2;
		}).ToReadOnlyReactiveProperty();
		AddDisposable(m_SelectedPortrait.Where((CharGenPortraitSelectorItemVM x) => x != null).Subscribe(delegate(CharGenPortraitSelectorItemVM portraitVM)
		{
			Game.Instance.GameCommandQueue.CharGenSetPortrait(portraitVM.GetBlueprintPortrait());
		}));
		TrySelectPortrait();
	}

	public void SetCurrentTab(CharGenPortraitTab tab)
	{
		CharGenPortraitTabVM value = TabSelector.EntitiesCollection.FirstOrDefault((CharGenPortraitTabVM elem) => elem.Tab == tab);
		m_CurrentTab.Value = value;
	}

	void ICharGenPortraitHandler.HandleSetPortrait(BlueprintPortrait portrait)
	{
		m_SelectionStatePortrait?.SelectPortrait(portrait);
		m_CharGenContext.Doll.SetPortrait(portrait);
		if (!UtilityNet.IsControlMainCharacter())
		{
			CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = m_AllPortraitsCollection.FirstOrDefault(portrait.Data.IsCustom ? ((Func<CharGenPortraitSelectorItemVM, bool>)((CharGenPortraitSelectorItemVM p) => portrait.Data.CustomId.Equals(p.PortraitData?.CustomId, StringComparison.Ordinal))) : ((Func<CharGenPortraitSelectorItemVM, bool>)((CharGenPortraitSelectorItemVM p) => portrait.Data == p.PortraitData)));
			if (charGenPortraitSelectorItemVM != null)
			{
				m_SelectorGroupVM.TrySelectEntity(charGenPortraitSelectorItemVM);
			}
			else
			{
				OnCustomPortraitCreate(portrait);
			}
		}
		Changed();
	}

	protected override void DisposeImplementation()
	{
	}

	public override void OnBeginView()
	{
		TrySelectPortrait();
	}

	protected void CollectCharacterPortraits()
	{
		foreach (BlueprintPortrait portrait in ConfigRoot.Instance.CharGenRoot.Portraits)
		{
			if (IsPortraitAllowed(portrait))
			{
				CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = AddDisposableAndReturn(new CharGenPortraitSelectorItemVM(portrait));
				m_AllPortraitsCollection.Add(charGenPortraitSelectorItemVM);
				PortraitCategory portraitCategory = portrait.Data.PortraitCategory;
				if (!PortraitGroupVms.ContainsKey(portraitCategory))
				{
					PortraitGroupVms.Add(portraitCategory, AddDisposableAndReturn(new CharGenPortraitGroupVM(portraitCategory)));
					PortraitGroupVms[portraitCategory].SetExpanded(isExpanded: true);
				}
				PortraitGroupVms[portraitCategory].Add(charGenPortraitSelectorItemVM);
			}
		}
	}

	private bool IsPortraitAllowed(BlueprintPortrait portrait)
	{
		if (portrait.IsDlcRestricted())
		{
			return false;
		}
		return true;
	}

	private void CollectCustomPortraits()
	{
		using (ProfileScope.New("Collect custom portraits"))
		{
			CustomPortraitGroup.Add(AddDisposableAndReturn(new CharGenPortraitSelectorItemVM(delegate
			{
				OnCustomPortraitCreate();
			})));
			foreach (PortraitData item in CustomPortraitsManager.Instance.LoadAllPortraits(40))
			{
				CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = AddDisposableAndReturn(new CharGenPortraitSelectorItemVM(item, OnCustomPortraitChange));
				m_AllPortraitsCollection.Add(charGenPortraitSelectorItemVM);
				CustomPortraitGroup.Add(charGenPortraitSelectorItemVM);
			}
		}
	}

	private bool TryUpdatePortraitFromState()
	{
		if (m_CharGenContext.Doll.Portrait != null)
		{
			CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = m_AllPortraitsCollection.FirstOrDefault((CharGenPortraitSelectorItemVM p) => p.PortraitData == m_CharGenContext.Doll.Portrait.Data);
			if (charGenPortraitSelectorItemVM != null)
			{
				m_SelectedPortrait.Value = charGenPortraitSelectorItemVM;
				m_SelectedPortrait.ForceNotify();
				return true;
			}
		}
		return false;
	}

	private void TrySelectPortrait()
	{
		TryUpdatePortraitFromState();
	}

	private void OnCustomPortraitCreate(BlueprintPortrait blueprintPortrait = null)
	{
		CustomPortraitGroup.RemoveNonexistentItems();
		CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = new CharGenPortraitSelectorItemVM(blueprintPortrait?.Data ?? CustomPortraitsManager.Instance.CreateNew(), OnCustomPortraitChange);
		m_AllPortraitsCollection.Add(charGenPortraitSelectorItemVM);
		CustomPortraitGroup.Add(charGenPortraitSelectorItemVM);
		m_SelectedPortrait.Value = charGenPortraitSelectorItemVM;
		OpenCustomPortraitCreator();
	}

	private void OnCustomPortraitChange(CharGenPortraitSelectorItemVM changeItem)
	{
		m_SelectedPortrait.Value = changeItem;
		OpenCustomPortraitCreator();
	}

	public void SelectPortraitAndOpenCreator(CharGenPortraitSelectorItemVM changeItem)
	{
		OnCustomPortraitChange(changeItem);
	}

	public void OpenCustomPortraitCreator()
	{
		if (CustomPortraitCreatorVM.CurrentValue == null && PortraitVM.CurrentValue.PortraitData.IsCustom && CustomPortraitsManager.Instance.EnsureCustomPortraits(PortraitVM.CurrentValue.PortraitData.CustomId) && UtilityNet.IsControlMainCharacter())
		{
			m_CustomPortraitCreatorVM.Value = new CharGenCustomPortraitCreatorVM(PortraitVM, OnOpenFolderClick, OnRefreshPortraitsClick, OnCustomPortraitCreatorClose);
		}
	}

	public void SelectNextPortraitsTab()
	{
		TabSelector.SelectNextValidEntity();
	}

	private void OnCustomPortraitCreatorClose()
	{
		CustomPortraitCreatorVM.CurrentValue?.Dispose();
		m_CustomPortraitCreatorVM.Value = null;
	}

	private void OnOpenFolderClick()
	{
		CustomPortraitsManager.Instance.OpenPortraitFolder(m_SelectedPortrait.Value.PortraitData.CustomId);
	}

	private void OnRefreshPortraitsClick()
	{
		string customId = m_SelectedPortrait.Value.PortraitData.CustomId;
		m_AllPortraitsCollection.RemoveAll((CharGenPortraitSelectorItemVM item) => item.PortraitData?.IsCustom ?? true);
		CustomPortraitGroup.RemoveCustomItems();
		CustomPortraitsManager.Instance.UpdateGuid(customId);
		CustomPortraitsManager.Instance.Cleanup();
		foreach (PortraitData item in CustomPortraitsManager.Instance.LoadAllPortraits(40))
		{
			CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = new CharGenPortraitSelectorItemVM(item, OnCustomPortraitChange);
			m_AllPortraitsCollection.Add(charGenPortraitSelectorItemVM);
			CustomPortraitGroup.Add(charGenPortraitSelectorItemVM);
		}
		m_SelectedPortrait.Value = CustomPortraitGroup.GetByIdOrFirstValid(customId);
		m_SelectedPortrait.ForceNotify();
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager != null)
		{
			BlueprintPortraitSelection selectionByType = UtilityChargen.GetSelectionByType<BlueprintPortraitSelection>(manager.Path);
			if (selectionByType != null)
			{
				m_SelectionStatePortrait = manager.GetSelectionState(manager.Path, selectionByType, 0) as SelectionStatePortrait;
				m_SelectedPortrait.Value = null;
				TrySelectPortrait();
			}
		}
	}
}
