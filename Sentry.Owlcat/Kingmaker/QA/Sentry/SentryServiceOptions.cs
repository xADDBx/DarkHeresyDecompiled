using UnityEngine;

namespace Kingmaker.QA.Sentry;

public class SentryServiceOptions : ScriptableObject
{
	public int stringBuilderPoolCapacity;

	public int logQueueCapacity;

	public int addToLogQueueTimeoutMs;
}
