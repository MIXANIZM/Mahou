using System;
using System.IO;
using System.Text;

namespace Mahou {
	/// <summary>Writes low-frequency user data through a flushed temporary file and same-directory replace.</summary>
	static class AtomicFile {
		static readonly object SyncRoot = new object();

		public static void WriteAllText(string path, string content, Encoding encoding) {
			if (String.IsNullOrEmpty(path)) throw new ArgumentException("A destination path is required.", "path");
			if (encoding == null) throw new ArgumentNullException("encoding");

			lock (SyncRoot) {
				var fullPath = Path.GetFullPath(path);
				var directory = Path.GetDirectoryName(fullPath);
				if (!String.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

				var temp = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
				var backup = fullPath + ".bak";
				try {
					using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None,
						4096, FileOptions.WriteThrough)) {
						using (var writer = new StreamWriter(stream, encoding, 4096, true)) {
							writer.Write(content ?? String.Empty);
							writer.Flush();
						}
						stream.Flush(true);
					}

					if (File.Exists(fullPath)) {
						try {
							File.Replace(temp, fullPath, backup, true);
						} catch (PlatformNotSupportedException) {
							ReplaceByCopy(temp, fullPath);
						} catch (IOException) {
							ReplaceByCopy(temp, fullPath);
						}
					} else {
						File.Move(temp, fullPath);
					}
				} finally {
					try { if (File.Exists(temp)) File.Delete(temp); } catch { }
				}
			}
		}

		static void ReplaceByCopy(string temp, string destination) {
			File.Copy(temp, destination, true);
			File.Delete(temp);
		}
	}
}
