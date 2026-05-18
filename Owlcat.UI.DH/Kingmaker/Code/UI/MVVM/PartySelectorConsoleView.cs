using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
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
	private HintView m_LinkAllHint;

	[SerializeField]
	private HintView m_LinkHint;

	[SerializeField]
	private HintView m_LevelUpHint;

	[SerializeField]
	private HintView m_MoveToNextHint;

	[SerializeField]
	private HintView m_MoveToPreviousHint;

	private List<PartySelectorItemConsoleView> m_CreatedItems = new List<PartySelectorItemConsoleView>();

	private List<PartySelectorItemConsoleView> m_Characters = new List<PartySelectorItemConsoleView>();

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
		ModalWindowsSounds.Instance.MessageBox.Show.Play();
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
		m_FadeAnimator.DisappearAnimation();
		ModalWindowsSounds.Instance.MessageBox.Hide.Play();
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state: false, ModalWindowUIType.PartySelector);
		});
		EventBus.RaiseEvent(delegate(ICullFocusHandler h)
		{
			h.HandleRestoreFocus();
		});
		foreach (PartySelectorItemConsoleView character in m_Characters)
		{
			character.Unbind();
		}
	}

	private void CreateInput()
	{
	}

	private void SetLink()
	{
		if (m_SelectedEntity != null && m_SelectedEntity.CurrentValue.UnitEntityData.IsDirectlyControllable())
		{
			ModalWindowsSounds.UISoundPartySelectorConsole partySelectorConsole = ModalWindowsSounds.Instance.PartySelectorConsole;
			((m_SelectedEntity.CurrentValue.Or(null)?.IsLinked?.CurrentValue).GetValueOrDefault() ? partySelectorConsole.UnselectOne : partySelectorConsole.SelectOne).Play();
			m_SelectedEntity?.CurrentValue.SetLink();
			OnFocusChanged();
		}
	}

	private void SetMassLink()
	{
		ModalWindowsSounds.UISoundPartySelectorConsole partySelectorConsole = ModalWindowsSounds.Instance.PartySelectorConsole;
		((m_SelectedEntity.CurrentValue.Or(null)?.IsLinked?.CurrentValue).GetValueOrDefault() ? partySelectorConsole.UnselectAll : partySelectorConsole.SelectAll).Play();
		base.ViewModel.SetMassLink();
		OnFocusChanged();
	}

	private void LevelUp()
	{
		m_SelectedEntity.CurrentValue.Or(null)?.LevelUp();
	}

	private void MoveCharacter(bool next)
	{
		base.ViewModel.SwitchCharacter(next);
	}

	private void OnFocusChanged()
	{
	}

	public void HandleSwitchPartyCharacters(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
	}
}
