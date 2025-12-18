using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueInfoNoteView : View<BlueprintClueNote>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_NoteText;

	protected override void OnBind()
	{
		m_NoteText.text = base.ViewModel.Text.Text;
	}
}
