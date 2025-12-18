using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabModsConsoleView : DlcManagerTabModsBaseView
{
	[Header("Console Part")]
	[SerializeField]
	private DlcManagerTabModsModSelectorConsoleView m_ModSelectorConsoleView;

	[SerializeField]
	private ConsoleHint m_OpenNexusModsHint;

	[SerializeField]
	private ConsoleHint m_OpenSteamWorkshopHint;

	protected override void OnBind()
	{
		base.OnBind();
		m_ModSelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_InfoView.ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ConsoleHint openModSettingsHint, ReactiveProperty<bool> modSettingsIsAvailable)
	{
		openModSettingsHint.Bind(inputLayer.AddButton(delegate
		{
		}, 17, base.ViewModel.IsEnabled.And(modSettingsIsAvailable).ToReadOnlyReactiveProperty(initialValue: false))).AddTo(this);
		openModSettingsHint.SetLabel(UIStrings.Instance.DlcManager.ModSettings);
		m_OpenNexusModsHint.Bind(inputLayer.AddButton(delegate
		{
			OpenNexusMods();
		}, 10, base.ViewModel.IsEnabled, InputActionEventType.ButtonJustReleased)).AddTo(this);
		m_OpenSteamWorkshopHint.Bind(inputLayer.AddButton(delegate
		{
			OpenSteamWorkshop();
		}, 11, base.ViewModel.IsEnabled.And(base.ViewModel.IsSteam).ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased)).AddTo(this);
		m_ModSelectorConsoleView.CreateInputImpl(inputLayer, hintsWidget);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_ModSelectorConsoleView.GetNavigationEntities();
	}
}
