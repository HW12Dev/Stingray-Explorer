using System;
using System.Runtime.CompilerServices;

namespace StingrayExplorer
{
	class Program
	{
		//static string testfile = @"X:\SteamLibrary\steamapps\common\Helldivers 2\data\e4d793e60c63aa56";
		static string testfile = @"X:\SteamLibrary\steamapps\common\Helldivers 2\data\2e24ba9dd702da5c";
		static string helldivers = @"X:\SteamLibrary\steamapps\common\Helldivers 2\data\";

		static string outdir = @"X:\Projects\StingrayExplorer\gameextract\";

		static void Main(string[] args)
		{
			Hash64.LoadHashlist("hashlist.txt");

			//Console.WriteLine(new Hash64("wwise_bank").Hash);
			
			
			//Console.WriteLine(0x1a0e932d2c7e46a1);
			//return;
			//Console.WriteLine(new Hash64("hash_lookup").Hash);
			//return;
			//Console.WriteLine(new Hash64("content/audio").Hash.ToString("X").ToLower());
			
			
			//Console.WriteLine(ImportFuncs.stingray_murmur_64("content/ui").ToString("X").ToLower());

			//Console.WriteLine(ImportFuncs.stingray_murmur_64("bik"));
			//Console.WriteLine(ImportFuncs.stingray_murmur_64("bk2"));


			List<StingrayHeaderFile> headers = new List<StingrayHeaderFile>();
			headers.Add(new StingrayHeaderFile().Read(new BinaryReader(File.OpenRead(testfile)), testfile));
			/*foreach (var file in Directory.GetFiles(helldivers))
			{
				if (file.Contains(".")) continue;

				//Console.WriteLine(file);
				BinaryReader br = new BinaryReader(File.OpenRead(file));
				headers.Add(new StingrayHeaderFile().Read(br, file));
				br.Close();
			}*/

			Dictionary<ulong, int> extensions = new Dictionary<ulong, int>();
			foreach(var header in headers)
			{
				foreach(var extension in header.ExtensionEntries)
				{
					//if (extension.extension == "hash_lookup") Console.WriteLine(header.File);
					if (!extensions.TryGetValue(extension.extension.Hash, out _)) extensions[extension.extension.Hash] = 0;
					extensions[extension.extension.Hash]+=(int)extension.count;
				}
			}

			foreach(var extension in extensions)
			{
				Console.WriteLine(String.Format("{0}: {1}", new Hash64(extension.Key), extension.Value));
			}
			/*foreach(var header in headers)
			{
				foreach(var file in header.FileEntries)
				{
					if(file.path == new Hash64(0x4bd6f15b725a604d))
					{
						Console.WriteLine(header.File);
					}
				}
			}*/
			
			/*int extracted = 0;
			foreach(var header in headers)
			{
				foreach(var file in header.FileEntries)
				{
					if(file.size == 0) continue;

					BinaryReader br = new BinaryReader(File.OpenRead(file.streamfile));
					br.BaseStream.Position = file.offset;

					if (file.path.ToString().Contains("/")) {
						string[] folder_path_arr = file.path.ToString().Split('/');
						string folder_path = "";
						for(int i = 0; i < folder_path_arr.Length-1; i++)
						{
							folder_path += folder_path_arr[i] + "/";
						}
						if(!Directory.Exists(outdir + folder_path))
						{
							Directory.CreateDirectory(outdir + folder_path);
						}
					}

					string outfile = file.path.ToString() + "." + file.extension.ToString();


					File.WriteAllBytes(outdir + outfile, br.ReadBytes((int)file.size));
					br.Close();
					extracted++;
				}
			}*/

			//Console.WriteLine(String.Format("Extracted {0} files from {1}", extracted, helldivers));

			/*BinaryReader br = new BinaryReader(File.OpenRead(testfile));
			var header = new StingrayHeaderFile().Read(br, testfile);

			for (int i = 0; i < header.FileEntries.Count; i++)
			{
				Console.Write(header.FileEntries[i].offset);
				Console.Write(",");
				Console.Write(header.FileEntries[i].size);
				Console.Write(":");
				Console.Write(new Hash64(header.FileEntries[i].extension).Source);
				Console.WriteLine();
			}*/
		}
	}
}