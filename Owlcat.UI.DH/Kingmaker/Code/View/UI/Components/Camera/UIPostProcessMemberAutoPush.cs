using UnityEngine;

namespace Kingmaker.Code.View.UI.Components.Camera;

[RequireComponent(typeof(UIPostProcessMember))]
public class UIPostProcessMemberAutoPush : MonoBehaviour
{
	public void Start()
	{
		GetComponent<UIPostProcessMember>().Bind();
	}
}
