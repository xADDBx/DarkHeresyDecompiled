using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SaveLoadMenuVM : ViewModel
{
	public readonly SelectionGroupRadioVM<SaveLoadMenuEntityVM> SelectionGroup;

	private List<SaveLoadMenuEntityVM> m_EntitiesList;

	private readonly ReactiveProperty<SaveLoadMenuEntityVM> m_CurrentEntity = new ReactiveProperty<SaveLoadMenuEntityVM>();

	private readonly ReactiveProperty<SaveLoadMode> m_CurrentMode;

	private readonly List<SaveLoadMode> m_ModeList;

	private readonly ReactiveProperty<bool> m_HasFewEntities = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> HasFewEntities => m_HasFewEntities;

	public SaveLoadMenuVM(ReactiveProperty<SaveLoadMode> currentMode, List<SaveLoadMode> modeList)
	{
		m_CurrentMode = currentMode;
		m_ModeList = modeList;
		CreateEntities();
		SelectionGroup = new SelectionGroupRadioVM<SaveLoadMenuEntityVM>(m_EntitiesList, m_CurrentEntity).AddTo(this);
		m_CurrentEntity.Subscribe(delegate(SaveLoadMenuEntityVM e)
		{
			m_CurrentMode.Value = e?.Mode ?? SaveLoadMode.Load;
		}).AddTo(this);
	}

	private void CreateEntities()
	{
		m_EntitiesList = new List<SaveLoadMenuEntityVM>();
		foreach (SaveLoadMode mode in m_ModeList)
		{
			SaveLoadMenuEntityVM saveLoadMenuEntityVM = new SaveLoadMenuEntityVM(mode).AddTo(this);
			m_EntitiesList.Add(saveLoadMenuEntityVM);
			if (mode == m_CurrentMode.Value)
			{
				m_CurrentEntity.Value = saveLoadMenuEntityVM;
			}
		}
		m_HasFewEntities.Value = m_EntitiesList.Count > 1;
	}
}
