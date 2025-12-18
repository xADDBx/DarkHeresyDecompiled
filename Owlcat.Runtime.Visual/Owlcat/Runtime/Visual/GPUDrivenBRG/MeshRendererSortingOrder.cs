using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class MeshRendererSortingOrder : MonoBehaviour
{
	[SerializeField]
	private int m_SortingOrder;

	public int SortingOrder
	{
		get
		{
			return m_SortingOrder;
		}
		set
		{
			if (value != m_SortingOrder)
			{
				m_SortingOrder = value;
				UpdateSortingOrder();
			}
		}
	}

	private void Awake()
	{
		UpdateSortingOrder();
	}

	private void OnValidate()
	{
		UpdateSortingOrder();
	}

	private void UpdateSortingOrder()
	{
		if (TryGetComponent<MeshRenderer>(out var component) && component.sortingOrder != SortingOrder)
		{
			component.sortingOrder = SortingOrder;
		}
	}
}
