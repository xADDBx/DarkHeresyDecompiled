using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

[ViewFactoryPolicy(ViewFactoryPolicyFlag.DontPool, null)]
public class GroupChangerPCView : GroupChangerBaseView
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_AcceptButton;

	[SerializeField]
	private TextMeshProUGUI m_AcceptLabel;

	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	protected override void OnBind()
	{
		base.OnBind();
		bool flag = UtilityNet.IsControlMainCharacter();
		m_AcceptButton.Or(null)?.gameObject.SetActive(flag);
		if (flag)
		{
			ObservableSubscribeExtensions.Subscribe(m_AcceptButton.OnLeftClickAsObservable(), delegate
			{
				OnAccept();
			}).AddTo(this);
			if (m_AcceptLabel != null)
			{
				m_AcceptLabel.text = UIStrings.Instance.CommonTexts.Accept;
			}
		}
		m_CloseButton.Interactable = base.ViewModel.CloseEnabled.CurrentValue;
		if (base.ViewModel.CloseEnabled.CurrentValue)
		{
			ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
			{
				OnCancel();
			}).AddTo(this);
			EscHotkeyManager.Instance.Subscribe(base.OnCancel).AddTo(this);
		}
	}
}
