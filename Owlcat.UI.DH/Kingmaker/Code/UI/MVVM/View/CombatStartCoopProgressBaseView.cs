using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CombatStartCoopProgressBaseView : View<CombatStartCoopProgressVM>
{
	[SerializeField]
	private List<CombatStartCoopProgressBaseItemView> m_Items;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.ViewModel.IsActive.Subscribe(base.gameObject.SetActive).AddTo(this);
		base.ViewModel.TotalProgress.Subscribe(delegate(int value)
		{
			for (int j = 0; j < m_Items.Count; j++)
			{
				m_Items[j].gameObject.SetActive(j < value);
			}
		}).AddTo(this);
		base.ViewModel.CurrentProgress.Subscribe(delegate(int value)
		{
			for (int i = 0; i < m_Items.Count; i++)
			{
				m_Items[i].SetActive(i < value);
			}
		}).AddTo(this);
	}
}
