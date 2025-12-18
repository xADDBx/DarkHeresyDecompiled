using JetBrains.Annotations;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class JournalQuestObjectivePCView : JournalQuestObjectiveBaseView
{
	[Header("Navigation Group Objects")]
	[SerializeField]
	[UsedImplicitly]
	private WidgetList m_WidgetList;

	[SerializeField]
	[UsedImplicitly]
	private JournalQuestObjectiveAddendumPCView m_AddendumViewPrefab;

	[SerializeField]
	private Image m_HintPlace;

	protected override void OnBind()
	{
		SetupHeader();
		base.OnBind();
		DrawEntities();
		ObservableSubscribeExtensions.Subscribe(m_PinButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.DoPin();
		}).AddTo(this);
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Addendums.ToArray(), m_AddendumViewPrefab);
	}

	protected override void SetupState()
	{
		base.SetupState();
		if (m_HintPlace != null)
		{
			m_HintPlace.SetHint(GetHintText());
		}
	}
}
