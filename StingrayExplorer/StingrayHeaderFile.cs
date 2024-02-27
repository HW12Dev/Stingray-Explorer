using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StingrayExplorer
{
	// For the purposes of reading files, a "Stingray Header File" is any file in the "data" folder that has no extension
	public class StingrayHeaderFile
	{
		public struct HeaderFileEntry
		{
			public Hash64 path;
			public Hash64 extension;
			public uint offset;
			public uint size;
			public string streamfile;

			public byte[] ending8bytes;
		}
		public struct HeaderExtensionEntry
		{
			public Hash64 extension;
			public uint count;
		}

		public StingrayHeaderFile() { }

		public List<HeaderFileEntry> FileEntries = new List<HeaderFileEntry>();
		public List<HeaderExtensionEntry> ExtensionEntries = new List<HeaderExtensionEntry>();

		public string File;

		public StingrayHeaderFile Read(BinaryReader br, string file)
		{
			this.File = file;

			br.ReadUInt32(); // all header files start with 0x11, 0x00, 0x00, 0xF0

			uint extensioncount = br.ReadUInt32(); // Unknown, possibly extension count

			uint filecount = br.ReadUInt32(); // most likely filecount

			br.BaseStream.Position = 0x50; // start of "extension" block

			for (int i = 0; i < extensioncount; i++) // size: 32 bytes
			{
				HeaderExtensionEntry entry = new HeaderExtensionEntry();
				entry.extension = new Hash64(br.ReadUInt64());
				entry.count = br.ReadUInt32();

				br.ReadBytes(4);

				if (i == extensioncount - 1)
				{
					br.ReadBytes(8);
				} else
				{
					br.ReadBytes(16);
				}

				ExtensionEntries.Add(entry);
			}


			//br.BaseStream.Position = 0xA8;
			for (int i = 0; i < filecount; i++)
			{
				HeaderFileEntry entry = new HeaderFileEntry();

				entry.path = new Hash64(br.ReadUInt64());
				entry.extension = new Hash64(br.ReadUInt64());
				br.ReadBytes(8); // unknown

				entry.offset = br.ReadUInt32();
				entry.streamfile = file + ".stream";


				br.BaseStream.Position += 0x20; // unknown bytes, ignored

				entry.size = br.ReadUInt32(); // file size

				br.BaseStream.Position += 0x8;

				//br.BaseStream.Position += 44; // unknown bytes, ignored

				entry.ending8bytes = br.ReadBytes(8); // unknown
				FileEntries.Add(entry);
			}

			br.BaseStream.Position += 0x58; // unknown

			// test
			//br.BaseStream.Position += 15 * filecount;

			int test = 0;
			foreach(var filee in FileEntries)
			{
				if (filee.extension == "wwise_stream" && test != 0) continue;
				if(filee.extension == "wwise_stream") { test++; }
				Console.WriteLine(filee.path + "." + filee.extension + ":");
				Console.Write(String.Format("\t"));
				foreach (var b in filee.ending8bytes)
				{
					Console.Write(String.Format("{0} ", b.ToString("X")));
				}
				Console.WriteLine();
			}

			return this;
		}
	}
}
