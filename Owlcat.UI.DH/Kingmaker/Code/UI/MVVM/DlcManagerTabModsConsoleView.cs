using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabModsConsoleView : DlcManagerTabModsBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private DlcManagerTabModsModSelectorConsoleView m_ModSelectorConsoleView;

	[SerializeField]
	private HintView m_OpenNexusModsHint;

	[SerializeField]
	private HintView m_OpenSteamWorkshopHint;

	protected override void OnBind()
	{
		base.OnBind();
		m_ModSelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_InfoView.ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl()
	{
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_ModSelectorConsoleView.GetNavigationEntities();
	}
}
