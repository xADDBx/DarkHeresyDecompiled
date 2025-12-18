using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ServiceWindowsMenuVM : ViewModel
{
	private List<ServiceWindowsMenuEntityVM> m_EntitiesList;

	private readonly ReactiveProperty<ServiceWindowsMenuEntityVM> m_SelectedEntity;

	private readonly Action<ServiceWindowsType> m_OnSelect;

	public readonly SelectionGroupRadioVM<ServiceWindowsMenuEntityVM> SelectionGroup;

	private readonly ReactiveProperty<bool> m_IsAdditionalBackgroundNeeded = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsAdditionalBackgroundNeeded => m_IsAdditionalBackgroundNeeded;

	public ServiceWindowsMenuVM(Action<ServiceWindowsType> onSelect)
	{
		m_OnSelect = onSelect;
		CreateEntities();
		m_SelectedEntity = new ReactiveProperty<ServiceWindowsMenuEntityVM>();
		SelectionGroup = new SelectionGroupRadioVM<ServiceWindowsMenuEntityVM>(m_EntitiesList, m_SelectedEntity).AddTo(this);
		m_SelectedEntity.Skip(1).Subscribe(OnEntitySelected).AddTo(this);
	}

	public void SelectWindow(ServiceWindowsType type)
	{
		ReactiveProperty<bool> isAdditionalBackgroundNeeded = m_IsAdditionalBackgroundNeeded;
		ServiceWindowsType currentServiceWindow = RootUIContext.Instance.CurrentServiceWindow;
		isAdditionalBackgroundNeeded.Value = currentServiceWindow == ServiceWindowsType.Encyclopedia || currentServiceWindow == ServiceWindowsType.Journal || currentServiceWindow == ServiceWindowsType.LocalMap;
		ServiceWindowsMenuEntityVM currentValue = m_SelectedEntity.CurrentValue;
		m_SelectedEntity.Value = m_EntitiesList.FirstOrDefault((ServiceWindowsMenuEntityVM e) => e.ServiceWindowsType == type);
		if (currentValue == m_SelectedEntity.CurrentValue)
		{
			m_SelectedEntity.ForceNotify();
		}
	}

	private void CreateEntities()
	{
		m_EntitiesList = new List<ServiceWindowsMenuEntityVM>();
		foreach (ServiceWindowsType value in Enum.GetValues(typeof(ServiceWindowsType)))
		{
			if (value != 0)
			{
				ServiceWindowsMenuEntityVM item = new ServiceWindowsMenuEntityVM(value).AddTo(this);
				m_EntitiesList.Add(item);
			}
		}
	}

	private void OnEntitySelected(ServiceWindowsMenuEntityVM entity)
	{
		m_OnSelect?.Invoke(entity?.ServiceWindowsType ?? ServiceWindowsType.None);
	}

	public void Close()
	{
		if (!RootUIContext.Instance.ServiceWindowNowIsOpening)
		{
			m_OnSelect?.Invoke(ServiceWindowsType.None);
		}
	}
}
