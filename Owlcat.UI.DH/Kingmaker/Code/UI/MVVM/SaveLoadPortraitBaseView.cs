using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SaveLoadPortraitBaseView : View<SaveLoadPortraitVM>
{
	[SerializeField]
	private Image m_Portrait;

	[Header("Character Rank")]
	[SerializeField]
	private GameObject m_RankGameObject;

	[SerializeField]
	private TextMeshProUGUI m_RankLabel;

	protected override void OnBind()
	{
		base.gameObject.Or(null)?.SetActive(value: true);
		m_Portrait.sprite = base.ViewModel.Portrait;
		SetRank();
	}

	protected override void OnUnbind()
	{
		if (this != null && base.gameObject != null)
		{
			base.gameObject.Or(null)?.SetActive(value: false);
		}
	}

	private void SetRank()
	{
		if (!(m_RankGameObject == null) && !(m_RankLabel == null))
		{
			m_RankGameObject.Or(null)?.SetActive(!string.IsNullOrEmpty(base.ViewModel.Rank));
			m_RankLabel.text = base.ViewModel.Rank;
		}
	}
}
