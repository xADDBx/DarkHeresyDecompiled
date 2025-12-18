using System;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveStudyVM : ViewModel
{
	public readonly StudyGroup StudyGroup;

	private readonly BlueprintClue m_BlueprintClue;

	private readonly Action m_OnStudyClick;

	public DetectiveStudyVM(StudyGroup study, Action onStudyClick)
	{
		StudyGroup = study;
		m_OnStudyClick = onStudyClick;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void OnStudyClick()
	{
		if (m_OnStudyClick != null)
		{
			m_OnStudyClick?.Invoke();
			UISounds.Instance.Play(UISounds.Instance.Sounds.DetectiveSystem.ClueResearch, isButton: true);
		}
	}
}
