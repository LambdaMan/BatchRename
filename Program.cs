using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BatchRename
{
	internal class Program
	{
		class Options
		{
			[Option("dir", Required = true, HelpText = "Specify the directory.")]
			public string DirectoryPath { get; set; } = string.Empty;

			[Option("removePattern", Required = false, HelpText = "Specify a fixed string to remove unconditionally on every file path.")]
			public string RemovePattern { get; set; } = string.Empty;

			[Option("removeRegex", Required = false, HelpText = "Specify a regex to remove unconditionally on every file path.")]
			public string RemoveRegex { get; set; } = string.Empty;

			[Option("removeStartPattern", Required = false, HelpText = "Specify a fixed string to match the start of a range to remove on every file path.")]
			public string RemoveStartPattern { get; set; } = string.Empty;

			[Option("removeEndPattern", Required = false, HelpText = "Specify a fixed string to match the end of a range to remove on every file path.")]
			public string RemoveEndPattern { get; set; } = string.Empty;

			[Option('r', Required = false, HelpText = "Recurse on sub directory.")]
			public bool Recursive { get; set; } = false;

			[Option('n', Required = false, HelpText = "Preview only.")]
			public bool Preview { get; set; } = false;


			internal Regex? RemoveRegexInstance { get; set; }
		}

		static void RenameDirectoryFiles(string directory, Options opts)
		{
			if (opts.Recursive)
			{
				foreach (string currentDirectory in Directory.GetDirectories(directory))
				{
					RenameDirectoryFiles(currentDirectory, opts);
				}
			}

			foreach (string sourceFilePath in Directory.GetFiles(directory))
			{
				FileInfo fi = new FileInfo(sourceFilePath);
				if(string.IsNullOrEmpty(fi.DirectoryName))
				{
					Console.WriteLine($"Error : Unexpected null directory path for {sourceFilePath}");
					continue;
				}
				string targetFilePath = string.Empty;
				if (string.IsNullOrEmpty(opts.RemovePattern) == false)
				{
					string targetFileName = fi.Name.Replace(opts.RemovePattern, string.Empty);
					if (targetFileName != fi.Name)
					{
						targetFilePath = Path.Combine(fi.DirectoryName, targetFileName);

					}
				}
				else if (opts.RemoveRegexInstance != null)
				{
					string targetFileName = opts.RemoveRegexInstance.Replace(fi.Name, string.Empty);
					if (targetFileName != fi.Name)
					{
						targetFilePath = Path.Combine(fi.DirectoryName, targetFileName);

					}
				}
				else if (string.IsNullOrEmpty(opts.RemoveStartPattern) == false && string.IsNullOrEmpty(opts.RemoveEndPattern) == false)
				{
					int startPos = fi.Name.IndexOf(opts.RemoveStartPattern);
					int endPos = fi.Name.LastIndexOf(opts.RemoveEndPattern);

					if (startPos != -1 && endPos != -1)
					{
						string nameStart = fi.Name.Substring(0, startPos);
						string nameEnd = fi.Name.Substring(endPos + opts.RemoveEndPattern.Length);
						string targetFileName = nameStart + nameEnd;
						if (targetFileName != fi.Name)
						{
							targetFilePath = Path.Combine(fi.DirectoryName, targetFileName);
						}
					}
				}


				if (targetFilePath != string.Empty)
				{
					if(!opts.Preview)
					{
						File.Move(sourceFilePath, targetFilePath);
					}
					Console.WriteLine($"Moved {sourceFilePath} --> {targetFilePath}");
				}
			}
		}

		static void Main(string[] args)
		{
			CommandLine.Parser.Default.ParseArguments<Options>(args).WithParsed(RunOptions).WithNotParsed(HandleParseError);
		}
		static void RunOptions(Options opts)
		{
			if (string.IsNullOrEmpty(opts.RemoveRegex) == false)
			{
				try
				{
					opts.RemoveRegexInstance = new Regex(opts.RemoveRegex);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Given regex {opts.RemoveRegex} is invalid ({ex.Message}).");
					return;
				}
			}

			RenameDirectoryFiles(opts.DirectoryPath, opts);
		}
		static void HandleParseError(IEnumerable<Error> errs)
		{
			//handle errors
		}
	}
}
