namespace Owlcat.Runtime.Visual.Experimental.Collections;

public interface IPredicate<T, U> where T : unmanaged where U : unmanaged
{
	bool Invoke(in T a, in U b);
}
public interface IPredicate<T> where T : unmanaged
{
	bool Invoke(in T a);
}
