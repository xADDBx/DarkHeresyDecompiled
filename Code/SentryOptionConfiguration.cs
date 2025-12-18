using Sentry;
using Sentry.Unity;
using UnityEngine;

public class SentryOptionConfiguration : SentryOptionsConfiguration
{
	[SerializeField]
	private bool _filterErrors;

	public override void Configure(SentryUnityOptions options)
	{
		options.SetBeforeSend(delegate(SentryEvent sentryEvent)
		{
			if (_filterErrors && sentryEvent.Exception == null)
			{
				return (SentryEvent?)null;
			}
			sentryEvent.ServerName = null;
			sentryEvent.Contexts.Device.Name = null;
			return sentryEvent;
		});
	}
}
