using UnityEngine;

namespace Owlcat.UI;

public interface IRewiredCursorController
{
	bool Enabled { get; set; }

	GameObject Cursor { get; }
}
