using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.UI.Commands;

namespace Owlcat.UI.Navigation;

internal class FocusCommandProviderCollection : IDisposable
{
	private readonly FocusLayer m_FocusLayer;

	private readonly Dictionary<ICommandProvider, IDisposable> m_Providers = new Dictionary<ICommandProvider, IDisposable>();

	public IReadOnlyCollection<ICommandProvider> Providers => m_Providers.Keys;

	public FocusCommandProviderCollection(FocusLayer layer)
	{
		m_FocusLayer = layer;
	}

	public void Reset(IEnumerable<ICommandProvider> providers)
	{
		List<ICommandProvider> list = m_Providers.Keys.Except(providers).ToList();
		List<ICommandProvider> list2 = providers.Except(m_Providers.Keys).ToList();
		foreach (ICommandProvider item in list)
		{
			if (m_Providers.Remove(item, out var value))
			{
				value.Dispose();
			}
		}
		foreach (ICommandProvider item2 in list2)
		{
			if (!m_FocusLayer.Contains(item2))
			{
				m_Providers.Add(item2, m_FocusLayer.Add(item2));
			}
		}
	}

	public void Dispose()
	{
		foreach (IDisposable value in m_Providers.Values)
		{
			value.Dispose();
		}
		m_Providers.Clear();
	}
}
