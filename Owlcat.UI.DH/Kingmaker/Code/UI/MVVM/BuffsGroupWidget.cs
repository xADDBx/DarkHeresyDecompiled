using System;
using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BuffsGroupWidget : MonoBehaviour
{
	[Serializable]
	private struct BuffGroupSpriteSet
	{
		public Sprite Single;

		public Sprite Multiple;

		public void SetSprite(int effectsCount, Image image)
		{
			Sprite sprite = ((effectsCount > 1) ? Multiple : Single);
			image.sprite = sprite;
		}
	}

	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	[Space]
	[ShowIf("m_UseSpritesSet")]
	[SerializeField]
	private BuffGroupSpriteSet m_SpriteSet;

	[ShowIf("m_UseSpritesSet")]
	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private bool m_UseSpritesSet = true;

	public void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive);
	}

	public void SetCount(int count)
	{
		if (m_UseSpritesSet)
		{
			m_SpriteSet.SetSprite(count, m_Image);
		}
	}

	public void SetActiveLayer(string layerName)
	{
		m_Selectable.SetActiveLayer(layerName);
	}
}
