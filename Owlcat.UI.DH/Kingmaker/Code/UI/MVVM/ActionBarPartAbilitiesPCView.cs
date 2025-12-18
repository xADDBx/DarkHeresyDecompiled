using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartAbilitiesPCView : ActionBarPartAbilitiesBaseView
{
	[SerializeField]
	private ActionBarAbilitiesRowView m_RowView;

	[SerializeField]
	private OwlcatMultiButton m_NextRowButton;

	[SerializeField]
	private OwlcatMultiButton m_PrevRowButton;

	[SerializeField]
	private ActionBarSlotAbilityPCView m_SlotPCView;

	[SerializeField]
	private OwlcatMultiSelectable m_RowIndexIndicator;

	[Header("Hotkey block")]
	[SerializeField]
	private TextMeshProUGUI m_HotkeyText;

	[SerializeField]
	private GameObject m_HotkeyContainer;

	private int m_CurrentRowIndex;

	private string m_BindName;

	private SettingsEntityKeyBindingPair m_Binding;

	private List<ActionBarSlotAbilityPCView> m_BoundSlots;

	public override void Initialize()
	{
		m_RowView.Initialize(m_TooltipPlace, new List<Vector2> { Vector2.zero });
		m_BindName = "ChangeAbilitySet";
		m_Binding = SettingsRoot.Controls.Keybindings.ActionBar.GetBindingPair("change-ability-set");
		SetChangeSetKeyBindLabel();
	}

	protected override void OnBind()
	{
		base.ViewModel.UnitChanged.Subscribe(DrawEntries).AddTo(this);
		base.ViewModel.SlotCountChanged.Subscribe(DrawEntries).AddTo(this);
		m_NextRowButton.OnLeftClickAsObservable().Subscribe(SelectNextRow).AddTo(this);
		m_PrevRowButton.OnLeftClickAsObservable().Subscribe(SelectPrevRow).AddTo(this);
		Game.Instance.Keyboard.Bind(m_BindName, SelectNextRow).AddTo(this);
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged += OnChangeSetBindingChanged;
		}
		DrawEntries();
	}

	protected override void OnUnbind()
	{
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged -= OnChangeSetBindingChanged;
		}
	}

	private void SelectNextRow()
	{
		int count = base.ViewModel.Slots.Count;
		if (count != 0)
		{
			int num = Mathf.CeilToInt((float)count / (float)base.SlotsInRow);
			m_CurrentRowIndex = (m_CurrentRowIndex + 1) % num;
			DrawEntries();
		}
	}

	private void SelectPrevRow()
	{
		int count = base.ViewModel.Slots.Count;
		if (count != 0)
		{
			int num = Mathf.CeilToInt((float)count / (float)base.SlotsInRow);
			m_CurrentRowIndex = (m_CurrentRowIndex - 1 + num) % num;
			DrawEntries();
		}
	}

	private void DrawEntries()
	{
		if (base.ViewModel.Slots.Count != 0)
		{
			m_RowIndexIndicator.SetActiveLayer(m_CurrentRowIndex);
			int count = m_CurrentRowIndex * base.SlotsInRow;
			List<ActionBarSlotVM> slots = base.ViewModel.Slots.Skip(count).Take(base.SlotsInRow).ToList();
			m_RowView.DrawEntries(slots, m_SlotPCView).AddTo(this);
			SetAbilitiesKeyBindings();
		}
	}

	private void SetAbilitiesKeyBindings()
	{
		ClearAbilitiesKeyBindings();
		m_BoundSlots?.Clear();
		if (m_BoundSlots == null)
		{
			m_BoundSlots = new List<ActionBarSlotAbilityPCView>();
		}
		m_BoundSlots.AddRange(m_RowView.GetSlots().Cast<ActionBarSlotAbilityPCView>());
		for (int i = 0; i < m_BoundSlots.Count; i++)
		{
			m_BoundSlots[i].SetKeyBinding(i);
		}
	}

	private void ClearAbilitiesKeyBindings()
	{
		if (m_BoundSlots == null)
		{
			return;
		}
		foreach (ActionBarSlotAbilityPCView boundSlot in m_BoundSlots)
		{
			boundSlot.ClearKeyBinding();
		}
	}

	private void OnChangeSetBindingChanged(KeyBindingPair obj)
	{
		SetChangeSetKeyBindLabel();
	}

	private void SetChangeSetKeyBindLabel()
	{
		string stringByBinding = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(m_BindName));
		m_HotkeyText.text = stringByBinding;
		m_HotkeyContainer.SetActive(stringByBinding.Length > 0);
	}
}
