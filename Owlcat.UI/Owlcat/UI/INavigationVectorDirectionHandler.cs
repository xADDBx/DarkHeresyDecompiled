using UnityEngine;

namespace Owlcat.UI;

public interface INavigationVectorDirectionHandler : IConsoleEntity
{
	bool HandleVector(Vector2 vector);
}
