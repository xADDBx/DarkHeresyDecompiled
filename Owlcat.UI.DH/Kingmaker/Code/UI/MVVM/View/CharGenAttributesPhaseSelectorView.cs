using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAttributesPhaseSelectorView : View<SelectionGroupRadioVM<CharGenAttributesItemVM>>
{
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private CharGenAttributesPhaseSelectorItemView m_ItemViewPrefab;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	protected override void OnBind()
	{
		DrawEntities();
		base.ViewModel.EntitiesCollection.ObserveCountChanged().Subscribe(delegate
		{
			DrawEntities();
		}).AddTo(this);
		m_Label.text = UIStrings.Instance.CharGen.CharacterSkillPoints;
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection.ToArray(), m_ItemViewPrefab, unused: true).AddTo(this);
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_WidgetList.GetNavigationEntities();
	}

	public IConsoleNavigationEntity GetSelectedEntity()
	{
		return m_WidgetList.Entries.Cast<CharGenAttributesPhaseSelectorItemView>().FirstOrDefault((CharGenAttributesPhaseSelectorItemView i) => i.GetViewModel()?.IsSelected.Value ?? false);
	}
}
