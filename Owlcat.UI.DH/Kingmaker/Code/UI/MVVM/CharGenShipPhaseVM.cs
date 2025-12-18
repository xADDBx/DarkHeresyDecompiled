using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenShipPhaseVM : CharGenPhaseBaseVM
{
	private readonly ReactiveProperty<bool> m_CurrentPageIsFirst = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_CurrentPageIsLast = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<Action> m_InterruptHandler = new ReactiveCommand<Action>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ObservableList<CharGenShipItemVM> m_ShipEntitiesList = new ObservableList<CharGenShipItemVM>();

	private bool m_ShipNameWasEdited;

	private bool m_Subscribed;

	private readonly ReactiveProperty<CharGenChangeNameMessageBoxVM> m_MessageBoxVM = new ReactiveProperty<CharGenChangeNameMessageBoxVM>();

	private readonly ReactiveProperty<CharGenShipItemVM> m_SelectedShipEntity = new ReactiveProperty<CharGenShipItemVM>();

	private readonly ReactiveProperty<string> m_ShipName = new ReactiveProperty<string>();

	public ReadOnlyReactiveProperty<bool> CurrentPageIsFirst => m_CurrentPageIsFirst;

	public ReadOnlyReactiveProperty<bool> CurrentPageIsLast => m_CurrentPageIsLast;

	public Observable<Action> InterruptHandler => m_InterruptHandler;

	public ReadOnlyReactiveProperty<CharGenChangeNameMessageBoxVM> MessageBoxVM => m_MessageBoxVM;

	public ReadOnlyReactiveProperty<CharGenShipItemVM> SelectedShipEntity => m_SelectedShipEntity;

	public ReadOnlyReactiveProperty<string> ShipName => m_ShipName;

	public SelectionGroupRadioVM<CharGenShipItemVM> ShipSelectionGroup { get; }

	public CharGenShipPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Summary)
	{
		base.DollRoomType = CharGenDollRoomType.Ship;
		base.DollPosition = CharacterDollPosition.Ship;
		base.CanInterruptChargen = true;
		SetShowVisualSettings(show: false);
		base.HasPortrait = false;
		m_ShipName.Value = string.Empty;
		ShipSelectionGroup = new SelectionGroupRadioVM<CharGenShipItemVM>(m_ShipEntitiesList, m_SelectedShipEntity);
		AddDisposable(ShipSelectionGroup);
		AddDisposable(SelectedShipEntity.Subscribe(SetShip));
		CreateTooltipSystem();
		ConfigRoot.Instance.CharGenRoot.EnsureShipPregens(delegate(List<ChargenUnit> ships)
		{
			foreach (ChargenUnit item in ships.Where((ChargenUnit ship) => !ship.Blueprint.IsDlcRestricted()))
			{
				m_ShipEntitiesList.Add(AddDisposableAndReturn(new CharGenShipItemVM(item)));
			}
		});
		AddDisposable(IsCompletedAndAvailable.Subscribe(delegate(bool value)
		{
			m_OverrideConfirmHintLabel.Value = (value ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CharGen.EditName);
		}));
		AddDisposable(SelectedShipEntity.Subscribe(delegate(CharGenShipItemVM value)
		{
			m_CurrentPageIsFirst.Value = m_ShipEntitiesList.FirstOrDefault() == value;
			m_CurrentPageIsLast.Value = m_ShipEntitiesList.LastOrDefault() == value;
		}));
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_ShipEntitiesList.Clear();
	}

	protected override bool CheckIsCompleted()
	{
		return true;
	}

	protected override void OnBeginDetailedView()
	{
		if (!m_Subscribed)
		{
			AddDisposable(EventBus.Subscribe(this));
			AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
			m_Subscribed = true;
		}
		TrySelectItem();
		if (!m_ShipNameWasEdited)
		{
			SetName(GetDefaultName());
		}
	}

	protected virtual void Clear()
	{
		m_SelectedShipEntity.Value = null;
		m_ShipNameWasEdited = false;
	}

	protected virtual void HandleLevelUpManager(LevelUpManager manager)
	{
		Clear();
		if (manager != null)
		{
			UpdateIsCompleted();
		}
	}

	private void CreateTooltipSystem()
	{
		AddDisposable(InfoVM = new InfoSectionVM());
		AddDisposable(SecondaryInfoVM = new InfoSectionVM());
		AddDisposable(m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate));
	}

	private void SetShip(CharGenShipItemVM shipItemVM)
	{
	}

	private void TrySelectItem()
	{
		if (SelectedShipEntity.CurrentValue == null)
		{
			ShipSelectionGroup.TrySelectFirstValidEntity();
		}
	}

	private void SetupTooltipTemplate()
	{
		m_ReactiveTooltipTemplate.Value = TooltipTemplate();
	}

	private TooltipBaseTemplate TooltipTemplate()
	{
		return null;
	}

	public void SetName(string shipName)
	{
	}

	public override void InterruptChargen(Action onComplete)
	{
		Action parameter = (Game.Instance.IsControllerGamepad ? ((Action)delegate
		{
		}) : onComplete);
		m_InterruptHandler.Execute(parameter);
	}

	public string GetRandomName()
	{
		return BlueprintCharGenRoot.Instance.PregenCharacterNames.GetRandomShipName(ShipName.CurrentValue);
	}

	private string GetDefaultName()
	{
		return BlueprintCharGenRoot.Instance.PregenCharacterNames.GetDefaultShipName(string.Empty);
	}

	public void ShowChangeNameMessageBox(Action onComplete = null)
	{
		DisposeMessageBox();
		m_MessageBoxVM.Value = new CharGenChangeNameMessageBoxVM(UIStrings.Instance.CharGen.ChooseName, UIStrings.Instance.SettingsUI.DialogApply, delegate(string text)
		{
			text = text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				SetName(text);
			}
			onComplete?.Invoke();
		}, ShipName.CurrentValue, GetRandomName, DisposeMessageBox);
	}

	private void DisposeMessageBox()
	{
		DisposeAndRemove(MessageBoxVM);
	}

	public bool GoNextPage()
	{
		return ShipSelectionGroup.SelectNextValidEntity();
	}

	public bool GoPrevPage()
	{
		return ShipSelectionGroup.SelectPrevValidEntity();
	}
}
