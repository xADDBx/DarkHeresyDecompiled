using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class LocationClueInformationDecorView : ClueInformationDecorBaseView
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_PaperTitle;

	[SerializeField]
	private TMP_Text m_PaperReportName;

	[SerializeField]
	private TMP_Text m_ClueNameLabel;

	[SerializeField]
	private List<GameObject> m_UniqueDecorations = new List<GameObject>();

	[Header("Values")]
	[SerializeField]
	private TextStyle m_TextStyle;

	protected override void OnBind()
	{
		m_PaperTitle.text = UIStrings.Instance.DetectiveDecor.PaperTitleLocation.Text;
		m_ClueNameLabel.text = UIStrings.Instance.DetectiveDecor.LocationNameLabel.Text;
		m_PaperReportName.styleSheet = m_TextStyle.StyleSheet;
		using (GameLogContext.Scope)
		{
			GameLogContext.TextStyle = m_TextStyle;
			GameLogContext.CaseItem = base.ViewModel;
			m_PaperReportName.text = UIStrings.Instance.DetectiveDecor.PaperReportNameLocation.Text;
			GameLogContext.TextStyle = UIConfig.Instance.DefaultTextStyle;
		}
		int num = Animator.StringToHash(base.ViewModel.AssetGuid) % m_UniqueDecorations.Count;
		for (int i = 0; i < m_UniqueDecorations.Count; i++)
		{
			m_UniqueDecorations[i].SetActive(i == num);
		}
	}
}
