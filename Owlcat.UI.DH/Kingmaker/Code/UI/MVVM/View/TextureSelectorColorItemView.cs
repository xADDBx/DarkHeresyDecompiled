using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorColorItemView : TextureSelectorItemView
{
	protected override Sprite GetSprite(Texture2D texture)
	{
		Rect rect = new Rect(texture.width / 2, 0f, 1f, texture.height);
		return Sprite.Create(texture, rect, Vector2.zero);
	}
}
