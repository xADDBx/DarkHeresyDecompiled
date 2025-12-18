using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SoulMarkRewardPCView : SoulMarkRewardBaseView
{
	[Header("PC Part")]
	[SerializeField]
	protected OwlcatButton m_AcceptButton;

	[SerializeField]
	protected TextMeshProUGUI m_AcceptText;

	[SerializeField]
	protected OwlcatButton m_DeclineButton;

	[SerializeField]
	protected TextMeshProUGUI m_DeclineText;

	protected override void OnBind()
	{
		base.OnBind();
		TextHelper.AppendTexts(m_AcceptText, m_DeclineText);
		ObservableSubscribeExtensions.Subscribe(m_AcceptButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnAcceptPressed();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_DeclineButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnDeclinePressed();
		}).AddTo(this);
		m_MainButton.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_AcceptText.text = UIStrings.Instance.PopUps.SeeOtherRanks;
		m_DeclineText.text = UIStrings.Instance.CommonTexts.Accept;
	}
}
