using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueInformationScreenView : View<BlueprintClue>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_InformationLabel;

	[SerializeField]
	private TMP_Text m_CaseClueDecorText;

	[SerializeField]
	private List<GameObject> m_ScreenClueElements;

	[SerializeField]
	private List<GameObject> m_PaperClueElements;

	[SerializeField]
	private List<TMP_Text> m_BackButtonsLabels;

	[Header("Values")]
	[SerializeField]
	private string m_CaseDataFormat = "::: {0} :::";

	protected override void OnBind()
	{
		m_InformationLabel.text = string.Format(m_CaseDataFormat, UIStrings.Instance.DetectiveJournal.CaseDataLabel.Text);
		m_BackButtonsLabels.ForEach(delegate(TMP_Text b)
		{
			b.text = UIStrings.Instance.CommonTexts.Back.Text;
		});
		SetupBackButtons();
	}

	private void SetupBackButtons()
	{
		switch (base.ViewModel.GetUIData().UIType)
		{
		case BlueprintClue.UIType.Person:
		case BlueprintClue.UIType.Location:
			m_PaperClueElements.ForEach(delegate(GameObject i)
			{
				i.SetActive(value: true);
			});
			m_ScreenClueElements.ForEach(delegate(GameObject i)
			{
				i.SetActive(value: false);
			});
			break;
		default:
			m_PaperClueElements.ForEach(delegate(GameObject i)
			{
				i.SetActive(value: false);
			});
			m_ScreenClueElements.ForEach(delegate(GameObject i)
			{
				i.SetActive(value: true);
			});
			break;
		}
	}
}
