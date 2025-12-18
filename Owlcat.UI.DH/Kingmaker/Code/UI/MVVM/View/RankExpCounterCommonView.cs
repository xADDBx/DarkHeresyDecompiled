using Code.View.UI.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankExpCounterCommonView : View<CharInfoExperienceVM>
{
	[SerializeField]
	private RectTransform m_LayoutRect;

	[Header("Ranks")]
	[SerializeField]
	private bool n_HasNewRanks = true;

	[SerializeField]
	[ShowIf("n_HasNewRanks")]
	private GameObject m_RanksContainer;

	[SerializeField]
	[ShowIf("n_HasNewRanks")]
	private TextMeshProUGUI m_RanksDesc;

	[SerializeField]
	[ShowIf("n_HasNewRanks")]
	private TextMeshProUGUI m_RanksCount;

	[Header("Exp")]
	[SerializeField]
	private bool n_HasExp = true;

	[SerializeField]
	[ShowIf("n_HasExp")]
	private TextMeshProUGUI m_ExpLabel;

	[SerializeField]
	[ShowIf("n_HasExp")]
	private Image m_ExpProgressBar;

	[Header("Level")]
	[SerializeField]
	private bool n_HasLevel = true;

	[SerializeField]
	[ShowIf("n_HasLevel")]
	private TextMeshProUGUI m_CurrentLevelLabel;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_RanksDesc, m_RanksCount, m_ExpLabel, m_CurrentLevelLabel);
		}
		if (n_HasNewRanks)
		{
			base.ViewModel.NewRanksCount.Subscribe(delegate(int ranks)
			{
				m_RanksContainer.SetActive(ranks > 0);
				m_RanksCount.text = ranks.ToString();
			}).AddTo(this);
			this.SetHint(UIStrings.Instance.CharacterSheet.AvailableRanksHint).AddTo(this);
		}
		if (n_HasExp)
		{
			base.ViewModel.CurrentExp.CombineLatest(base.ViewModel.NextLevelExp, (int currentExp, int nextExp) => new { currentExp, nextExp }).Subscribe(value =>
			{
				m_ExpLabel.text = $"{value.currentExp}/{value.nextExp}";
			}).AddTo(this);
			base.ViewModel.CurrentLevelExpRatio.Subscribe(delegate(float value)
			{
				m_ExpProgressBar.fillAmount = value;
			}).AddTo(this);
		}
		if (n_HasLevel)
		{
			base.ViewModel.Level.Subscribe(delegate(int level)
			{
				m_CurrentLevelLabel.text = string.Format(UIStrings.Instance.CharacterSheet.CurrentLevelLabel.Text, level);
				LayoutRebuilder.ForceRebuildLayoutImmediate(m_LayoutRect);
			}).AddTo(this);
		}
		m_RanksDesc.text = string.Format(UIStrings.Instance.CharacterSheet.RanksCounterLabel.Text, "");
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		m_TextHelper.Dispose();
	}
}
