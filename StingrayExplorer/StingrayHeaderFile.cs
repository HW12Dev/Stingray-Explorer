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
			public enum SourceLocation
			{
				HEADER = 1 << 0,
				STREAM = 1 << 1,
				GPU_RESOURCES = 1 << 2,
			}
			public Hash64 path;
			public Hash64 extension;

			public uint stream_offset;
			public uint gpu_offset;
			public uint header_offset;

			public uint header_size;
			public uint stream_size;
			public uint gpu_size;

			public int source_location;

			public uint MiscDataOffset; // unknown for the most part

			public override string ToString() {
				return path + "." + extension;
			}
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

				//br.ReadBytes(8); // unknown
				entry.MiscDataOffset = br.ReadUInt32();

				br.ReadBytes(4); // unknown

				entry.stream_offset = br.ReadUInt32();

				br.BaseStream.Position += 4; // unknown

				entry.gpu_offset = br.ReadUInt32();

				br.BaseStream.Position += 4; // unknown

				//entry.source_file = file + ".stream";

				br.BaseStream.Position += 16; // unknown bytes, ignored


				entry.header_size = br.ReadUInt32(); // file size
				entry.stream_size = br.ReadUInt32(); // file size
				entry.gpu_size = br.ReadUInt32(); // file size

				if (entry.gpu_size == 0 && entry.stream_size == 0)
				{
					entry.source_location |= (int)HeaderFileEntry.SourceLocation.HEADER;
				} else if (entry.gpu_size != 0 && entry.stream_size == 0)
				{
					entry.source_location |= (int)HeaderFileEntry.SourceLocation.GPU_RESOURCES;
				} else if (entry.gpu_size == 0 && entry.stream_size != 0)
				{
					entry.source_location |= (int)HeaderFileEntry.SourceLocation.STREAM;
				} else if (entry.header_offset == 0) { } else
				{
					Console.WriteLine("UNKNOWN source location for file: " + entry.path + "." + entry.extension + " " + entry.header_offset + ":" + entry.header_size + " " + entry.stream_size + " " + entry.gpu_size);
				}

				br.BaseStream.Position += 0x4; // unknown 4 bytes

				//br.BaseStream.Position += 44; // unknown bytes, ignored

				br.BaseStream.Position += 8;// br.ReadBytes(8); // unknown
				
				FileEntries.Add(entry);
			}

			//br.BaseStream.Position += 0x58; // unknown

			// test
			//br.BaseStream.Position += 15 * filecount;

			/*int test = 0;
			foreach(var filee in FileEntries)
			{
				if (filee.extension == "wwise_stream" && test > 20) continue;
				if(filee.extension == "wwise_stream") { test++; }
				Console.WriteLine(filee.path + "." + filee.extension + ":" + " off: h" + filee.header_offset + "/s" + filee.stream_offset + "/g" + filee.gpu_offset + " size: h" + filee.header_size + "/s" + filee.stream_size + "/g" + filee.gpu_size + " loc: " + filee.source_location);
				Console.Write(String.Format("\t"));
				Console.Write(filee.MiscDataOffset);
				Console.WriteLine();
			}*/

			return this;
		}

		public bool WriteAsset(Hash64 path, Hash64 extension, BinaryWriter bw)
		{
			foreach(var file in FileEntries)
			{
				if(file.path == path && file.extension == extension)
				{
					return WriteAsset(file, bw);
				}
			}
			return false;
		}

		public bool WriteAsset(HeaderFileEntry entry, BinaryWriter bw)
		{
			//Console.Write(entry.MiscDataOffset + ":" + GetExtraDataLength(entry));
			//Console.WriteLine();
			// Process Misc Data first

			/*if(entry.path == 0x3cfd61f81a6c2b3d)
			{
				System.Diagnostics.Debugger.Break();
			}*/

			try
			{
				BinaryReader br = new BinaryReader(System.IO.File.OpenRead(File));
				br.BaseStream.Position = entry.MiscDataOffset + 4;
				byte[] extradata;// = br.ReadBytes(br.ReadInt32());
				if (entry.extension == "texture")
				{
					br.BaseStream.Position += 188;
					extradata = br.ReadBytes(160);
				} else if(entry.extension == "package")
				{
					br.BaseStream.Position += 4;
					int count = br.ReadInt32();
					br.BaseStream.Position += 4;
					extradata = br.ReadBytes(count*16);
				} else if (entry.extension == "hash_lookup")
				{
					br.BaseStream.Position -= 4;
					int size = br.ReadInt32();
					extradata = br.ReadBytes(size);
				} else
				{
					extradata = br.ReadBytes(br.ReadInt32());
				}

				bw.Write(extradata);

				Array.Clear(extradata, 0, extradata.Length);
				extradata = null;

				if ((entry.source_location & (int)HeaderFileEntry.SourceLocation.GPU_RESOURCES) != 0)
				{
					BinaryReader br_gpu = new BinaryReader(new BufferedStream(System.IO.File.OpenRead(File + ".gpu_resources")));
					br_gpu.BaseStream.Position = entry.gpu_offset;
					bw.Write(br_gpu.ReadBytes((int)entry.gpu_size));
					br_gpu.Close();
					br_gpu.Dispose();
					br_gpu = null;
				}
				if ((entry.source_location & (int)HeaderFileEntry.SourceLocation.STREAM) != 0)
				{
					BinaryReader br_stream = new BinaryReader(new BufferedStream(System.IO.File.OpenRead(File + ".stream")));
					br_stream.BaseStream.Position = entry.stream_offset;
					bw.Write(br_stream.ReadBytes((int)entry.stream_size));
					br_stream.Close();
					br_stream.Dispose();
					br_stream = null;
				}

				br.Close();
			} catch (Exception e) { }
			GC.Collect();
			return true;
		}
	}
}
