using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class FilledDelayedSlider : MonoBehaviour
{
	[SerializeField]
	private Slider m_MainSlider;

	[SerializeField]
	private Image m_RangedImage;

	[SerializeField]
	private Color m_PositiveDeltaColor = Color.green;

	[SerializeField]
	private Color m_NegativeDeltaColor = Color.red;

	[SerializeField]
	private float m_DeltaShowDelay = 2f;

	[SerializeField]
	private float m_DeltaMoveTime = 1f;

	private static readonly int ID_Start = Shader.PropertyToID("_FillStart");

	private static readonly int ID_End = Shader.PropertyToID("_FillEnd");

	private static readonly int ID_Axis = Shader.PropertyToID("_Axis");

	private bool m_Horizontal;

	private bool m_Inverted;

	private float m_TempValue;

	private Tweener m_DeltaTween;

	private Material m_RuntimeMat;

	public void Initialize()
	{
		Slider.Direction direction = m_MainSlider.direction;
		m_Horizontal = direction == Slider.Direction.LeftToRight || direction == Slider.Direction.RightToLeft;
		m_Inverted = direction == Slider.Direction.LeftToRight || direction == Slider.Direction.BottomToTop;
		m_RangedImage.type = Image.Type.Filled;
		m_RangedImage.fillOrigin = (m_Inverted ? 1 : 0);
		m_RangedImage.fillAmount = 1f;
		m_RuntimeMat = new Material(m_RangedImage.material);
		m_RuntimeMat.name += " (Inst)";
		m_RangedImage.material = m_RuntimeMat;
		HideRangeImmediate();
	}

	public void SetValue(float value, bool showDelta = true, bool noDelay = false)
	{
		if (!(Mathf.Abs(m_MainSlider.value - value) < 0.01f))
		{
			if (!showDelta)
			{
				m_MainSlider.value = value;
				HideRangeImmediate();
			}
			else
			{
				AnimateValue(value, noDelay);
			}
		}
	}

	private void AnimateValue(float targetValue, bool noDelay)
	{
		m_TempValue = m_MainSlider.normalizedValue;
		m_MainSlider.value = targetValue;
		float newNorm = m_MainSlider.normalizedValue;
		m_RangedImage.color = ((newNorm > m_TempValue) ? m_PositiveDeltaColor : m_NegativeDeltaColor);
		m_DeltaTween?.Kill();
		m_DeltaTween = DOTween.To(() => m_TempValue, delegate(float v)
		{
			m_TempValue = v;
			SetShaderRange(m_TempValue, newNorm);
		}, newNorm, m_DeltaMoveTime).ChangeStartValue(m_TempValue).SetDelay(noDelay ? 0f : m_DeltaShowDelay)
			.SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: true);
	}

	private void HideRangeImmediate()
	{
		SetShaderRange(0f, 0f);
	}

	private void SetShaderRange(float normA, float normB)
	{
		normA = Mathf.Clamp01(normA);
		normB = Mathf.Clamp01(normB);
		if (m_Inverted)
		{
			normA = 1f - normA;
			normB = 1f - normB;
		}
		float num = Mathf.Min(normA, normB);
		float num2 = Mathf.Max(normA, normB);
		Sprite sprite = m_RangedImage.sprite;
		Texture2D texture = sprite.texture;
		Rect textureRect = sprite.textureRect;
		float num3;
		float num4;
		if (m_Horizontal)
		{
			num3 = textureRect.x / (float)texture.width;
			num4 = textureRect.width / (float)texture.width;
		}
		else
		{
			num3 = textureRect.y / (float)texture.height;
			num4 = textureRect.height / (float)texture.height;
		}
		float value = num3 + num * num4;
		float value2 = num3 + num2 * num4;
		m_RuntimeMat.SetFloat(ID_Start, value);
		m_RuntimeMat.SetFloat(ID_End, value2);
		m_RuntimeMat.SetFloat(ID_Axis, m_Horizontal ? 0f : 1f);
	}

	private void OnDestroy()
	{
		m_DeltaTween?.Kill();
		if ((bool)m_RuntimeMat)
		{
			Object.Destroy(m_RuntimeMat);
		}
	}
}
