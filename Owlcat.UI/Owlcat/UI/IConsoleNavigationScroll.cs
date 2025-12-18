using UnityEngine;

namespace Owlcat.UI;

public interface IConsoleNavigationScroll
{
	bool CanFocusEntity(IConsoleEntity entity);

	void ScrollLeft();

	void ScrollRight();

	void ScrollUp();

	void ScrollDown();

	void ScrollInDirection(Vector2 direction);

	void ForceScrollToEntity(IConsoleEntity entity);
}
