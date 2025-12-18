using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseStoryScenarioEntityIntegralDlcBaseView : SelectionGroupEntityView<NewGamePhaseStoryScenarioEntityIntegralDlcVM>, INewGameSwitchOnOffDlcHandler, ISubscriber
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private Sprite m_TransparentSprite;

	[SerializeField]
	private Sprite m_LittleCircleSprite;

	[SerializeField]
	private Sprite m_DlcAvailableSprite;

	[SerializeField]
	private Sprite m_DlcNotAvailableSprite;

	[SerializeField]
	private TextMeshProUGUI m_OnText;

	[SerializeField]
	private TextMeshProUGUI m_OffText;

	private bool m_IsSelected;

	private bool m_DlcIsOn;

	private PantographConfig PantographConfig { get; set; }

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Title.text = base.ViewModel.Title;
		m_OnText.text = UIStrings.Instance.SettingsUI.SettingsToggleOn;
		m_OffText.text = UIStrings.Instance.SettingsUI.SettingsToggleOff;
		m_DlcIsOn = base.ViewModel.BlueprintDlc.GetDlcSwitchOnOffState();
		m_Button.SetActiveLayer((!base.ViewModel.BlueprintDlc.IsPurchased) ? "NotAvailable" : (m_DlcIsOn ? "On" : "Off"));
		SetupPantographConfig();
		AddDisposable(EventBus.Subscribe(this));
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		m_IsSelected = value;
		if (value)
		{
			SetupPantographConfig();
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
			EventBus.RaiseEvent(delegate(INewGameChangeDlcHandler h)
			{
				h.HandleNewGameChangeDlc(base.ViewModel.Campaign, base.ViewModel.BlueprintDlc);
			});
		}
		OnChangeSelectedStateImpl();
	}

	protected virtual void OnChangeSelectedStateImpl()
	{
	}

	private void SetupPantographConfig()
	{
		List<Sprite> list = new List<Sprite> { m_LittleCircleSprite };
		if (!base.ViewModel.BlueprintDlc.IsPurchased)
		{
			list.Add(m_DlcNotAvailableSprite);
		}
		string textIcon = (base.ViewModel.BlueprintDlc.IsPurchased ? ((string)(m_DlcIsOn ? UIStrings.Instance.SettingsUI.SettingsToggleOn : UIStrings.Instance.SettingsUI.SettingsToggleOff)) : string.Empty);
		PantographConfig = new PantographConfig(base.transform, base.ViewModel.Title, list, useLargeView: false, textIcon);
	}

	public void HandleNewGameSwitchOnOffDlc(BlueprintDlc dlc, bool value)
	{
		if (dlc != base.ViewModel.BlueprintDlc)
		{
			return;
		}
		m_DlcIsOn = value;
		m_Button.SetActiveLayer((!base.ViewModel.BlueprintDlc.IsPurchased) ? "NotAvailable" : (value ? "On" : "Off"));
		if (m_IsSelected)
		{
			SetupPantographConfig();
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
		}
	}
}
