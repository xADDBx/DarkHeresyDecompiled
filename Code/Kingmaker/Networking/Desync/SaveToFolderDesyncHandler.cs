using System;
using System.IO;
using Kingmaker.Networking.Hash;
using Kingmaker.Networking.Serialization;
using Kingmaker.Utility.Serialization;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Desync;

public class SaveToFolderDesyncHandler : IDesyncHandler
{
	private static JsonSerializer JsonSerializer => GameStateJsonSerializer.Serializer;

	public void RaiseDesync(HashableState data, DesyncMeta meta)
	{
		string contents = JsonSerializer.SerializeObject(data);
		string text = Path.Combine(ApplicationPaths.ReplayLogsDir, "Net", "Desync");
		Directory.CreateDirectory(text);
		File.WriteAllText(Path.Combine(text, $"{DateTime.Now.ToFileTime()}_{meta.Tick}.json"), contents);
	}
}
