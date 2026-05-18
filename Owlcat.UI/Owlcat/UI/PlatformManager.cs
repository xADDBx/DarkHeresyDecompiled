using System.Collections.Generic;
using R3;

namespace Owlcat.UI;

public class PlatformManager
{
	private static PlatformManager s_Instance;

	public readonly ReactiveProperty<ConsoleType> ConsoleTypeProperty = new ReactiveProperty<ConsoleType>();

	public bool Switch2MouseModeActive;

	private readonly OverriddenPlatformProvider m_OverriddenPlatform = new OverriddenPlatformProvider();

	private readonly List<ConsoleTypeProvider> m_TypeProviders;

	public static PlatformManager Instance => s_Instance ?? (s_Instance = new PlatformManager());

	public ConsoleType Type => ConsoleTypeProperty.Value;

	public bool SwapButtonsForJapanese { get; private set; }

	private PlatformManager()
	{
		m_TypeProviders = new List<ConsoleTypeProvider>
		{
			m_OverriddenPlatform,
			new PlatformTypeProvider(),
			new DefaultTypeProvider()
		};
		UpdateConsoleType();
	}

	public void UpdateConsoleType()
	{
		foreach (ConsoleTypeProvider typeProvider in m_TypeProviders)
		{
			if (typeProvider.TryGetConsoleType(out var type))
			{
				ConsoleTypeProperty.Value = type;
				break;
			}
		}
		if (ConsoleTypeProperty.Value == ConsoleType.Switch)
		{
			SwapButtonsForJapanese = true;
		}
	}

	public void SetOverriddenConsoleType(ConsoleType type)
	{
		m_OverriddenPlatform.SetConsoleType(type);
		UpdateConsoleType();
	}

	public void AddProvider(ConsoleTypeProvider provider)
	{
		m_TypeProviders.Insert(m_TypeProviders.Count - 1, provider);
	}
}
