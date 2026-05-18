using UnityEngine;

namespace Kingmaker.Visual.Animation;

public class ConstantPositionProvider : IVector3PositionProvider
{
	public Vector3 Position { get; }

	public ConstantPositionProvider(Vector3 position)
	{
		Position = position;
	}
}
