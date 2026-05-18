using System.Collections.Generic;

namespace Kingmaker.Visual.Animation.Decorators;

public interface IRuntimeDecorator
{
	UnitAnimationDecoratorObject DecoratorAsset { get; }

	IReadOnlyList<IDecoratorVisibilityRequest> Requests { get; }

	bool IsVisible { get; }

	void AddRequest(IDecoratorVisibilityRequest request);

	void RemoveRequest(IDecoratorVisibilityRequest request);
}
