using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class NewAddendumView : View<BlueprintClueAddendum>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Description;

	protected override void OnBind()
	{
		m_Description.text = base.ViewModel.Description.Text;
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
