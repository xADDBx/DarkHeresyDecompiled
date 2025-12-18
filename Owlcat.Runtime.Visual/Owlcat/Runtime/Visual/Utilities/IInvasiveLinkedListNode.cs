namespace Owlcat.Runtime.Visual.Utilities;

internal interface IInvasiveLinkedListNode<T>
{
	T Prev { get; set; }

	T Next { get; set; }
}
