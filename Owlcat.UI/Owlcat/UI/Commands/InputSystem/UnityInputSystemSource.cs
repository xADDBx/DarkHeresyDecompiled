namespace Owlcat.UI.Commands.InputSystem;

public class UnityInputSystemSource
{
	private readonly CommandLayerStack m_Stack;

	public static bool IsSupported()
	{
		return false;
	}

	public UnityInputSystemSource(CommandLayerStack stack)
	{
		m_Stack = stack;
	}

	public void Update()
	{
	}
}
