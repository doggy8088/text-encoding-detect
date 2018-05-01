// Copyright 2015-2016 Jonathan Bennett <jon@autoitscript.com>
// 
// https://www.autoitscript.com 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AutoIt.Common;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: TextEncodingDetect.exe <filename>|<dirname>");
            return 1;
        }

        // find docs -not -type d -exec "C:\Users\wakau\source\repos\text-encoding-detect\TextEncodingDetect-C#\TextEncodingDetect\bin\Debug\TextEncodingDetect.exe" "{}" ";"
        // "C:\Users\wakau\source\repos\text-encoding-detect\TextEncodingDetect-C#\TextEncodingDetect\bin\Debug\TextEncodingDetect.exe" .

        if (Directory.Exists(args[0]))
        {
            var files = Directory.GetFiles(args[0], "*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if (file.Contains(Path.DirectorySeparatorChar + "."))
                {
                    continue;
                }
                ShowResult(file);
            }
        }
        else
        {
            ShowResult(args[0]);
        }

        return 0;
    }

    private static void ShowResult(string filename, bool showPerfect = false)
    {
        var result = GetFileState(filename);

        if (!showPerfect && (result.Lines == result.CRLFs || result.Lines == result.LFs))
        {
            return;
        }

        Console.WriteLine($"File: {filename}");
        Console.WriteLine($"Encoding: {result.Encoding}");
        Console.WriteLine($"Lines: {result.Lines}\tCRLF: {result.CRLFs}\tLF: {result.LFs}");
        if (result.Lines == result.CRLFs || result.Lines == result.LFs)
        {
            Console.WriteLine($"Result: PERFECT!");
        }
        else
        {
            Console.WriteLine($"Result: INCONSISTENCE!!!!!!!!!!!!!");
        }
        Console.WriteLine();
    }

    public class FileStateModel
    {
        public TextEncodingDetect.Encoding Encoding { get; set; }
        public int Lines { get; set; }
        public int CRLFs { get; set; }
        public int LFs { get; set; }
    }

    public static FileStateModel GetFileState(string filename)
    {
        // Read in the file in binary
        byte[] buffer;

        try
        {
            buffer = File.ReadAllBytes(filename);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw ex;
        }

        // Detect encoding
        var textDetect = new TextEncodingDetect();
        TextEncodingDetect.Encoding encoding = textDetect.DetectEncoding(buffer, buffer.Length);

        string str = "";

        StringBuilder sb = new StringBuilder();

        //sb.AppendLine("File: " + filename);

        //sb.Append("Encoding: ");
        if (encoding == TextEncodingDetect.Encoding.None)
        {
            //sb.AppendLine("Binary");
        }
        else if (encoding == TextEncodingDetect.Encoding.Ascii)
        {
            str = Encoding.ASCII.GetString(buffer);
            //sb.AppendLine("ASCII (chars in the 0-127 range)");
        }
        else if (encoding == TextEncodingDetect.Encoding.Ansi)
        {
            str = Encoding.Default.GetString(buffer);
            //sb.AppendLine("ANSI (chars in the range 0-255 range)");
        }
        else if (encoding == TextEncodingDetect.Encoding.Utf8Bom || encoding == TextEncodingDetect.Encoding.Utf8Nobom)
        {
            str = Encoding.UTF8.GetString(buffer);
            //sb.AppendLine("UTF-8");
        }
        else if (encoding == TextEncodingDetect.Encoding.Utf16LeBom || encoding == TextEncodingDetect.Encoding.Utf16LeNoBom)
        {
            str = Encoding.Unicode.GetString(buffer);
            //sb.AppendLine("UTF-16 Little Endian");
        }
        else if (encoding == TextEncodingDetect.Encoding.Utf16BeBom || encoding == TextEncodingDetect.Encoding.Utf16BeNoBom)
        {
            str = Encoding.BigEndianUnicode.GetString(buffer);
            //sb.AppendLine("UTF-16 Big Endian");
        }

        int All_Lines = 0;
        int CRLF_Count = 0;
        int LF_Count = 0;

        if (encoding != TextEncodingDetect.Encoding.None)
        {
            All_Lines = LineBreakCount(str);
            CRLF_Count = LineBreakCount(str, new[] { "\r\n" });
            LF_Count = All_Lines - CRLF_Count;

            //sb.AppendLine(
            //    "Length: " + str.Length + "\t" +
            //    "Lines: " + All_Lines + "\t" +
            //    "CRLF: " + CRLF_Count + "\t" +
            //    "  LF: " + (LF_Count));
        }

        return new FileStateModel()
        {
            Encoding = encoding,
            Lines = All_Lines,
            CRLFs = CRLF_Count,
            LFs = LF_Count
        };
    }

    public static int LineBreakCount(string s)
    {
        if (s == null) throw new ArgumentNullException("s");

        return LineBreakCount(s, new[] { "\r\n", "\r", "\n" });
    }
    public static int LineBreakCount(string s, params string[] patterns)
    {
        if (s == null) throw new ArgumentNullException("s");
        if (patterns == null) throw new ArgumentNullException("patterns");

        return s.Split(patterns, StringSplitOptions.None).Length - 1;
    }
}