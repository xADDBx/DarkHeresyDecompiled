using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class LocalMapUnitMarkerPCView : LocalMapMarkerPCView
{
	[SerializeField]
	private Image m_Mark;

	[SerializeField]
	private Sprite m_EnemySprite;

	[SerializeField]
	private Sprite m_NpcSprite;

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.IsEnemy.Subscribe(delegate(bool value)
		{
			m_Mark.sprite = (value ? m_EnemySprite : m_NpcSprite);
		}).AddTo(this);
	}
}
