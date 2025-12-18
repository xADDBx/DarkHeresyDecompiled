using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootUnitsPageBaseView<TStatCheckLootUnitCardView, TStatCheckLootSmallUnitCardView> : StatCheckLootPageView<StatCheckLootUnitsPageVM> where TStatCheckLootUnitCardView : StatCheckLootUnitCardBaseView where TStatCheckLootSmallUnitCardView : StatCheckLootSmallUnitCardBaseView
{
	[SerializeField]
	private TextMeshProUGUI m_SwitchUnitSubHeaderLabel;

	[SerializeField]
	private TStatCheckLootUnitCardView m_SelectedUnitCardSlot;

	[SerializeField]
	private List<TStatCheckLootSmallUnitCardView> m_SmallUnitSlots;

	[SerializeField]
	private OwlcatToggleGroup m_ToggleGroup;

	protected override void InitializeImpl()
	{
		m_SelectedUnitCardSlot.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.SelectedUnitCardVM.Subscribe(m_SelectedUnitCardSlot.Bind).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.UpdateSmallUnitSlots, delegate
		{
			UpdateSmallUnitSlots();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ClearSmallUnitSlots, delegate
		{
			ClearSmallUnitSlots();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_InputLayer = null;
	}

	protected override void BuildNavigationImpl()
	{
		m_NavigationBehaviour.SetEntitiesHorizontal(m_SmallUnitSlots);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "StatCheckLootUnitsPageBaseViewInput"
		});
		CreateInputImpl(m_InputLayer);
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
	}

	protected void OnBackWithoutConfirmUnit()
	{
		base.ViewModel.BackWithoutConfirmUnit();
	}

	protected void OnConfirmUnit()
	{
		base.ViewModel.ConfirmUnit();
	}

	private void UpdateSmallUnitSlots()
	{
		ClearSmallUnitSlots();
		int num = -1;
		for (int i = 0; i < base.ViewModel.SmallUnitSlotsVMs.Count; i++)
		{
			if (i >= m_SmallUnitSlots.Count)
			{
				PFLog.UI.Error("StatCheckLootUnitsPageBaseView.UpdateSmallUnitSlots - SmallUnitSlotsVMs count is more than slots count!");
				return;
			}
			TStatCheckLootSmallUnitCardView val = m_SmallUnitSlots[i];
			if (base.ViewModel.SmallUnitSlotsVMs[i].IsSelected.CurrentValue)
			{
				num = i;
			}
			val.Bind(base.ViewModel.SmallUnitSlotsVMs[i]);
			val.SetToggleGroup(m_ToggleGroup);
			val.gameObject.SetActive(value: true);
		}
		if (num > -1)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_SmallUnitSlots[num]);
		}
		else
		{
			m_NavigationBehaviour.FocusOnCurrentEntity();
		}
	}

	private void ClearSmallUnitSlots()
	{
		foreach (TStatCheckLootSmallUnitCardView smallUnitSlot in m_SmallUnitSlots)
		{
			smallUnitSlot.Unbind();
			smallUnitSlot.gameObject.SetActive(value: false);
		}
	}
}
