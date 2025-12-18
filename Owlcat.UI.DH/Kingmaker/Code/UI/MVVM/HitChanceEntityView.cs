using System;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class HitChanceEntityView : View<HitChanceEntityVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TextMeshProUGUI m_Index;

	protected override void OnBind()
	{
		double num = Math.Round(base.ViewModel.Chance, 0);
		m_Text.text = (base.ViewModel.IsLast ? $"{num}%" : $"{num}/");
	}

	public void SetColor(Color color)
	{
		m_Text.color = color;
	}
}
