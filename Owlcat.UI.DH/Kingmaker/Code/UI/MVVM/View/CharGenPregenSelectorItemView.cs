using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenPregenSelectorItemView : SelectionGroupEntityView<CharGenPregenSelectorItemVM>
{
	[SerializeField]
	private TextMeshProUGUI m_PregenNameText;

	[SerializeField]
	private GameObject m_PortraitGroup;

	[SerializeField]
	private Image m_PortraitImage;

	[SerializeField]
	private CharGenPregenPhasePantographItemView m_PantographItemView;

	public PantographConfig PantographConfig { get; protected set; }

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CharacterName.Subscribe(delegate(string e)
		{
			m_PregenNameText.text = e;
		}));
		SetupPantographConfig();
		AddDisposable(base.ViewModel.IsSelected.Subscribe(OnSelected));
		m_PortraitGroup.SetActive(base.ViewModel.Portrait.CurrentValue != null);
		m_PortraitImage.sprite = base.ViewModel.Portrait.CurrentValue;
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
		PantographConfig = new PantographConfig(base.transform, m_PantographItemView, base.ViewModel, useLargeView: true);
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
