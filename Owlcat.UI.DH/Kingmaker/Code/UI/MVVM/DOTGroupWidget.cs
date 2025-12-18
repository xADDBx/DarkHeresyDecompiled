using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DOTGroupWidget : MonoBehaviour
{
	[SerializeField]
	private OwlcatMultiSelectable m_DOTEffectSingle;

	[SerializeField]
	private OwlcatMultiSelectable[] m_DOTEffectMultiples;

	public int MaxEffectsCount => m_DOTEffectMultiples.Length;

	public void SetActive(bool isActive)
	{
		base.gameObject.SetActive(isActive);
	}

	public void SetEffectsCount(int count)
	{
		if (count > 1)
		{
			SetMultiple(count);
		}
		else
		{
			SetSingle();
		}
	}

	public void SetActiveLayerSingle(string layerName)
	{
		m_DOTEffectSingle.SetActiveLayer(layerName);
		OwlcatMultiSelectable[] dOTEffectMultiples = m_DOTEffectMultiples;
		for (int i = 0; i < dOTEffectMultiples.Length; i++)
		{
			dOTEffectMultiples[i].gameObject.SetActive(value: false);
		}
	}

	public void SetActiveLayerMultiple(string layerName, int idx)
	{
		if (idx >= 0 && idx < m_DOTEffectMultiples.Length)
		{
			m_DOTEffectMultiples[idx].SetActiveLayer(layerName);
		}
	}

	private void SetSingle()
	{
		m_DOTEffectSingle.gameObject.SetActive(value: true);
		OwlcatMultiSelectable[] dOTEffectMultiples = m_DOTEffectMultiples;
		for (int i = 0; i < dOTEffectMultiples.Length; i++)
		{
			dOTEffectMultiples[i].gameObject.SetActive(value: false);
		}
	}

	private void SetMultiple(int count)
	{
		m_DOTEffectSingle.gameObject.SetActive(value: false);
		for (int i = 0; i < m_DOTEffectMultiples.Length; i++)
		{
			bool active = i < count;
			m_DOTEffectMultiples[i].gameObject.SetActive(active);
		}
	}
}
