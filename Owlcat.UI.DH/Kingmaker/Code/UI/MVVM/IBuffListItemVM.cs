using System;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public interface IBuffListItemVM : IDisposable
{
	string Name { get; }

	string SourceName { get; }

	string Stack { get; }

	Sprite Icon { get; }

	TooltipBaseTemplate Tooltip { get; }

	ReadOnlyReactiveProperty<string> Duration { get; }
}
