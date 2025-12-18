using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharacterVisualSettingsPCView : CharacterVisualSettingsView<CharacterVisualSettingsEntityPCView>
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_Close;

	[SerializeField]
	private OwlcatMultiButton m_Complete;

	[SerializeField]
	private TextMeshProUGUI m_CompleteLabel;

	protected override void OnBind()
	{
		base.OnBind();
		if (m_Close != null)
		{
			UISounds.Instance.SetClickAndHoverSound(m_Close, ButtonSoundsEnum.PlastickSound);
			ObservableSubscribeExtensions.Subscribe(m_Close.OnLeftClickAsObservable(), delegate
			{
				Close();
			}).AddTo(this);
		}
		if (m_Complete != null)
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
