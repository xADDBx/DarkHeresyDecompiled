namespace Owlcat.UI;

public interface IBindable
{
	void Unbind();
}
public interface IBindable<in T> : IBindable
{
	void Bind(T source);
}
