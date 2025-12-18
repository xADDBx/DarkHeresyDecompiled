using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class PaperReportDecorView : View<BlueprintCase>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_ReportTitle;

	[SerializeField]
	private TMP_Text m_ReportHeader;

	[SerializeField]
	private TMP_Text m_BottomDecorText;

	[SerializeField]
	private TMP_Text m_NameCharacter;

	[SerializeField]
	private TMP_Text m_ReportNumber;

	[Header("Values")]
	[SerializeField]
	private TextStyle m_TextStyle;

	protected override void OnBind()
	{
		DetectiveJournalDecor detectiveDecor = UIStrings.Instance.DetectiveDecor;
		using (GameLogContext.Scope)
		{
			GameLogContext.Case = base.ViewModel;
			m_ReportTitle.text = detectiveDecor.ReportTitle.Text;
		}
		string text = detectiveDecor.ReportAuthor.Text;
		m_ReportHeader.text = detectiveDecor.ReportDepartment.Text + "\n" + text;
		m_BottomDecorText.text = detectiveDecor.ReportDecorSignature.Text;
		m_NameCharacter.text = Game.Instance.Player.MainCharacter.Entity.CharacterName;
		m_ReportNumber.styleSheet = m_TextStyle.StyleSheet;
		using (GameLogContext.Scope)
		{
			GameLogContext.TextStyle = m_TextStyle;
			GameLogContext.Case = base.ViewModel;
			m_ReportNumber.text = UIStrings.Instance.DetectiveDecor.ReportNumberLabel.Text;
			GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
		}
	}
}
