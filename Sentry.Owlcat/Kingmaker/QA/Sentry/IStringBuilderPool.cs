namespace Kingmaker.QA.Sentry;

internal interface IStringBuilderPool
{
	PooledStringBuilder Rent();
}
