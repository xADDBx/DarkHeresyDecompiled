using Kingmaker.Blueprints.Root.Strings;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class PartyStatsOverviewView : View<PartyStatsOverviewVM>
{
	[SerializeField]
	private GameObject m_Root;

	[SerializeField]
	private TextMeshProUGUI m_HeaderText;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private PartyStatsOverviewCharacterView m_RowPrefab;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.IsActive.Subscribe(delegate(bool active)
		{
			if (m_Root != null)
			{
				m_Root.SetActive(active);
			}
		}).AddTo(this);
		if (m_HeaderText != null)
		{
			m_HeaderText.text = UIStrings.Instance.CharGen.CompanionsValues;
		}
		DrawEntries();
		base.ViewModel.Items.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntries();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		if (m_WidgetList != null)
		{
			m_WidgetList.Clear();
		}
		if (m_Root != null)
		{
			m_Root.SetActive(value: false);
		}
		base.OnUnbind();
	}

	private void DrawEntries()
	{
		if (!(m_WidgetList == null) && !(m_RowPrefab == null))
		{
			m_WidgetList.Clear();
			m_WidgetList.DrawEntries(base.ViewModel.Items, m_RowPrefab);
		}
	}
}
