using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionFalseCaseItemView : View<BlueprintCaseItem>
{
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_Description;

	protected override void OnBind()
	{
		m_Title.text = UIStrings.Instance.DetectiveJournal.RefutedLabel.Text;
		m_Description.text = base.ViewModel.Description.Text;
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
