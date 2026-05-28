using UnityEngine;

namespace Owlcat.UI.Navigation;

public class FloatNavigationComponent : MonoBehaviour
{
	[SerializeField]
	private bool m_Ignore;

	public bool Ignore => m_Ignore;
}
