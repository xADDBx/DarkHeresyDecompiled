using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerModEntityConsoleView : DlcManagerModEntityBaseView, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, IConsoleEntity, INavigationRightDirectionHandler
{
	[SerializeField]
	private HintView m_ModSettingsHint;

	public void CreateInputImpl()
	{
	}

	public bool GetAvailableSettings()
	{
		return base.ViewModel.ModSettingsAvailable.CurrentValue;
	}

	protected override void OnChangeSelectedStateImpl(bool value)
	{
		base.OnChangeSelectedStateImpl(value);
		base.ViewModel.SelectMe();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public override bool IsValid()
	{
		return base.gameObject.activeSelf;
	}

	public bool HandleLeft()
	{
		SwitchValue();
		return true;
	}

	public bool HandleRight()
	{
		SwitchValue();
		return true;
	}
}
