using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionEntityFromView : View<BlueprintCaseItem>
{
	[Header("Views")]
	[SerializeField]
	private TypeToPrefabSelector<ConclusionEntityView> m_CluePrefabs;

	[SerializeField]
	private ConclusionEntity m_ConclusionEntityPrefab;

	[Header("Elements")]
	[SerializeField]
	private RectTransform m_Container;

	private GameObject m_View;

	public LineDirectionData LineFrom { get; private set; }

	protected override void OnBind()
	{
		BlueprintCaseItem viewModel = base.ViewModel;
		if (!(viewModel is BlueprintClue blueprintClue))
		{
			if (!(viewModel is BlueprintClueAddendum blueprintClueAddendum))
			{
				if (viewModel is BlueprintConclusion source)
				{
					ConclusionEntity widget = WidgetFactory.GetWidget(m_ConclusionEntityPrefab);
					widget.Bind(source);
					widget.transform.SetParent(m_Container, worldPositionStays: false);
					m_View = widget.gameObject;
					LineFrom = widget.LineFrom;
					widget.SetHighlightAlwaysDisabled();
				}
			}
			else
			{
				ConclusionEntityView widget2 = WidgetFactory.GetWidget(m_CluePrefabs.GetEntity(blueprintClueAddendum.ParentClue.Blueprint.UIClueType));
				widget2.Bind(new ClueConclusionEntityVM(blueprintClueAddendum));
				widget2.transform.SetParent(m_Container, worldPositionStays: false);
				m_View = widget2.gameObject;
				widget2.SetHighlightAlwaysDisabled();
				LineFrom = widget2.LineFrom;
			}
		}
		else
		{
			ConclusionEntityView widget3 = WidgetFactory.GetWidget(m_CluePrefabs.GetEntity(blueprintClue.UIClueType));
			widget3.Bind(new ClueConclusionEntityVM(blueprintClue));
			widget3.transform.SetParent(m_Container, worldPositionStays: false);
			m_View = widget3.gameObject;
			widget3.SetHighlightAlwaysDisabled();
			LineFrom = widget3.LineFrom;
		}
	}

	protected override void OnUnbind()
	{
		Object.Destroy(m_View);
	}
}
