using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConstructConclusionScreenView : View<BlueprintCaseItem>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_InformationLabel;

	[SerializeField]
	private TMP_Text m_CaseClueDecorText;

	[Header("Values")]
	[SerializeField]
	private string m_CaseDataFormat = "::: {0} :::";

	protected override void OnBind()
	{
		m_InformationLabel.text = string.Format(m_CaseDataFormat, UIStrings.Instance.DetectiveJournal.ChooseConclusionLabel.Text);
	}
}
