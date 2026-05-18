using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickItemRestrictionView : BrickBaseView<BrickItemRestrictionVM>
{
	[Header("Elements")]
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private ItemRestrictionWidget ItemRestrictionWidgetPrefab;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	protected override void OnBind()
	{
		m_StateSelectable.SetActiveLayer(GetStateLayer());
		m_WidgetList.DrawEntries(base.ViewModel.GetFalseRestrictionStrings(), ItemRestrictionWidgetPrefab);
	}

	protected override void OnUnbind()
	{
		m_WidgetList.Clear();
	}

	private string GetStateLayer()
	{
		if (!base.ViewModel.CanEquipItem)
		{
			return "Default";
		}
		if (!base.ViewModel.HasFalseRestriction)
		{
			return "Positive";
		}
		return "Negative";
	}
}
