using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class StatCheckLootMainPageBaseView<TStatCheckLootUnitCardView> : StatCheckLootPageView<StatCheckLootMainPageVM> where TStatCheckLootUnitCardView : StatCheckLootUnitCardBaseView
{
	[SerializeField]
	private List<TStatCheckLootUnitCardView> m_UnitCardSlots;

	protected override void InitializeImpl()
	{
		foreach (TStatCheckLootUnitCardView unitCardSlot in m_UnitCardSlots)
		{
			unitCardSlot.Initialize();
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.UpdateUnitSlots.Subscribe(UpdateUnitSlots).AddTo(this);
		base.ViewModel.ClearUnitSlots.Subscribe(ClearUnitSlots).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_InputLayer = null;
	}

	protected override void BuildNavigationImpl()
	{
		m_NavigationBehaviour.SetEntitiesHorizontal(m_UnitCardSlots);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "StatCheckLootMainPageBaseViewInput"
		});
		CreateInputImpl(m_InputLayer);
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer)
	{
	}

	protected void OnCheckStat()
	{
		base.ViewModel.CheckStat();
	}

	protected void OnClose()
	{
		base.ViewModel.CloseDialog();
	}

	protected void OnSwitchUnit()
	{
		base.ViewModel.SwitchUnit();
	}

	private void UpdateUnitSlots()
	{
		ClearUnitSlots();
		for (int i = 0; i < base.ViewModel.UnitSlotVMByStatType.Count; i++)
		{
			if (i >= m_UnitCardSlots.Count)
			{
				PFLog.UI.Error("StatCheckLootMainPageBaseView.UpdateUnitSlots - UnitSlotVMs count is more than slots count!");
				return;
			}
			TStatCheckLootUnitCardView val = m_UnitCardSlots[i];
			val.Bind(base.ViewModel.UnitSlotVMByStatType.ElementAt(i).Value);
			val.gameObject.SetActive(value: true);
		}
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void ClearUnitSlots()
	{
		foreach (TStatCheckLootUnitCardView unitCardSlot in m_UnitCardSlots)
		{
			unitCardSlot.Unbind();
			unitCardSlot.gameObject.SetActive(value: false);
		}
	}
}
