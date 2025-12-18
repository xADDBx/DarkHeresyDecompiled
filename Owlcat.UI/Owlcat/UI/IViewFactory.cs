using System.Threading;
using System.Threading.Tasks;

namespace Owlcat.UI;

public interface IViewFactory
{
	Task<IBindable<T>> Retain<T>(T data, CancellationToken cancelationToken);

	void Release(IBindable view);
}
