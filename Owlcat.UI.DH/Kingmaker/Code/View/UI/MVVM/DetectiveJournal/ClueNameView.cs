using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueNameView : View<BlueprintClue>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_ClueName;

	[SerializeField]
	private RectTransform m_NameContainer;

	[Header("Values")]
	[SerializeField]
	private float m_OneLineWidth = 166f;

	[SerializeField]
	private float m_TwoLineWidth = 186f;

	protected override void OnBind()
	{
		m_ClueName.text = base.ViewModel.GetUIData().Name.Text;
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			float x = ((m_ClueName.textInfo.lineCount > 1) ? m_TwoLineWidth : m_OneLineWidth);
			m_NameContainer.sizeDelta = new Vector2(x, m_NameContainer.sizeDelta.y);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_NameContainer.sizeDelta = new Vector2(m_OneLineWidth, m_NameContainer.sizeDelta.y);
	}
}
