using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class PartySelectorConsoleView : View<PartyVM>, ISwitchPartyCharactersHandler, ISubscriber
{
	[SerializeField]
	private PartySelectorItemConsoleView m_ItemPrefab;

	[SerializeField]
	private GridLayoutGroup m_Content;

	[Space]
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private ConsoleHint m_LinkAllHint;

	[SerializeField]
	private ConsoleHint m_LinkHint;

	[SerializeField]
	private ConsoleHint m_LevelUpHint;

	[SerializeField]
	private ConsoleHint m_MoveToNextHint;

	[SerializeField]
	private ConsoleHint m_MoveToPreviousHint;

	private List<PartySelectorItemConsoleView> m_CreatedItems = new List<PartySelectorItemConsoleView>();

	private List<PartySelectorItemConsoleView> m_Characters = new List<PartySelectorItemConsoleView>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private ReadOnlyReactiveProperty<PartySelectorItemConsoleView> m_SelectedEntity;

	private readonly ReactiveProperty<bool> m_IsLevelUp = new ReactiveProperty<bool>();

	private bool IsInHub => Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode ?? false;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
		InitializeItems();
	}

	private void InitializeItems()
	{
		m_CreatedItems = m_Content.GetComponentsInChildren<PartySelectorItemConsoleView>(includeInactive: true).ToList();
		foreach (PartySelectorItemConsoleView createdItem in m_CreatedItems)
		{
			createdItem.Initialize();
		}
	}

	protected override void OnBind()
	{
		EventBus.Subscribe(this).AddTo(this);
		base.ViewModel.UpdateConsoleGroup();
		for (int i = 0; i < base.ViewModel.CharactersVM.Count - m_CreatedItems.Count; i++)
		{
			PartySelectorItemConsoleView partySelectorItemConsoleView = Object.Instantiate(m_ItemPrefab, m_Content.transform, worldPositionStays: false);
			partySelectorItemConsoleView.Initialize();
			m_CreatedItems.Add(partySelectorItemConsoleView);
		}
		m_Characters.Clear();
		m_Characters = m_CreatedItems.GetRange(0, base.ViewModel.CharactersVM.Count);
		for (int j = 0; j < m_Characters.Count; j++)
		{
			if (base.ViewModel.CharactersVM[j].UnitEntityData != null)
			{
				m_Characters[j].Bind(base.ViewModel.CharactersVM[j]);
			}
		}
		m_CanvasSortingComponent.PushView().AddTo(this);
		m_FadeAnimator.AppearAnimation();
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		CreateNavigation();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: true, ModalWindowUIType.PartySelector);
		});
		EventBus.RaiseEvent(delegate(ICullFocusHandler h)
		{
			h.HandleRemoveFocus();
		});
	}

	protected override void OnUnbind()
	{
		PartySelectorItemConsoleView partySelectorItemConsoleView = m_NavigationBehaviour?.CurrentEntity as PartySelectorItemConsoleView;
		m_FadeAnimator.DisappearAnimation();
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		m_NavigationBehaviour?.Clear();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: false, ModalWindowUIType.PartySelector);
		});
		EventBus.RaiseEvent(delegate(ICullFocusHandler h)
		{
			h.HandleRestoreFocus();
		});
		if (partySelectorItemConsoleView != null)
		{
			partySelectorItemConsoleView.SetSelected();
		}
		foreach (PartySelectorItemConsoleView character in m_Characters)
		{
			character.Unbind();
		}
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, Vector2Int.one).AddTo(this);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "PartySelectorConsoleView"
		});
		if (IsInHub)
		{
			CreateTwoColumnsNavigation();
		}
		else
		{
			CreateNavigationWithRemote();
		}
		if (m_Characters.Count > 0)
		{
			PartySelectorItemConsoleView partySelectorItemConsoleView = m_Characters.FirstOrDefault((PartySelectorItemConsoleView c) => c != null && c.ViewModel != null && c.UnitEntityData == UtilityParty.GetCurrentSelectedUnit());
			partySelectorItemConsoleView = ((partySelectorItemConsoleView != null) ? partySelectorItemConsoleView : m_Characters.FirstOrDefault((PartySelectorItemConsoleView c) => c.ViewModel != null && (c.UnitEntityData?.IsDirectlyControllable() ?? false)));
			partySelectorItemConsoleView = ((partySelectorItemConsoleView != null) ? partySelectorItemConsoleView : m_Characters.FirstOrDefault((PartySelectorItemConsoleView c) => c.ViewModel != null));
			m_NavigationBehaviour.FocusOnEntityManual(partySelectorItemConsoleView, fullReset: false);
		}
		m_SelectedEntity = m_NavigationBehaviour.DeepestFocusAsObservable.Select((IConsoleEntity e) => e as PartySelectorItemConsoleView).ToReadOnlyReactiveProperty();
		m_SelectedEntity.Subscribe(delegate
		{
			OnFocusChanged();
		}).AddTo(this);
		CreateInput();
	}

	private void CreateNavigationWithRemote()
	{
		m_Content.constraintCount = 1;
		List<PartySelectorItemConsoleView> range = m_Characters.GetRange(0, m_Characters.Count);
		m_NavigationBehaviour.AddRow(range.Where((PartySelectorItemConsoleView item) => item.ViewModel != null && item.UnitEntityData.InPartyAndControllable()).ToList());
	}

	private void CreateTwoColumnsNavigation()
	{
		int num = ((m_Characters.Count((PartySelectorItemConsoleView c) => c.gameObject.activeSelf) <= 6) ? 1 : 2);
		m_Content.constraintCount = num;
		int num2 = Mathf.CeilToInt(1f * (float)m_Characters.Count / (float)num);
		List<PartySelectorItemConsoleView> range = m_Characters.GetRange(0, num2);
		List<PartySelectorItemConsoleView> range2 = m_Characters.GetRange(num2, m_Characters.Count - num2);
		m_NavigationBehaviour.AddRow(range.Where((PartySelectorItemConsoleView item) => item.ViewModel != null && item.UnitEntityData.InPartyAndControllable()).ToList());
		m_NavigationBehaviour.AddRow(range2.ToList());
	}

	private void CreateInput()
	{
		if (IsInHub || m_Characters.Count <= 0)
		{
			GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
			return;
		}
		if (m_Characters.Any((PartySelectorItemConsoleView c) => c.UnitEntityData.IsDirectlyControllable()))
		{
			m_LinkHint.Bind(m_InputLayer.AddButton(SetLink, 11, InputActionEventType.ButtonJustReleased)).AddTo(this);
			m_LinkAllHint.Bind(m_InputLayer.AddButton(SetMassLink, 11, InputActionEventType.ButtonJustLongPressed)).AddTo(this);
			m_LevelUpHint.Bind(m_InputLayer.AddButton(LevelUp, 10, m_IsLevelUp)).AddTo(this);
			m_LevelUpHint.SetLabel(UIStrings.Instance.MainMenu.LevelUp);
			m_MoveToNextHint.Bind(m_InputLayer.AddButton(delegate
			{
				MoveCharacter(next: true);
			}, 15)).AddTo(this);
			m_MoveToPreviousHint.Bind(m_InputLayer.AddButton(delegate
			{
				MoveCharacter(next: false);
			}, 14)).AddTo(this);
		}
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void SetLink(InputActionEventData data)
	{
		if (m_SelectedEntity != null && m_SelectedEntity.CurrentValue.UnitEntityData.IsDirectlyControllable())
		{
			BlueprintUISound.UISoundPartySelectorConsole partySelectorConsole = UISounds.Instance.Sounds.PartySelectorConsole;
			((m_SelectedEntity.CurrentValue.Or(null)?.IsLinked?.CurrentValue).GetValueOrDefault() ? partySelectorConsole.UnselectOne : partySelectorConsole.SelectOne).Play();
			m_SelectedEntity?.CurrentValue.SetLink();
			OnFocusChanged();
		}
	}

	private void SetMassLink(InputActionEventData data)
	{
		BlueprintUISound.UISoundPartySelectorConsole partySelectorConsole = UISounds.Instance.Sounds.PartySelectorConsole;
		((m_SelectedEntity.CurrentValue.Or(null)?.IsLinked?.CurrentValue).GetValueOrDefault() ? partySelectorConsole.UnselectAll : partySelectorConsole.SelectAll).Play();
		base.ViewModel.SetMassLink();
		OnFocusChanged();
	}

	private void LevelUp(InputActionEventData data)
	{
		m_SelectedEntity.CurrentValue.Or(null)?.LevelUp();
	}

	private void MoveCharacter(bool next)
	{
		base.ViewModel.SwitchCharacter(next);
	}

	private void OnFocusChanged()
	{
		string label = ((m_SelectedEntity.CurrentValue.Or(null)?.IsLinked?.CurrentValue).GetValueOrDefault() ? UIStrings.Instance.PartyTexts.Unlink : UIStrings.Instance.PartyTexts.Link);
		m_LinkHint.SetLabel(label);
		string label2 = (base.ViewModel.CharactersVM.Where((PartyCharacterVM c) => c.UnitEntityData != null).Any((PartyCharacterVM c) => c.IsLinked.CurrentValue) ? UIStrings.Instance.PartyTexts.UnlinkAll : UIStrings.Instance.PartyTexts.LinkAll);
		m_LinkAllHint.SetLabel(label2);
		m_IsLevelUp.Value = m_SelectedEntity.CurrentValue.Or(null)?.IsLevelUp.CurrentValue ?? false;
	}

	public void HandleSwitchPartyCharacters(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_Characters.FirstOrDefault((PartySelectorItemConsoleView c) => c.UnitEntityData == unit1));
		}).AddTo(this);
	}
}
