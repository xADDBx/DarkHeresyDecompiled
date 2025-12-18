using System.IO;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Networking;
using Kingmaker.Utility.Serialization;
using OwlPack.Runtime;

namespace Kingmaker.Utility;

public static class SaveConverter
{
	private static void Convert<T>(ZipSaver oldSaver, ZipSaver newSaver, SaveInfo.SaveFormat newFormat, string fileName)
	{
		string text = fileName;
		string text2 = fileName.Replace((newFormat == SaveInfo.SaveFormat.JSON) ? ".owl" : ".json", (newFormat == SaveInfo.SaveFormat.JSON) ? ".json" : ".owl");
		if (newFormat == SaveInfo.SaveFormat.JSON)
		{
			if (!text.EndsWith(".owl"))
			{
				text += ".owl";
			}
			if (text2.EndsWith(".json"))
			{
				text2 = text2.Substring(0, text2.Length - ".json".Length);
			}
		}
		else if (!text2.EndsWith(".owl"))
		{
			text2 += ".owl";
		}
		if (newFormat == SaveInfo.SaveFormat.JSON)
		{
			T value = StandardSerializer.DeserializeFromBinary<T>(new BinaryInputArchive(new MemoryStream(oldSaver.ReadBytes(text))));
			JsonOutputArchive<ArrayMemoryBufferWriter> jsonOutputArchive = StandardSerializer.SerializeAnyToJson(ref value);
			MemoryStream memoryStream = new MemoryStream();
			jsonOutputArchive.Write(memoryStream);
			memoryStream.Position = 0L;
			newSaver.SaveBytes(text2, memoryStream.ToArray());
		}
		else
		{
			T value2 = StandardSerializer.DeserializeFromJson<T>(new JsonInputArchive(new MemoryStream(oldSaver.ReadBytes(text))));
			BinaryOutputArchive<ArrayMemoryBufferWriter> binaryOutputArchive = StandardSerializer.SerializeAnyToBinary(ref value2);
			MemoryStream memoryStream2 = new MemoryStream();
			binaryOutputArchive.Write(memoryStream2);
			memoryStream2.Position = 0L;
			newSaver.SaveBytes(text2, memoryStream2.ToArray());
		}
	}

	public static bool ConvertSave(string savePath)
	{
		if (!File.Exists(savePath))
		{
			PFLog.Default.Error("convert-save: save file at \"" + savePath + "\" not found");
			return false;
		}
		ZipSaver zipSaver = new ZipSaver(savePath, ISaver.Mode.Read);
		string text = zipSaver.ReadHeader();
		if (string.IsNullOrEmpty(text))
		{
			PFLog.Default.Error("convert-save: save file at \"" + savePath + "\" does not contain readable header.json");
			return false;
		}
		SaveInfo saveInfo = SaveSystemJsonSerializer.Serializer.DeserializeObject<SaveInfo>(text);
		SaveInfo.SaveFormat format = saveInfo.Format;
		SaveInfo.SaveFormat saveFormat2 = (saveInfo.Format = ((saveInfo.Format != SaveInfo.SaveFormat.OwlPack) ? SaveInfo.SaveFormat.OwlPack : SaveInfo.SaveFormat.JSON));
		string fileName = zipSaver.FileName;
		string directoryName = Path.GetDirectoryName(fileName);
		fileName = Path.GetFileName(fileName);
		fileName = fileName.Replace("_json.", ".");
		fileName = fileName.Replace("_bin.", ".");
		fileName = ((saveFormat2 != SaveInfo.SaveFormat.OwlPack) ? (Path.GetFileNameWithoutExtension(fileName) + "_json.zks") : (Path.GetFileNameWithoutExtension(fileName) + "_owl.zks"));
		ZipSaver zipSaver2 = zipSaver.CloneWithNewName(Path.Combine(directoryName, fileName)) as ZipSaver;
		zipSaver2.SetMode(ISaver.Mode.Write);
		new CommandNetManager();
		string json = SaveSystemJsonSerializer.Serializer.SerializeObject(saveInfo);
		zipSaver2.SaveJson("header", json);
		Convert<SceneEntitiesState>(zipSaver, zipSaver2, saveFormat2, SaveConsts.PlayerFileName(format));
		Convert<SceneEntitiesState>(zipSaver, zipSaver2, saveFormat2, SaveConsts.PartyFileName(format));
		foreach (string allFile in zipSaver.GetAllFiles())
		{
			if (format == SaveInfo.SaveFormat.OwlPack)
			{
				if (allFile == "header.json")
				{
					continue;
				}
				if (Path.GetExtension(allFile) != ".owl")
				{
					zipSaver2.SaveBytes(allFile, zipSaver.ReadBytes(allFile));
					continue;
				}
				goto IL_022b;
			}
			if (allFile == SaveConsts.PlayerFileName(SaveInfo.SaveFormat.JSON) || allFile == SaveConsts.PartyFileName(SaveInfo.SaveFormat.JSON) || allFile == "header.json")
			{
				continue;
			}
			if (!(Path.GetExtension(allFile) != ".json"))
			{
				switch (allFile)
				{
				case "coop.json":
				case "statistic.json":
				case "settings.json":
					break;
				default:
					goto IL_022b;
				}
			}
			zipSaver2.SaveBytes(allFile, zipSaver.ReadBytes(allFile));
			continue;
			IL_022b:
			if (Path.GetFileNameWithoutExtension(allFile).Length == 32)
			{
				Convert<AreaPersistentState>(zipSaver, zipSaver2, saveFormat2, allFile);
			}
			else
			{
				Convert<SceneEntitiesState>(zipSaver, zipSaver2, saveFormat2, allFile);
			}
		}
		zipSaver2.Save();
		zipSaver2.Dispose();
		zipSaver.Dispose();
		PFLog.Default.Log($"convert-save: converted \"{savePath}\" from {format} to {saveFormat2} and saved as {fileName}");
		return true;
	}
}
