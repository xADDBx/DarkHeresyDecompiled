using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class InventoryDollPCView : InventoryDollView<InventoryEquipSlotPCView>
{
	[Header("Character Visual Settings")]
	[SerializeField]
	private OwlcatMultiButton m_VisualSettingsViewButton;

	[SerializeField]
	private CharacterVisualSettingsPCView m_VisualSettingsPCView;

	public override void Initialize()
	{
		base.Initialize();
		if (m_VisualSettingsPCView != null)
		{
			m_VisualSettingsPCView.Initialize();
		}
	}

	protected override void OnBind()
	{
		base.OnBind();
		if (m_VisualSettingsPCView != null)
		{
			UISounds.Instance.SetClickAndHoverSound(m_VisualSettingsViewButton, ButtonSoundsEnum.PlastickSound);
			m_VisualSettingsViewButton.SetHint(UIStrings.Instance.CharGen.ShowVisualSettings).AddTo(this);
			base.ViewModel.VisualSettingsVM.Subscribe(m_VisualSettingsPCView.Bind).AddTo(this);
			ObservableSubscribeExtensions.Subscribe(m_VisualSettingsViewButton.OnLeftClickAsObservable(), delegate
			{
				base.ViewModel.SwitchVisualSettings();
			}).AddTo(this);
		}
	}
}
