using System;

namespace Owlcat.UI;

public class InputGroup : IDisposable
{
	private InputLayer m_Layer;

	public InputGroup(InputLayer layer, string name)
	{
		m_Layer = layer;
		m_Layer.SetCurrentGroup(name);
	}

	public void Dispose()
	{
		m_Layer.SetCurrentGroup("");
	}
}
