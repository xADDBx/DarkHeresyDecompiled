using Rewired;
using UnityEngine;

namespace Owlcat.UI.Commands.Rewired;

internal class Axis2D
{
	private struct Axis1D
	{
		private readonly int ActionId;

		private readonly string Binding;

		private bool Changed;

		public float Value;

		public Axis1D(int actionId)
		{
			ActionId = actionId;
			Binding = "gamepad:" + RewiredUtility.GetName(actionId);
			Value = 0f;
			Changed = false;
		}

		public void OnAxisActiveOrJustInactive(InputActionEventData inputActionEventData)
		{
			if (inputActionEventData.actionId == ActionId)
			{
				Changed = true;
				Value = inputActionEventData.GetAxis();
			}
		}

		public bool Update(CommandLayerStack stack)
		{
			if (Changed)
			{
				Changed = false;
				stack.Consume(RewiredInputEvent.Create(Binding, Value, Vector2.zero));
				return true;
			}
			return false;
		}
	}

	private readonly string m_Name;

	private Axis1D m_X;

	private Axis1D m_Y;

	public Axis2D(string name, int actionIdX, int actionIdY)
	{
		m_Name = "gamepad:" + name;
		m_X = new Axis1D(actionIdX);
		m_Y = new Axis1D(actionIdY);
	}

	public void OnAxisActiveOrJustInactive(InputActionEventData inputActionEventData)
	{
		m_X.OnAxisActiveOrJustInactive(inputActionEventData);
		m_Y.OnAxisActiveOrJustInactive(inputActionEventData);
	}

	public void Update(CommandLayerStack stack)
	{
		if (false | m_X.Update(stack) | m_Y.Update(stack))
		{
			stack.Consume(RewiredInputEvent.Create(m_Name, 0f, new Vector2(m_X.Value, m_Y.Value)));
		}
	}
}
