using UnityEngine;

namespace Owlcat.UI.Navigation;

public interface IPointerProvider
{
	bool Enabled { get; set; }

	Vector2 Position { get; set; }
}
