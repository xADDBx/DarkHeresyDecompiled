using Owlcat.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM;

public class FactionReputationView : View<FactionReputationVM>
{
	[Header("Elements")]
	[SerializeField]
	private WidgetList m_FactionsWidgetList;

	[Header("Views")]
	[FormerlySerializedAs("factionWidgetPrefab")]
	[SerializeField]
	private FactionReputationWidgetView m_FactionWidgetPrefab;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		m_FactionsWidgetList.DrawEntries(base.ViewModel.FactionWidgetVMs, m_FactionWidgetPrefab);
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_FactionsWidgetList.Clear();
	}
}
