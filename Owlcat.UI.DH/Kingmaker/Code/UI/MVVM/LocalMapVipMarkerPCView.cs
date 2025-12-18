using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapVipMarkerPCView : LocalMapMarkerPCView
{
	[SerializeField]
	private Image m_Mark;

	[SerializeField]
	private Sprite m_MapObjectSprite;

	[SerializeField]
	private Sprite m_NpcSprite;

	protected override void OnBind()
	{
		base.OnBind();
		m_Mark.sprite = (base.ViewModel.IsMapObject.CurrentValue ? m_MapObjectSprite : m_NpcSprite);
	}
}
