using System;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseDifficultyConsoleView : NewGamePhaseDifficultyBaseView
{
	[Serializable]
	public class SettingsViews
	{
		[SerializeField]
		private SettingsEntityHeaderConsoleView m_SettingsEntityHeaderViewPrefab;

		[SerializeField]
		private SettingsEntityBoolConsoleView m_SettingsEntityBoolViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownConsoleView m_SettingsEntityDropdownViewPrefab;

		[SerializeField]
		private SettingsEntitySliderConsoleView m_SettingsEntitySliderViewPrefab;

		[SerializeField]
		private SettingsEntityDropdownGameDifficultyConsoleView m_SettingsEntityDropdownGameDifficultyViewPrefab;

		[SerializeField]
		private SettingsEntityBoolOnlyOneSaveConsoleView m_SettingsEntityBoolOnlyOneSaveViewPrefab;

		public void InitializeVirtualList(VirtualListComponent virtualListComponent)
		{
			virtualListComponent.Initialize(new VirtualListElementTemplate<SettingsEntityHeaderVM>(m_SettingsEntityHeaderViewPrefab), new VirtualListElementTemplate<SettingsEntityBoolVM>(m_SettingsEntityBoolViewPrefab), new VirtualListElementTemplate<SettingsEntityDropdownVM>(m_SettingsEntityDropdownViewPrefab, 0), new VirtualListElementTemplate<SettingsEntitySliderVM>(m_SettingsEntitySliderViewPrefab, 0), new VirtualListElementTemplate<SettingsEntityDropdownGameDifficultyVM>(m_SettingsEntityDropdownGameDifficultyViewPrefab, 0), new VirtualListElementTemplate<SettingsEntityBoolOnlyOneSaveVM>(m_SettingsEntityBoolOnlyOneSaveViewPrefab));
		}
	}

	[SerializeField]
	private SettingsViews m_SettingsViews;

	public void Initialize()
	{
		m_SettingsViews.InitializeVirtualList(m_VirtualList);
	}

	protected override void OnBind()
	{
		m_VirtualList.Subscribe(base.ViewModel.SettingEntities).AddTo(this);
		base.OnBind();
	}

	public void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * base.InfoView.ScrollRectExtended.scrollSensitivity);
		base.InfoView.ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl()
	{
	}
}
