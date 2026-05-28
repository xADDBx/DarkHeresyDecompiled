using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Owlcat.UI.Navigation;

public interface INavigationGraph
{
	bool TryGet([NotNull] GameObject selected, Vector2 dir, out GameObject result);
}
