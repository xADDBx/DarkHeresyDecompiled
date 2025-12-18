using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueInfoNotesView : View<ClueInfoNotesVM>
{
	[Header("Elements")]
	[SerializeField]
	private WidgetList m_NotesContainer;

	[Header("Views")]
	[SerializeField]
	private ClueInfoNoteView NoteViewPrefab;

	protected override void OnBind()
	{
		DrawNotes();
		base.ViewModel.Notes.ObserveCountChanged().Subscribe(delegate
		{
			DrawNotes();
		}).AddTo(this);
	}

	private void DrawNotes()
	{
		m_NotesContainer.Clear();
		m_NotesContainer.DrawEntries(base.ViewModel.Notes, NoteViewPrefab);
		base.gameObject.SetActive(base.ViewModel.Notes.Count > 0);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
