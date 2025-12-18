using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UITexturePointerWatcher : MonoBehaviour
{
	[Header("Диапазон движения курсора (в пикселях)")]
	public float screenMinX;

	public float screenMaxX = Screen.width;

	[Header("Смещение текстуры (в UV координатах)")]
	public float shiftMinX = -0.2f;

	public float shiftMaxX = 0.2f;

	[Header("Смещение текстуры (в UV координатах)")]
	public float shiftUVMinX = -0.2f;

	public float shiftUVMaxX = 0.2f;

	private Material material;

	private Image image;

	public Vector2 originalMin;

	public Vector2 originalMax;

	public Vector2 originalAlphaUV;

	private void Start()
	{
		image = GetComponent<Image>();
		if (image != null && image.material != null)
		{
			material = Object.Instantiate(image.material);
			image.material = material;
			originalMin = material.GetVector("_Min");
			originalMax = material.GetVector("_Max");
			originalAlphaUV = material.GetVector("_AlphaUV");
		}
	}

	private void Update()
	{
		if (!(material == null))
		{
			float x = Input.mousePosition.x;
			float t = Mathf.InverseLerp(screenMinX, screenMaxX, x);
			float num = Mathf.Lerp(shiftMinX, shiftMaxX, t);
			float num2 = Mathf.Lerp(shiftUVMinX, shiftUVMaxX, t);
			material.SetVector("_Min", new Vector4(Mathf.Min(originalMin.x + num, 0f), originalMin.y, 0f, 0f));
			material.SetVector("_Max", new Vector4(Mathf.Max(originalMax.x + num, 1f), originalMax.y, 0f, 0f));
			material.SetVector("_AlphaUV", new Vector4(originalAlphaUV.x + num2, originalAlphaUV.y, 0f, 0f));
		}
	}
}
