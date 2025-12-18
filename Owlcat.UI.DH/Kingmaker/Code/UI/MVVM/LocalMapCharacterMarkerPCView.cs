using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapCharacterMarkerPCView : LocalMapMarkerPCView
{
	[SerializeField]
	private Image m_Portrait;

	[NonSerialized]
	public string CharacterName;

	[SerializeField]
	private GameObject m_SelectedActiveFrame;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.Portrait.Subscribe(delegate(Sprite value)
		{
			m_Portrait.sprite = value;
		}).AddTo(this);
		CharacterName = base.ViewModel.Description.CurrentValue;
		base.ViewModel.IsSelected.Subscribe(m_SelectedActiveFrame.SetActive).AddTo(this);
	}
}
