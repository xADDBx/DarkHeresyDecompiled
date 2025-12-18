using System;
using System.Collections.Generic;

namespace Owlcat;

public class CableConnectorService
{
	private static CableConnectorService _instance;

	private HashSet<SplinePointsLocator> _locators = new HashSet<SplinePointsLocator>();

	public Action<SplinePointsLocator> NewLocatorAdded;

	public static CableConnectorService Instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}
			_instance = new CableConnectorService();
			return _instance;
		}
	}

	public void Register(SplinePointsLocator locator)
	{
		_locators.Add(locator);
		NewLocatorAdded?.Invoke(locator);
	}
}
