using Kingmaker.Code.View.UI.UIUtilities;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenBackgroundBasePhaseSelectorItemView<TViewModel> : SelectionGroupEntityView<TViewModel> where TViewModel : CharGenBackgroundBaseItemVM
{
	protected const string BUTTON_LAYER_NORMAL = "Normal";

	protected const string BUTTON_LAYER_CHOSEN = "Selected";

	protected const string BUTTON_LAYER_NOT_AVAILABLE = "NotAvailable";

	protected const string BUTTON_LAYER_NOT_AVAILABLE_TAKEN = "NotAvailableTaken";

	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_DisplayName;

	[SerializeField]
	private Image m_Icon;

	protected override void OnBind()
	{
		base.OnBind();
		m_DisplayName.text = base.ViewModel.Feature.Name;
		if ((bool)m_Icon)
		{
			m_Icon.sprite = base.ViewModel.Feature.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		}
		m_Button.OnHoverAsObservable().Subscribe(base.ViewModel.SetHovered).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnRightClickAsObservable(), delegate
		{
			TooltipHelper.ShowInfo(base.ViewModel.Template);
		}).AddTo(this);
		AddDisposable(base.ViewModel.IsSelected.Subscribe(OnSelected));
		base.ViewModel.State.Subscribe(delegate
		{
			UpdateAccessibility();
		}).AddTo(this);
	}

	protected override void OnClick()
	{
		if (UtilityNet.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	private void OnSelected(bool value)
	{
		UpdateAccessibility();
		m_Button.SetFocused(value);
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value)
		{
			base.ViewModel.SetSelectedFromView(state: true);
		}
	}

	protected virtual void UpdateAccessibility()
	{
		OwlcatMultiButton button = m_Button;
		button.SetActiveLayer(base.ViewModel.State.CurrentValue switch
		{
			LEVEL_UP_ITEM_STATE.Available => base.ViewModel.IsSelected.Value ? "Selected" : "Normal", 
			LEVEL_UP_ITEM_STATE.NotAvailable => "NotAvailable", 
			LEVEL_UP_ITEM_STATE.AlreadyExist => "NotAvailableTaken", 
			_ => "Normal", 
		});
	}
}
