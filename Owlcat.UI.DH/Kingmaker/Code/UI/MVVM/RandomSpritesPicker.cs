using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class RandomSpritesPicker : RandomPickerBase
{
	[SerializeField]
	private List<Image> m_Images = new List<Image>();

	[SerializeField]
	private List<Sprite> m_Sprites = new List<Sprite>();

	public override void Randomize(string seed)
	{
		Reset();
		if (m_Images.Empty())
		{
			return;
		}
		if (m_Sprites.Empty())
		{
			int num = GetRandomIds(seed, 1, m_Images.Count).First();
			for (int i = 0; i < m_Images.Count; i++)
			{
				m_Images[i].gameObject.SetActive(i == num);
			}
		}
		else
		{
			List<int> randomIds = GetRandomIds(seed, m_Images.Count, m_Sprites.Count);
			for (int j = 0; j < randomIds.Count; j++)
			{
				m_Images[j].sprite = m_Sprites[randomIds[j]];
			}
		}
	}

	public override void Reset()
	{
	}
}
