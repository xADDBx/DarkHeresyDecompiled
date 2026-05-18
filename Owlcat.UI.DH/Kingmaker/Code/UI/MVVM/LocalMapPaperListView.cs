using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapPaperListView : View<LocalMapPaperListVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_PaperTitle;

	[SerializeField]
	private TMP_Text m_SubtitleText;

	[SerializeField]
	private WidgetList m_PaperList;

	[FormerlySerializedAs("m_PaperEntityPrefab")]
	[Header("Views")]
	[SerializeField]
	private LocalTransitionMapEntityView PaperEntityPrefab;

	protected override void OnBind()
	{
		m_PaperTitle.text = base.ViewModel.Name.Text;
		m_SubtitleText.text = UIStrings.Instance.Transition.TravelToText.Text;
		m_PaperList.DrawEntries(base.ViewModel.Entities, PaperEntityPrefab).AddTo(this);
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
