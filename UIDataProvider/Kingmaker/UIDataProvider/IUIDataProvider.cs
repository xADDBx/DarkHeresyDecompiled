using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.UIDataProvider;

public interface IUIDataProvider
{
	[NotNull]
	string Name { get; }

	[NotNull]
	string Description { get; }

	[CanBeNull]
	Sprite Icon { get; }

	[CanBeNull]
	string NameForAcronym { get; }
}
