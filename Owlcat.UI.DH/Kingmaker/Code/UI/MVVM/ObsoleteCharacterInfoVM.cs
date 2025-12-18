using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ObsoleteCharacterInfoVM : ViewModel
{
	public readonly Dictionary<CharInfoComponentType, ReactiveProperty<CharInfoComponentVM>> ComponentVMs = new Dictionary<CharInfoComponentType, ReactiveProperty<CharInfoComponentVM>>();

	public readonly SelectionGroupRadioVM<CharInfoPagesMenuEntityVM> PagesSelectionGroupRadioVM;

	private readonly ReactiveCommand<Unit> m_BiographyUpdated = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<CharInfoPageType> m_PageType = new ReactiveProperty<CharInfoPageType>();

	private readonly ReactiveProperty<BaseUnitEntity> m_Unit;

	private readonly ReactiveProperty<BaseUnitEntity> m_PreviewUnit = new ReactiveProperty<BaseUnitEntity>();

	private List<CharInfoComponentType> m_CreatedVMs = new List<CharInfoComponentType>();

	private readonly CharInfoPagesPC m_CharInfoPages;

	private readonly ObservableList<CharInfoPagesMenuEntityVM> m_Pages = new ObservableList<CharInfoPagesMenuEntityVM>();

	private readonly ReactiveProperty<CharInfoPagesMenuEntityVM> m_CurrentPage = new ReactiveProperty<CharInfoPagesMenuEntityVM>();

	private readonly ReactiveProperty<LevelUpManager> m_LevelUpManager = new ReactiveProperty<LevelUpManager>();

	public Observable<Unit> BiographyUpdated => m_BiographyUpdated;

	public ReadOnlyReactiveProperty<CharInfoPageType> PageType => m_PageType;

	public bool CanCloseWindow
	{
		get
		{
			if (m_LevelUpManager.Value != null)
			{
				return m_LevelUpManager.Value.IsCommitted;
			}
			return true;
		}
	}

	public bool PageCanHaveNoEntities
	{
		get
		{
			CharInfoPagesMenuEntityVM value = m_CurrentPage.Value;
			if (value == null)
			{
				return false;
			}
			return value.PageType == CharInfoPageType.Biography;
		}
	}

	public ObsoleteCharacterInfoVM(CharInfoPageType selectedPageType = CharInfoPageType.Summary)
	{
		m_CharInfoPages = new CharInfoPagesPC();
		m_CurrentPage.Subscribe(delegate(CharInfoPagesMenuEntityVM value)
		{
			if (value != null)
			{
				m_PageType.Value = value.PageType;
			}
		}).AddTo(this);
		ReactiveProperty<BaseUnitEntity> selectedUnitInUI = Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI;
		if (selectedUnitInUI.Value == null)
		{
			BaseUnitEntity baseUnitEntity = (selectedUnitInUI.Value = Game.Instance.Player.MainCharacterEntity);
		}
		m_Unit = Game.Instance.Controllers.SelectionCharacter.SelectedUnitInUI;
		m_PreviewUnit.Value = m_Unit.Value;
		foreach (CharInfoComponentType value in Enum.GetValues(typeof(CharInfoComponentType)))
		{
			ComponentVMs[value] = new ReactiveProperty<CharInfoComponentVM>();
		}
		EventBus.Subscribe(this).AddTo(this);
		PagesSelectionGroupRadioVM = new SelectionGroupRadioVM<CharInfoPagesMenuEntityVM>(m_Pages).AddTo(this);
		PagesSelectionGroupRadioVM.SelectedEntity.Subscribe(delegate(CharInfoPagesMenuEntityVM value)
		{
			if (value != m_CurrentPage.Value)
			{
				if (ComponentVMs[CharInfoComponentType.Progression].Value is UnitProgressionVM unitProgressionVM)
				{
					unitProgressionVM.TryClose(delegate
					{
						m_CurrentPage.Value = value;
					}, delegate
					{
						PagesSelectionGroupRadioVM.TrySelectEntity(m_CurrentPage.Value);
					});
				}
				else
				{
					m_CurrentPage.Value = value;
				}
			}
		}).AddTo(this);
		foreach (CharInfoPageType item in m_CharInfoPages.PagesOrder)
		{
			CharInfoPagesMenuEntityVM charInfoPagesMenuEntityVM = new CharInfoPagesMenuEntityVM(item, m_Unit).AddTo(this);
			m_Pages.Add(charInfoPagesMenuEntityVM);
			if (item == selectedPageType)
			{
				m_CurrentPage.Value = charInfoPagesMenuEntityVM;
				PagesSelectionGroupRadioVM.TrySelectEntity(charInfoPagesMenuEntityVM);
			}
		}
		m_CurrentPage.Subscribe(delegate
		{
			UpdateData();
		}).AddTo(this);
		m_Unit.Subscribe(delegate
		{
			if (m_CurrentPage.Value.PageType == CharInfoPageType.Biography)
			{
				UpdateData();
				m_BiographyUpdated.Execute();
			}
		}).AddTo(this);
	}

	protected override void OnDispose()
	{
		foreach (CharInfoComponentType value in Enum.GetValues(typeof(CharInfoComponentType)))
		{
			ComponentVMs[value].Value?.Dispose();
			ComponentVMs[value].Value = null;
		}
		m_LevelUpManager.Value = null;
	}

	public void SetCurrentPage(CharInfoPageType pageType)
	{
		CharInfoPagesMenuEntityVM value = m_CurrentPage.Value;
		if (value == null || value.PageType != pageType)
		{
			m_Pages.FirstOrDefault((CharInfoPagesMenuEntityVM p) => p.PageType == pageType)?.SetSelected(state: true);
		}
	}

	private void UpdateData()
	{
		if (m_CurrentPage.Value == null || !m_CurrentPage.Value.IsAvailable.CurrentValue)
		{
			PagesSelectionGroupRadioVM.TrySelectFirstValidEntity();
		}
		CreateVMs(m_CharInfoPages.GetComponentsList(m_CurrentPage.Value.PageType, GetUnitType(m_Unit.Value)));
		UIEventType eventType = m_CurrentPage.Value.PageType switch
		{
			CharInfoPageType.Summary => UIEventType.CharacterInfoSummaryOpen, 
			CharInfoPageType.Features => UIEventType.CharacterInfoFeaturesOpen, 
			CharInfoPageType.PsykerPowers => UIEventType.CharacterInfoPsykerPowersOpen, 
			CharInfoPageType.LevelProgression => UIEventType.CharacterInfoLevelProgressionOpen, 
			CharInfoPageType.Biography => UIEventType.CharacterInfoBiographyOpen, 
			CharInfoPageType.FactionsReputation => UIEventType.CharacterInfoFactionsReputationOpen, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		EventBus.RaiseEvent(delegate(IUIEventHandler h)
		{
			h.HandleUIEvent(eventType);
		});
	}

	private void CreateVMs(List<CharInfoComponentType> types)
	{
		if (types == null || m_CreatedVMs.SequenceEqual(types))
		{
			return;
		}
		foreach (CharInfoComponentType item in m_CreatedVMs.Except(types))
		{
			ComponentVMs[item].Value?.Dispose();
			ComponentVMs[item].Value = null;
		}
		foreach (CharInfoComponentType item2 in types.Except(m_CreatedVMs))
		{
			ComponentVMs[item2].Value = CreateVM(item2);
		}
		m_CreatedVMs = types;
	}

	private CharInfoComponentVM CreateVM(CharInfoComponentType type)
	{
		return type switch
		{
			CharInfoComponentType.NameAndPortrait => new CharInfoNameAndPortraitVM(m_Unit), 
			CharInfoComponentType.LevelClassScores => new CharInfoLevelClassScoresVM(m_Unit, m_LevelUpManager), 
			CharInfoComponentType.Skills => new CharInfoSkillsBlockVM(m_Unit, m_LevelUpManager), 
			CharInfoComponentType.Abilities => new CharInfoFeaturesVM(m_Unit), 
			CharInfoComponentType.AlignmentWheel => new CharInfoAlignmentVM(m_Unit), 
			CharInfoComponentType.AlignmentHistory => new CharInfoAlignmentHistoryVM(m_Unit), 
			CharInfoComponentType.Stories => new CharInfoStoriesVM(m_Unit), 
			CharInfoComponentType.NameFullPortrait => new CharInfoNameAndPortraitVM(m_Unit), 
			CharInfoComponentType.BiographyStories => new CharInfoStoriesVM(m_Unit), 
			CharInfoComponentType.Progression => new UnitProgressionVM(m_Unit, m_LevelUpManager, UnitProgressionMode.LevelUp), 
			CharInfoComponentType.Weapons => new CharInfoWeaponsBlockVM(m_Unit), 
			CharInfoComponentType.SkillsAndWeapons => new CharInfoSkillsAndWeaponsVM(m_Unit, m_LevelUpManager), 
			CharInfoComponentType.FactionsReputation => new CharInfoFactionsReputationVM(m_Unit), 
			CharInfoComponentType.Summary => new CharInfoSummaryVM(m_Unit), 
			_ => null, 
		};
	}

	private UnitType GetUnitType(BaseUnitEntity unit)
	{
		if (unit.IsMainCharacter)
		{
			return UnitType.MainCharacter;
		}
		if (Game.Instance.Player.AllCharacters.Contains(unit))
		{
			return UnitType.Companion;
		}
		if (!unit.IsPet)
		{
			return UnitType.Unknown;
		}
		return UnitType.Pet;
	}

	public void ClearProgressionIfNeeded(BaseUnitEntity newUnitEntity)
	{
		(ComponentVMs[CharInfoComponentType.Progression].Value as UnitProgressionVM)?.ClearLevelupManagerIfNeeded(newUnitEntity);
	}
}
