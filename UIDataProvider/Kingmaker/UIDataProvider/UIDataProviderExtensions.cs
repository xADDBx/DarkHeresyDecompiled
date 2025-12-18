using JetBrains.Annotations;

namespace Kingmaker.UIDataProvider;

public static class UIDataProviderExtensions
{
	[CanBeNull]
	public static IUIDataProvider SelectUIData([CanBeNull] this IUIDataProvider provider, UIDataType type)
	{
		if (provider == null)
		{
			return null;
		}
		switch (type)
		{
		case UIDataType.Name:
			if (!string.IsNullOrEmpty(provider.Name))
			{
				return provider;
			}
			break;
		case UIDataType.Description:
			if (!string.IsNullOrEmpty(provider.Description))
			{
				return provider;
			}
			break;
		case UIDataType.Icon:
			if (provider.Icon != null)
			{
				return provider;
			}
			break;
		case UIDataType.NameForAcronym:
			if (!string.IsNullOrEmpty(provider.NameForAcronym))
			{
				return provider;
			}
			break;
		}
		return null;
	}
}
