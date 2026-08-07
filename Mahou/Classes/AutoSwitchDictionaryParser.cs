using System;
using System.Collections.Generic;
using System.Threading;

namespace Mahou {
	internal sealed class AutoSwitchDictionaryParseResult {
		internal readonly bool Success;
		internal readonly string[] Sources;
		internal readonly string[] Replacements;
		internal readonly int RuleCount;
		internal readonly int AliasCount;
		internal readonly int CommentCount;
		internal readonly int ErrorIndex;

		internal AutoSwitchDictionaryParseResult(bool success, string[] sources, string[] replacements,
		                                         int ruleCount, int aliasCount, int commentCount,
		                                         int errorIndex) {
			Success = success;
			Sources = sources;
			Replacements = replacements;
			RuleCount = ruleCount;
			AliasCount = aliasCount;
			CommentCount = commentCount;
			ErrorIndex = errorIndex;
		}
	}

	internal static class AutoSwitchDictionaryParser {
		const string SourceMarker = "->";
		const string ReplacementMarker = "====>";
		const string EndMarker = "<====";
		static int invocationCount;

		internal static int InvocationCount {
			get { return Volatile.Read(ref invocationCount); }
		}

		internal static AutoSwitchDictionaryParseResult EmptySuccess() {
			return new AutoSwitchDictionaryParseResult(true, new string[0], new string[0], 0, 0, 0, -1);
		}

		internal static AutoSwitchDictionaryParseResult Parse(string dictionary) {
			Interlocked.Increment(ref invocationCount);
			if (String.IsNullOrEmpty(dictionary)) return EmptySuccess();

			var capacity = Math.Max(16, Math.Min(200000, dictionary.Length / 16));
			var sources = new List<string>(capacity);
			var replacements = new List<string>(capacity);
			var cursor = 0;
			var ruleCount = 0;
			var commentCount = 0;

			while (true) {
				SkipTrivia(dictionary, ref cursor, ref commentCount);
				if (cursor == dictionary.Length) {
					return new AutoSwitchDictionaryParseResult(true, sources.ToArray(), replacements.ToArray(),
						ruleCount, sources.Count, commentCount, -1);
				}
				if (!MatchesAt(dictionary, cursor, SourceMarker))
					return Failure(commentCount, cursor);

				var sourceStart = cursor + SourceMarker.Length;
				var lineEnd = dictionary.IndexOf('\n', sourceStart, dictionary.Length - sourceStart);
				if (lineEnd < 0) lineEnd = dictionary.Length;
				var replacementMarker = dictionary.IndexOf(ReplacementMarker, sourceStart,
					lineEnd - sourceStart, StringComparison.Ordinal);
				var sourceEnd = replacementMarker < 0 ? lineEnd : replacementMarker;
				while (sourceEnd > sourceStart && dictionary[sourceEnd - 1] == '\r') sourceEnd--;
				if (replacementMarker < 0) {
					cursor = lineEnd == dictionary.Length ? lineEnd : lineEnd + 1;
					SkipTrivia(dictionary, ref cursor, ref commentCount);
					if (!MatchesAt(dictionary, cursor, ReplacementMarker))
						return Failure(commentCount, cursor);
					replacementMarker = cursor;
				}
				var replacementStart = replacementMarker + ReplacementMarker.Length;
				var replacementEnd = dictionary.IndexOf(EndMarker, replacementStart,
					dictionary.Length - replacementStart, StringComparison.Ordinal);
				if (replacementEnd < 0) return Failure(commentCount, replacementStart);

				var replacement = CopyCompletedValue(dictionary, replacementStart,
					replacementEnd - replacementStart);
				var aliasStart = sourceStart;
				while (aliasStart <= sourceEnd) {
					var separator = dictionary.IndexOf('|', aliasStart, sourceEnd - aliasStart);
					var aliasEnd = separator < 0 ? sourceEnd : separator;
					sources.Add(CopyCompletedValue(dictionary, aliasStart, aliasEnd - aliasStart));
					replacements.Add(replacement);
					if (separator < 0) break;
					aliasStart = separator + 1;
				}

				ruleCount++;
				cursor = replacementEnd + EndMarker.Length;
			}
		}

		static AutoSwitchDictionaryParseResult Failure(int commentCount, int errorIndex) {
			return new AutoSwitchDictionaryParseResult(false, new string[0], new string[0],
				0, 0, commentCount, errorIndex);
		}

		static void SkipTrivia(string dictionary, ref int cursor, ref int commentCount) {
			while (cursor < dictionary.Length) {
				while (cursor < dictionary.Length &&
				       (Char.IsWhiteSpace(dictionary[cursor]) || dictionary[cursor] == '\ufeff')) cursor++;
				if (cursor >= dictionary.Length) return;
				var isComment = dictionary[cursor] == '#' ||
					(cursor + 1 < dictionary.Length && dictionary[cursor] == '/' && dictionary[cursor + 1] == '/');
				if (!isComment) return;
				commentCount++;
				var lineEnd = dictionary.IndexOf('\n', cursor, dictionary.Length - cursor);
				cursor = lineEnd < 0 ? dictionary.Length : lineEnd + 1;
			}
		}

		static bool MatchesAt(string dictionary, int index, string marker) {
			if (index < 0 || index + marker.Length > dictionary.Length) return false;
			return String.CompareOrdinal(dictionary, index, marker, 0, marker.Length) == 0;
		}

		static string CopyCompletedValue(string dictionary, int start, int length) {
			var carriageReturns = 0;
			for (var index = start; index < start + length; index++)
				if (dictionary[index] == '\r') carriageReturns++;
			if (carriageReturns == 0) return dictionary.Substring(start, length);

			var value = new char[length - carriageReturns];
			var target = 0;
			for (var index = start; index < start + length; index++)
				if (dictionary[index] != '\r') value[target++] = dictionary[index];
			return new string(value);
		}
	}
}
