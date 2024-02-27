using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StingrayExplorer
{
	public class Hash64
	{
		private static Dictionary<ulong, string> hashlist = new Dictionary<ulong, string>();

		//public string Source = "Unknown hash";
		public ulong Hash = 0UL;
		bool InHashlist = false;

		public Hash64(string input)
		{
			//Source = input;
			Hash = ImportFuncs.stingray_murmur_64(input);
			//hashlist[Hash] = Source;
			InHashlist = true;
		}

		public Hash64(ulong hash)
		{
			Hash = hash;

			if(hashlist.ContainsKey(Hash))
			{
				//Source = hashlist[Hash];
				InHashlist = true;
			} else
			{
				//Source = hash.ToString("X").ToLower();
				InHashlist = false;
			}
		}

		public static void LoadHashlist(string hashlistpath)
		{
			LoadHashlist(File.ReadAllLines(hashlistpath));
		}
		public static void LoadHashlist(string[] sources) {
			foreach(string hash in sources)
			{
				if(hash != "" && !hash.StartsWith("//"))
				{
					hashlist[ImportFuncs.stingray_murmur_64(hash)] = hash;
				}
			}
		}

		public override string ToString()
		{
			if(InHashlist)
			{
				//return Source;
				return hashlist[Hash];
			} else
			{
				return Hash.ToString("X").ToLower();
			}
		}

		public string ToHex()
		{
			return Hash.ToString("X").ToLower();
		}

		public static bool operator==(Hash64 lhs, Hash64 rhs) { return lhs.Hash == rhs.Hash; }
		public static bool operator!=(Hash64 lhs, Hash64 rhs) { return lhs.Hash != rhs.Hash; }
		public static bool operator ==(Hash64 lhs, ulong rhs) { return lhs.Hash == rhs; }
		public static bool operator!=(Hash64 lhs, ulong rhs) { return lhs.Hash != rhs; }
		public static bool operator ==(Hash64 lhs, string rhs) { return lhs.Hash == new Hash64(rhs).Hash; }
		public static bool operator !=(Hash64 lhs, string rhs) { return lhs.Hash != new Hash64(rhs).Hash; }
	}
}
