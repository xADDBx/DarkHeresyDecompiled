using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class VendorTransitionWindowPCView : VendorTransitionWindowView
{
	[SerializeField]
	protected OwlcatButton m_AcceptButton;

	[SerializeField]
	private TextMeshProUGUI m_AcceptButtonLabel;

	[FormerlySerializedAs("m_СancelButton")]
	[SerializeField]
	protected OwlcatButton m_CancelButton;

	[SerializeField]
	private TextMeshProUGUI m_СancelButtonLabel;

	protected override void OnBind()
	{
		base.OnBind();
		m_СancelButtonLabel.text = UIStrings.Instance.CommonTexts.Cancel;
		m_AcceptButtonLabel.text = UIStrings.Instance.CommonTexts.Accept;
		m_AcceptButton.OnLeftClickAsObservable().Subscribe(base.Deal).AddTo(this);
		m_CancelButton.OnLeftClickAsObservable().Subscribe(base.Close).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.Close).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		Close();
	}
}
