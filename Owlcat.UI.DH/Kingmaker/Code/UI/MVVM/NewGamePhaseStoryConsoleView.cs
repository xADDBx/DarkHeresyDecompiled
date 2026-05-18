using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryConsoleView : NewGamePhaseStoryBaseView
{
	[SerializeField]
	private NewGamePhaseStoryScenarioSelectorConsoleView m_StorySelectorConsoleView;

	[SerializeField]
	private CustomUIVideoPlayerConsoleView m_CustomUIVideoPlayerConsoleView;

	[SerializeField]
	private HintView m_ScrollStoryHint;

	[SerializeField]
	private HintView m_PurchaseHint;

	[SerializeField]
	private HintView m_InstallHint;

	public override void Initialize()
	{
		base.Initialize();
		if (!IsInit)
		{
			m_CustomUIVideoPlayerConsoleView.Initialize();
			IsInit = true;
		}
	}

	protected override void OnBind()
	{
		m_CustomUIVideoPlayerConsoleView.Bind(base.ViewModel.CustomUIVideoPlayerVM);
		base.OnBind();
		m_StorySelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl()
	{
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_StorySelectorConsoleView.GetNavigationEntities();
	}

	protected override void ShowHideVideoImpl(bool state)
	{
		base.ShowHideVideoImpl(state);
		m_CustomUIVideoPlayerConsoleView.gameObject.SetActive(state);
	}
}
