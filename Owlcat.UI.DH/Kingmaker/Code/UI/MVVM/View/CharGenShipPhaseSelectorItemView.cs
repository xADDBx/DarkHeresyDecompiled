using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenShipPhaseSelectorItemView : SelectionGroupEntityView<CharGenShipItemVM>
{
	[SerializeField]
	private TextMeshProUGUI m_DisplayName;

	public PantographConfig PantographConfig { get; protected set; }

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DisplayName.text = base.ViewModel.Title;
		SetupPantographConfig();
		AddDisposable(base.ViewModel.IsSelected.Subscribe(OnSelected));
	}

	protected virtual void SetupPantographConfig()
	{
		PantographConfig = new PantographConfig(base.transform, m_DisplayName.text);
	}

	private void OnSelected(bool value)
	{
		if (value && base.ViewModel.IsAvailable.CurrentValue)
		{
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
		}
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value)
		{
			base.ViewModel.SetSelectedFromView(state: true);
		}
		EventBus.RaiseEvent(delegate(IPantographHandler h)
		{
			h.SetFocus(value);
		});
	}
}
