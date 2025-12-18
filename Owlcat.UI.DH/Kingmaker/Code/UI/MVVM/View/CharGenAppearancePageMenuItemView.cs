using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAppearancePageMenuItemView : SelectionGroupEntityView<CharGenAppearancePageVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ButtonLabel;

	public PantographConfig PantographConfig { get; private set; }

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ButtonLabel.text = base.ViewModel.PageLabel;
		SetupPantographConfig();
		AddDisposable(base.ViewModel.IsSelected.Subscribe(OnSelected));
	}

	protected override void OnClick()
	{
		if (UtilityNet.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	private void SetupPantographConfig()
	{
		PantographConfig = new PantographConfig(base.transform, m_ButtonLabel.text);
	}

	private void OnSelected(bool value)
	{
		if (value && base.ViewModel.IsAvailable.CurrentValue && base.ViewModel.IsInDetailedView.CurrentValue)
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
		base.ViewModel.SetSelectedFromView(value);
	}
}
