using System.Collections.Generic;
using System.Linq;
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
				break;
			}
			TStatCheckLootUnitCardView val = m_UnitCardSlots[i];
			val.Bind(base.ViewModel.UnitSlotVMByStatType.ElementAt(i).Value);
			val.gameObject.SetActive(value: true);
		}
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
