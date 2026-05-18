using UnityEngine;

namespace Owlcat.UI;

public interface IHintIconProvider
{
	bool TryGetIcon(string binding, out Sprite sprite);
}
