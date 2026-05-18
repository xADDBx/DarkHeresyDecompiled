using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public sealed class CharacterVisualSettingsPCView : CharacterVisualSettingsView<CharacterVisualSettingsEntityPCView>
{
	[Header("Buttons")]
	[SerializeField]
	private bool m_HasCloseButton = true;

	[SerializeField]
	[ShowIf("m_HasCloseButton")]
	private OwlcatMultiButton m_Close;

	[SerializeField]
	private bool m_HasCompleteButton = true;

	[SerializeField]
	[ShowIf("m_HasCompleteButton")]
	private OwlcatMultiButton m_Complete;

	[SerializeField]
	[ShowIf("m_HasCompleteButton")]
	private TextMeshProUGUI m_CompleteLabel;

	protected override void OnBind()
	{
		base.OnBind();
		if (m_HasCloseButton)
		{
			UISounds.Instance.SetClickAndHoverSound(m_Close, ButtonSoundsEnum.PlastickSound);
			ObservableSubscribeExtensions.Subscribe(m_Close.OnLeftClickAsObservable(), delegate
			{
				Close();
			}).AddTo(this);
		}
		if (m_HasCompleteButton)
		{
			ObservableSubscribeExtensions.Subscribe(m_Complete.OnLeftClickAsObservable(), delegate
			{
				Close();
			}).AddTo(this);
			m_CompleteLabel.text = UIStrings.Instance.CharGen.Complete;
		}
	}

	public void Close()
	{
		base.ViewModel.Close();
	}
}
