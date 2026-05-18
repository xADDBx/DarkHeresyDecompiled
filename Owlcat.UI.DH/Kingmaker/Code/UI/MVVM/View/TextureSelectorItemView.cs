using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TextureSelectorItemView : SelectionGroupEntityView<TextureSelectorItemVM>
{
	[SerializeField]
	protected Image m_Image;

	[SerializeField]
	protected TextMeshProUGUI m_NumberText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Texture.Subscribe(SetTexture));
		if (m_NumberText != null)
		{
			TextMeshProUGUI numberText = m_NumberText;
			int number = base.ViewModel.Number;
			numberText.text = number.ToString();
		}
	}

	protected override void OnClick()
	{
		if (UtilityNet.IsControlMainCharacter())
		{
			base.OnClick();
		}
	}

	private void SetTexture(Texture2D texture)
	{
		if (texture != null)
		{
			m_Image.sprite = GetSprite(texture);
			return;
		}
		DefaultImageType type = ((base.ViewModel != null && base.ViewModel.IsEmpty) ? DefaultImageType.AppearanceEmpty : DefaultImageType.Appearance);
		m_Image.sprite = UIUtilityImage.GetDefault(type);
	}

	protected virtual Sprite GetSprite(Texture2D texture)
	{
		Rect rect = new Rect(0f, 0f, texture.width, texture.height);
		return Sprite.Create(texture, rect, Vector2.zero);
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		if (value)
		{
			ButtonsSounds.Instance.Default.Hover.Play();
		}
		m_Button.CanConfirm = !value;
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		if (value)
		{
			ButtonsSounds.Instance.Default.Hover.Play();
			base.ViewModel.DoFocusMe();
		}
	}
}
