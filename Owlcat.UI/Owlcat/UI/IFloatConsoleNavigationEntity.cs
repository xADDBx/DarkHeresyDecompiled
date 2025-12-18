using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.UI;

public interface IFloatConsoleNavigationEntity : IConsoleNavigationEntity, IConsoleEntity
{
	Vector2 GetPosition();

	List<IFloatConsoleNavigationEntity> GetNeighbours();
}
