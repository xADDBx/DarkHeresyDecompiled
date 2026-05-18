namespace Owlcat.Runtime.Visual.VirtualTexture;

internal class VTFeedbackRotation
{
	private int m_CallsThisFrame;

	private int m_CallsLastFrame;

	private int m_RotatingIndex;

	public void BeginFrame()
	{
		if (m_CallsThisFrame > 0)
		{
			m_CallsLastFrame = m_CallsThisFrame;
		}
		m_CallsThisFrame = 0;
		if (m_CallsLastFrame > 0)
		{
			m_RotatingIndex = (m_RotatingIndex + 1) % m_CallsLastFrame;
		}
	}

	public bool ShouldDispatch()
	{
		bool result = m_CallsThisFrame == m_RotatingIndex;
		m_CallsThisFrame++;
		return result;
	}
}
