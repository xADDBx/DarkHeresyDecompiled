using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickMinimalAdmissibleDamageView : BrickBaseView<BrickMinimalAdmissibleDamageVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_HeaderText;

	[SerializeField]
	private TMP_Text m_ResultValueText;

	[SerializeField]
	private TMP_Text m_MinimalAdmissibleDamageText;

	[SerializeField]
	private TMP_Text m_MinimalAdmissibleDamageValueText;

	[SerializeField]
	private TMP_Text m_ReasonsText;

	[SerializeField]
	private TMP_Text m_GameDifficultyText;

	protected override void OnBind()
	{
		m_HeaderText.text = GameLogStrings.Instance.TooltipBrickStrings.MinimalAdmissibleDamageHeader.Text;
		TMP_Text resultValueText = m_ResultValueText;
		int minimalAdmissibleDamage = base.ViewModel.MinimalAdmissibleDamage;
		resultValueText.text = "=" + minimalAdmissibleDamage;
		m_MinimalAdmissibleDamageText.text = GameLogStrings.Instance.TooltipBrickStrings.MinimalAdmissibleDamage.Text;
		m_MinimalAdmissibleDamageValueText.text = base.ViewModel.ReasonValue;
		m_ReasonsText.text = GameLogStrings.Instance.TooltipBrickStrings.MinimalAdmissibleDamageReason.Text;
		GameDifficultyOption currentGameDifficulty = SettingsRoot.Difficulty.GameDifficulty.GetValue();
		DifficultyPresetAsset difficultyPresetAsset = ConfigRoot.Instance.DifficultyList.Difficulties.First((DifficultyPresetAsset asset) => asset.Preset.GameDifficulty == currentGameDifficulty);
		m_GameDifficultyText.text = GameLogStrings.Instance.TooltipBrickStrings.MinimalAdmissibleDamageReasonValue.Text + " <b>" + difficultyPresetAsset.LocalizedName.Text + "</b>";
	}
}
