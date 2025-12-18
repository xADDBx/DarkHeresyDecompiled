using Rewired;
using RewiredConsts;
using UnityEngine;

namespace Owlcat.UI;

public class ControlAxis : MonoBehaviour, IControl
{
	public ControlAxisEventHandler Handler;

	[ActionIdProperty(typeof(Action))]
	public int AxisId;
}
