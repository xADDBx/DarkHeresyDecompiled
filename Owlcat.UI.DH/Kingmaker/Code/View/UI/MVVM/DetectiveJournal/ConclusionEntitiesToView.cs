using System.Collections.Generic;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ConclusionEntitiesToView : View<List<BlueprintConclusion.Source>>
{
	[Header("Views")]
	[SerializeField]
	private TypeToPrefabSelector<ConclusionEntityView> m_CluePrefabs;

	[SerializeField]
	private ConclusionEntity m_ConclusionEntityPrefab;

	[Header("Elements")]
	[SerializeField]
	private RectTransform m_Container;

	private readonly List<GameObject> m_Views = new List<GameObject>();

	public List<LineDirectionData> LineDirections { get; } = new List<LineDirectionData>();


	protected override void OnBind()
	{
		foreach (BlueprintConclusion.Source item in base.ViewModel)
		{
			BlueprintCaseItem blueprint = item.Item2.Blueprint;
			if (!(blueprint is BlueprintClue blueprintClue))
			{
				if (!(blueprint is BlueprintClueAddendum blueprintClueAddendum))
				{
					if (blueprint is BlueprintConclusion source)
					{
						ConclusionEntity widget = WidgetFactory.GetWidget(m_ConclusionEntityPrefab);
						widget.Bind(source);
						widget.transform.SetParent(m_Container, worldPositionStays: false);
						m_Views.Add(widget.gameObject);
						LineDirections.Add(widget.LineFrom);
					}
				}
				else
				{
					ConclusionEntityView widget2 = WidgetFactory.GetWidget(m_CluePrefabs.GetEntity(blueprintClueAddendum.ParentClue.Blueprint.UIClueType));
					widget2.Bind(new ClueConclusionEntityVM(blueprintClueAddendum));
					widget2.transform.SetParent(m_Container, worldPositionStays: false);
					m_Views.Add(widget2.gameObject);
					LineDirections.Add(widget2.LineFrom);
				}
			}
			else
			{
				ConclusionEntityView widget3 = WidgetFactory.GetWidget(m_CluePrefabs.GetEntity(blueprintClue.UIClueType));
				widget3.Bind(new ClueConclusionEntityVM(blueprintClue));
				widget3.transform.SetParent(m_Container, worldPositionStays: false);
				m_Views.Add(widget3.gameObject);
				LineDirections.Add(widget3.LineFrom);
			}
		}
	}

	protected override void OnUnbind()
	{
		m_Views.ForEach(Object.Destroy);
		LineDirections.Clear();
	}
}
