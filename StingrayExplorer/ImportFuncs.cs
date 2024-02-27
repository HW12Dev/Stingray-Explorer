using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StingrayExplorer
{
	public static class ImportFuncs
	{
		[DllImport("stingray_murmur.dll", EntryPoint = "stingray_murmur_64")]
		public static extern UInt64 stingray_murmur_64(String str);

		[DllImport("stingray_murmur.dll", EntryPoint = "stingray_murmur_32")]
		public static extern UInt32 stingray_murmur_32(String str);
	}
}
