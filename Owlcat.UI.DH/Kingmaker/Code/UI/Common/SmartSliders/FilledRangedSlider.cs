using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class FilledRangedSlider : MonoBehaviour
{
	[SerializeField]
	private Slider m_MainSlider;

	[SerializeField]
	private Image m_RangedImage;

	[SerializeField]
	private Color m_ColorStart = Color.white;

	[SerializeField]
	private Color m_ColorEnd = Color.gray;

	[SerializeField]
	private float m_BlinkDuration = 0.4f;

	private bool m_Horizontal;

	private bool m_Inverted;

	private Tweener m_BlinkTween;

	private static readonly int ID_Start = Shader.PropertyToID("_FillStart");

	private static readonly int ID_End = Shader.PropertyToID("_FillEnd");

	private static readonly int ID_Axis = Shader.PropertyToID("_Axis");

	private Material m_RuntimeMat;

	public void Initialize()
	{
		Slider.Direction direction = m_MainSlider.direction;
		m_Horizontal = direction == Slider.Direction.LeftToRight || direction == Slider.Direction.RightToLeft;
		m_Inverted = direction == Slider.Direction.RightToLeft || direction == Slider.Direction.TopToBottom;
		m_RangedImage.type = Image.Type.Filled;
		m_RangedImage.fillOrigin = (m_Inverted ? 1 : 0);
		m_RangedImage.fillAmount = 1f;
		m_RuntimeMat = new Material(m_RangedImage.material);
		m_RuntimeMat.name += " (Inst)";
		m_RangedImage.material = m_RuntimeMat;
		ResetRange();
	}

	public void SetMaxValue(float maxValue)
	{
		m_MainSlider.maxValue = maxValue;
	}

	public void SetRange(float from, float to, bool blink)
	{
		float from2 = Mathf.InverseLerp(m_MainSlider.minValue, m_MainSlider.maxValue, from);
		float to2 = Mathf.InverseLerp(m_MainSlider.minValue, m_MainSlider.maxValue, to);
		ApplyRange(from2, to2);
		m_BlinkTween?.Kill();
		if (blink && Mathf.Abs(from - to) > 0.01f)
		{
			m_BlinkTween = m_RangedImage.DOColor(m_ColorEnd, m_BlinkDuration).ChangeStartValue(m_ColorStart).SetLoops(-1, LoopType.Yoyo)
				.SetUpdate(isIndependentUpdate: true)
				.SetAutoKill(autoKillOnCompletion: true);
		}
	}

	public void Clear()
	{
		m_BlinkTween?.Kill();
		ResetRange();
	}

	private void ResetRange()
	{
		ApplyRange(0.5f, 0.5f);
	}

	private void ApplyRange(float from, float to)
	{
		from = Mathf.Clamp01(from);
		to = Mathf.Clamp01(to);
		if (m_Inverted)
		{
			from = 1f - from;
			to = 1f - to;
		}
		float start = Mathf.Min(from, to);
		float end = Mathf.Max(from, to);
		SetShaderRange(start, end);
	}

	private void SetShaderRange(float start, float end)
	{
		Sprite sprite = m_RangedImage.sprite;
		Texture2D texture = sprite.texture;
		Rect textureRect = sprite.textureRect;
		float num;
		float num2;
		if (m_Horizontal)
		{
			num = textureRect.x / (float)texture.width;
			num2 = textureRect.width / (float)texture.width;
		}
		else
		{
			num = textureRect.y / (float)texture.height;
			num2 = textureRect.height / (float)texture.height;
		}
		float value = num + start * num2;
		float value2 = num + end * num2;
		m_RuntimeMat.SetFloat(ID_Start, value);
		m_RuntimeMat.SetFloat(ID_End, value2);
		m_RuntimeMat.SetFloat(ID_Axis, m_Horizontal ? 0f : 1f);
	}

	protected virtual void OnDestroy()
	{
		m_BlinkTween?.Kill();
		if ((bool)m_RuntimeMat)
		{
			Object.Destroy(m_RuntimeMat);
		}
	}
}
