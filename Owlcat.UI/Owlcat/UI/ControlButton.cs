using Rewired;
using RewiredConsts;
using UnityEngine;

namespace Owlcat.UI;

public class ControlButton : MonoBehaviour, IControl
{
	public ControlButtonEventHandler Handler;

	[ActionIdProperty(typeof(Action))]
	public int[] ActionIds;
}
