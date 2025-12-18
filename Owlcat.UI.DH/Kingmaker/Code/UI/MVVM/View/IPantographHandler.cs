using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.View;

public interface IPantographHandler : ISubscriber
{
	void Bind(PantographConfig config);

	void Unbind();

	void SetFocus(bool focused);
}
