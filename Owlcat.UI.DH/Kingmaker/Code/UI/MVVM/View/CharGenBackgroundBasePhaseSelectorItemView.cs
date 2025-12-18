using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenBackgroundBasePhaseSelectorItemView<TViewModel> : SelectionGroupEntityView<TViewModel> where TViewModel : CharGenBackgroundBaseItemVM
{
	[SerializeField]
	private TextMeshProUGUI m_DisplayName;

	protected PantographConfig PantographConfig { get; set; }

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DisplayName.text = base.ViewModel.DisplayName;
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

	protected virtual void SetupPantographConfig()
	{
		PantographConfig = new PantographConfig(base.transform, m_DisplayName.text);
	}

	private void OnSelected(bool value)
	{
		if (!value || !base.ViewModel.IsAvailable.CurrentValue)
		{
			return;
		}
		ReadOnlyReactiveProperty<CharGenPhaseBaseVM> currentPhase = base.ViewModel.CurrentPhase;
		if (currentPhase == null || currentPhase.CurrentValue.PhaseType != 0)
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
