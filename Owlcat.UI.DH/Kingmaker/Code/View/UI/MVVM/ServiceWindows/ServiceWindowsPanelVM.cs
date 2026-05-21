using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.Common;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.ServiceWindows;

public class ServiceWindowsPanelVM : ViewModel
{
	private readonly ReactiveProperty<MenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<MenuEntityVM>();

	private readonly ReactiveProperty<CharInfoNameAndPortraitVM> m_CharInfoAndPortraitVM = new ReactiveProperty<CharInfoNameAndPortraitVM>();

	private readonly ReactiveProperty<BaseUnitEntity> m_SelectedUnit;

	private readonly ReactiveProperty<InventoryVM> m_InventoryVM = new ReactiveProperty<InventoryVM>();

	private readonly ReactiveProperty<CharacterInfoVM> m_CharInfoVM = new ReactiveProperty<CharacterInfoVM>();

	private readonly ReactiveProperty<JournalVM> m_JournalVM = new ReactiveProperty<JournalVM>();

	private readonly ReactiveProperty<FactionReputationVM> m_ReputationVM = new ReactiveProperty<FactionReputationVM>();

	private readonly ReactiveProperty<DetectiveJournalVM> m_DetectiveJournalVM = new ReactiveProperty<DetectiveJournalVM>();

	private readonly ReactiveProperty<EncyclopediaVM> m_EncyclopediaVM = new ReactiveProperty<EncyclopediaVM>();

	private readonly ReactiveProperty<LocalMapVM> m_LocalMapVM = new ReactiveProperty<LocalMapVM>();

	private readonly ReactiveProperty<FullScreenUIType> m_CurrentUIType = new ReactiveProperty<FullScreenUIType>();

	private readonly ReactiveProperty<bool> m_IsFromLevelUp = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<ServiceWindowsType> m_LockedWindowType = new ReactiveProperty<ServiceWindowsType>(ServiceWindowsType.None);

	private readonly Action m_Close;

	private readonly UnitBuffBlockVM m_BuffBlockVM;

	private readonly BuffGroupsVM m_BuffGroupsVM;

	private readonly Dictionary<FullScreenUIType, ServiceWindowsType> m_UITypeToWindowType = new Dictionary<FullScreenUIType, ServiceWindowsType>
	{
		{
			FullScreenUIType.Inventory,
			ServiceWindowsType.Inventory
		},
		{
			FullScreenUIType.CharacterScreen,
			ServiceWindowsType.CharacterInfo
		},
		{
			FullScreenUIType.Journal,
			ServiceWindowsType.Journal
		},
		{
			FullScreenUIType.Reputation,
			ServiceWindowsType.Reputation
		},
		{
			FullScreenUIType.DetectiveJournal,
			ServiceWindowsType.DetectiveJournal
		},
		{
			FullScreenUIType.Encyclopedia,
			ServiceWindowsType.Encyclopedia
		},
		{
			FullScreenUIType.LocalMap,
			ServiceWindowsType.LocalMap
		}
	};

	private IDisposable m_CurrentWindow;

	private CharInfoPageType m_Type;

	private BaseUnitEntity m_Unit;

	public readonly MenuVM MenuVM;

	private ServiceWindowsType CurrentWindowType => (ServiceWindowsType)(m_SelectedMenuEntity.Value?.EnumId ?? 0);

	public bool HasPrevWindow { get; private set; }

	public ReadOnlyReactiveProperty<InventoryVM> InventoryVM => m_InventoryVM;

	public ReadOnlyReactiveProperty<CharacterInfoVM> CharInfoVM => m_CharInfoVM;

	public ReadOnlyReactiveProperty<JournalVM> JournalVM => m_JournalVM;

	public ReadOnlyReactiveProperty<FactionReputationVM> ReputationVM => m_ReputationVM;

	public ReadOnlyReactiveProperty<DetectiveJournalVM> DetectiveJournalVM => m_DetectiveJournalVM;

	public ReadOnlyReactiveProperty<EncyclopediaVM> EncyclopediaVM => m_EncyclopediaVM;

	public ReadOnlyReactiveProperty<LocalMapVM> LocalMapVM => m_LocalMapVM;

	public ReadOnlyReactiveProperty<FullScreenUIType> CurrentUIType => m_CurrentUIType;

	public ReadOnlyReactiveProperty<CharInfoNameAndPortraitVM> CharInfoAndPortraitVM => m_CharInfoAndPortraitVM;

	public ReadOnlyReactiveProperty<ServiceWindowsType> LockedWindowType => m_LockedWindowType;

	public Texture FoWTextureTemp { get; private set; }

	public ServiceWindowsPanelVM(ServiceWindowsType windowType, Action close, CharInfoPageType type = CharInfoPageType.Convictions, BaseUnitEntity unit = null)
	{
		ServiceWindowsPanelVM serviceWindowsPanelVM = this;
		m_Close = close;
		m_Type = type;
		m_Unit = unit;
		m_IsFromLevelUp.Value = RootUIContext.Instance.IsChargenShown;
		if (unit != null)
		{
			Game.Instance.Controllers.SelectionCharacter.SetSelected(unit, force: false, forceFullScreenState: true);
		}
		m_SelectedUnit = Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI;
		m_BuffBlockVM = new UnitBuffBlockVM(m_SelectedUnit.CurrentValue).AddTo(this);
		m_SelectedUnit.Subscribe(m_BuffBlockVM.SetUnitData).AddTo(this);
		m_BuffGroupsVM = new BuffGroupsVM(m_BuffBlockVM.Buffs).AddTo(this);
		FoWTextureTemp = Shader.GetGlobalTexture(FogOfWarConstantBuffer._FogOfWarMask);
		List<MenuEntityVM> list = new List<MenuEntityVM>
		{
			GetMenuEntity(ServiceWindowsType.Inventory),
			GetMenuEntity(ServiceWindowsType.CharacterInfo),
			GetMenuEntity(ServiceWindowsType.Journal),
			GetMenuEntity(ServiceWindowsType.Reputation),
			GetMenuEntity(ServiceWindowsType.DetectiveJournal),
			GetMenuEntity(ServiceWindowsType.Encyclopedia),
			GetMenuEntity(ServiceWindowsType.LocalMap)
		};
		m_SelectedMenuEntity.Value = list.FirstOrDefault((MenuEntityVM e) => e.EnumId == (int)windowType);
		MenuVM = new MenuVM(list, m_SelectedMenuEntity).AddTo(this);
		m_SelectedMenuEntity.Subscribe(delegate(MenuEntityVM e)
		{
			serviceWindowsPanelVM.HandleSelectedMenu(e.EnumId);
		}).AddTo(this);
		CurrentUIType.Subscribe(HandleScreenTypeChanged).AddTo(this);
		LockedWindowType.Subscribe(LockOnWindow).AddTo(this);
		m_IsFromLevelUp.Subscribe(HandleIsFromLevelUp).AddTo(this);
	}

	private MenuEntityVM GetMenuEntity(ServiceWindowsType type)
	{
		return new MenuEntityVM(UIStrings.Instance.ServiceWindows.GetTitle(type), (int)type);
	}

	public void HandleOpenAt(ServiceWindowsType uiType)
	{
		MenuEntityVM value = MenuVM.EntitiesCollection.FirstOrDefault((MenuEntityVM e) => e.EnumId == (int)uiType);
		m_SelectedMenuEntity.Value = value;
	}

	private void HandleSelectedMenu(int selectedId)
	{
		switch ((ServiceWindowsType)selectedId)
		{
		case ServiceWindowsType.Inventory:
			HandleInventory();
			break;
		case ServiceWindowsType.CharacterInfo:
			HandleCharInfo(m_Type, m_Unit);
			break;
		case ServiceWindowsType.Journal:
			HandleJournal();
			break;
		case ServiceWindowsType.Reputation:
			HandleReputation();
			break;
		case ServiceWindowsType.LocalMap:
			HandleOpenLocalMap();
			break;
		case ServiceWindowsType.Encyclopedia:
			HandleOpenEncyclopedia();
			break;
		case ServiceWindowsType.DetectiveJournal:
			HandleDetectiveJournal(null);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case ServiceWindowsType.None:
			break;
		}
	}

	private void HandleInventory()
	{
		if (!CurrentWindowIs(InventoryVM.CurrentValue))
		{
			PlayOpenSound(ServiceWindowsType.Inventory);
			DisposeCurrentWindow();
			m_InventoryVM.Value = new InventoryVM().AddTo(this);
			m_CurrentWindow = InventoryVM.CurrentValue;
			m_CurrentUIType.Value = FullScreenUIType.Inventory;
		}
	}

	public void HandleCharInfo(CharInfoPageType type = CharInfoPageType.Convictions, BaseUnitEntity unit = null)
	{
		m_Type = type;
		m_Unit = unit;
		if (CurrentWindowIs(CharInfoVM.CurrentValue) && m_SelectedUnit.Value == unit)
		{
			CharInfoVM.CurrentValue.SetSelected(type);
			return;
		}
		PlayOpenSound(ServiceWindowsType.CharacterInfo);
		DisposeCurrentWindow();
		if (unit != null)
		{
			m_SelectedUnit.Value = unit;
		}
		m_CharInfoVM.Value = new CharacterInfoVM(type, m_SelectedUnit, m_BuffGroupsVM).AddTo(this);
		m_CurrentWindow = CharInfoVM.CurrentValue;
		m_CurrentUIType.Value = FullScreenUIType.CharacterScreen;
		m_IsFromLevelUp.Value = RootUIContext.Instance.IsChargenShown;
	}

	public void HandleJournal()
	{
		if (!CurrentWindowIs(JournalVM.CurrentValue))
		{
			PlayOpenSound(ServiceWindowsType.Journal);
			DisposeCurrentWindow();
			m_JournalVM.Value = new JournalVM().AddTo(this);
			m_CurrentWindow = JournalVM.CurrentValue;
			m_CurrentUIType.Value = FullScreenUIType.Journal;
		}
	}

	private void HandleReputation()
	{
		if (!CurrentWindowIs(ReputationVM.CurrentValue))
		{
			PlayOpenSound(ServiceWindowsType.Reputation);
			DisposeCurrentWindow();
			m_ReputationVM.Value = new FactionReputationVM().AddTo(this);
			m_CurrentWindow = ReputationVM.CurrentValue;
			m_CurrentUIType.Value = FullScreenUIType.Reputation;
		}
	}

	public void HandleDetectiveJournal(BlueprintCase caseToOpen, BlueprintClue focusClue = null, bool requireReport = false)
	{
		bool flag = requireReport && caseToOpen != null && !caseToOpen.IsClosed();
		if (flag)
		{
			m_LockedWindowType.Value = ServiceWindowsType.DetectiveJournal;
		}
		if (CurrentWindowIs(DetectiveJournalVM.CurrentValue))
		{
			DetectiveJournalVM.CurrentValue.SetOpenedCase(caseToOpen, focusClue, !flag);
			return;
		}
		PlayOpenSound(ServiceWindowsType.DetectiveJournal);
		DisposeCurrentWindow();
		m_DetectiveJournalVM.Value = new DetectiveJournalVM().AddTo(this);
		m_CurrentWindow = DetectiveJournalVM.CurrentValue;
		m_CurrentUIType.Value = FullScreenUIType.DetectiveJournal;
		if (caseToOpen != null || focusClue != null)
		{
			m_DetectiveJournalVM.Value.SetOpenedCase(caseToOpen, focusClue, !flag);
		}
	}

	public void HandleOpenEncyclopedia(INode page = null)
	{
		if (CurrentWindowIs(EncyclopediaVM.CurrentValue))
		{
			EncyclopediaVM.CurrentValue.HandleEncyclopediaPage(page);
			return;
		}
		PlayOpenSound(ServiceWindowsType.Encyclopedia);
		DisposeCurrentWindow();
		m_EncyclopediaVM.Value = new EncyclopediaVM(page).AddTo(this);
		m_CurrentWindow = EncyclopediaVM.CurrentValue;
		m_CurrentUIType.Value = FullScreenUIType.Encyclopedia;
	}

	private void HandleOpenLocalMap()
	{
		if (!CurrentWindowIs(LocalMapVM.CurrentValue))
		{
			PlayOpenSound(ServiceWindowsType.LocalMap);
			DisposeCurrentWindow();
			m_LocalMapVM.Value = new LocalMapVM().AddTo(this);
			m_CurrentWindow = LocalMapVM.CurrentValue;
			m_CurrentUIType.Value = FullScreenUIType.LocalMap;
		}
	}

	private void HandleScreenTypeChanged(FullScreenUIType screenType)
	{
		if (m_UITypeToWindowType.TryGetValue(screenType, out var windowType))
		{
			m_SelectedMenuEntity.Value = MenuVM.EntitiesCollection.FirstOrDefault((MenuEntityVM e) => e.EnumId == (int)windowType);
		}
		if (screenType == FullScreenUIType.Inventory)
		{
			ReactiveProperty<CharInfoNameAndPortraitVM> charInfoAndPortraitVM = m_CharInfoAndPortraitVM;
			if (charInfoAndPortraitVM.Value == null)
			{
				CharInfoNameAndPortraitVM charInfoNameAndPortraitVM2 = (charInfoAndPortraitVM.Value = new CharInfoNameAndPortraitVM(m_BuffBlockVM, m_BuffGroupsVM, m_SelectedUnit));
			}
		}
		else
		{
			m_CharInfoAndPortraitVM.Value?.Dispose();
			m_CharInfoAndPortraitVM.Value = null;
		}
	}

	private void HandleIsFromLevelUp(bool isFromLevelUp)
	{
		foreach (MenuEntityVM item in MenuVM.EntitiesCollection)
		{
			item.SetAvailable(!isFromLevelUp || item.EnumId == 2 || item.EnumId == 1);
		}
	}

	private void DisposeCurrentWindow()
	{
		if (m_CurrentWindow != null)
		{
			Metrics.Interface.FullScreenType(m_CurrentUIType.Value).State(InterfaceMetricsEvent.InterfaceStates.Close).Send();
			HasPrevWindow = true;
			if (m_CurrentWindow is IServiceWindow serviceWindow)
			{
				serviceWindow.HandleOnSwitchedFromWindow();
			}
			m_CurrentWindow.Dispose();
			m_InventoryVM.Value = null;
			m_CharInfoVM.Value = null;
			m_JournalVM.Value = null;
			m_ReputationVM.Value = null;
			m_DetectiveJournalVM.Value = null;
			m_EncyclopediaVM.Value = null;
			m_LocalMapVM.Value = null;
		}
	}

	private bool CurrentWindowIs(ViewModel other)
	{
		if (m_CurrentWindow != null)
		{
			return object.Equals(m_CurrentWindow, other);
		}
		return false;
	}

	public void Close()
	{
		PlayCloseSound(CurrentWindowType);
		m_CurrentUIType.Value = FullScreenUIType.Unknown;
		m_LockedWindowType.Value = ServiceWindowsType.None;
		m_Close?.Invoke();
		HasPrevWindow = false;
	}

	private void LockOnWindow(ServiceWindowsType windowType)
	{
		MenuVM.DisableAllExcept((int)windowType);
	}

	private void PlayOpenSound(ServiceWindowsType windowType)
	{
		if (m_CurrentWindow != null)
		{
			UISounds.Instance.Sounds.ServiceWindowsSounds.PlaySwitchSound(windowType);
		}
		else
		{
			UISounds.Instance.Sounds.ServiceWindowsSounds.PlayOpenSound(windowType);
		}
	}

	private void PlayCloseSound(ServiceWindowsType windowType)
	{
		UISounds.Instance.Sounds.ServiceWindowsSounds.PlayCloseSound(windowType);
	}
}
