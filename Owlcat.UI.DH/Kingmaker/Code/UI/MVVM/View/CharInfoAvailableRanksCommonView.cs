using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharInfoAvailableRanksCommonView : View<CharInfoAvailableRanksVM>
{
	[SerializeField]
	private GameObject m_RanksContainer;

	[SerializeField]
	private TextMeshProUGUI m_RanksDesc;

	[SerializeField]
	private TextMeshProUGUI m_RanksCount;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_RanksCount, m_RanksDesc);
		}
		base.ViewModel.NewRanksCount.Subscribe(delegate(int ranks)
		{
			m_RanksContainer.SetActive(ranks > 0 && base.ViewModel.IsInLevelupProcess);
			m_RanksCount.text = ranks.ToString();
		}).AddTo(this);
		this.SetHint(UIStrings.Instance.CharacterSheet.AvailableRanksHint).AddTo(this);
		m_RanksDesc.text = string.Format(UIStrings.Instance.CharacterSheet.RanksCounterLabel.Text, "");
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper.Dispose();
	}
}
