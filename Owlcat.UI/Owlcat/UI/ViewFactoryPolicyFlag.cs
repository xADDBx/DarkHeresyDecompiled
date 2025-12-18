namespace Owlcat.UI;

public enum ViewFactoryPolicyFlag
{
	None = 0,
	DontInstantiateAsync = 1,
	DontPool = 2,
	DontDismiss = 4,
	DontReparent = 8
}
