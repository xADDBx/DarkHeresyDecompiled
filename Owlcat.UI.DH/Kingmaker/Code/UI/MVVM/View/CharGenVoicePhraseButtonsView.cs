using System.Linq;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenVoicePhraseButtonsView : MonoBehaviour
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenVoicePhraseButtonView m_ItemPrefab;

	private CompositeDisposable m_Disposables = new CompositeDisposable();

	public void Bind(ObservableList<CharGenVoicePhraseButtonVM> buttons)
	{
		m_Disposables.Clear();
		DrawButtons(buttons);
		buttons.ObserveCountChanged().Subscribe(delegate
		{
			DrawButtons(buttons);
		}).AddTo(m_Disposables);
	}

	private void DrawButtons(ObservableList<CharGenVoicePhraseButtonVM> buttons)
	{
		m_WidgetList.DrawEntries(buttons.ToArray(), m_ItemPrefab);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_WidgetList.Container as RectTransform);
	}

	private void OnDestroy()
	{
		m_Disposables.Dispose();
	}
}
