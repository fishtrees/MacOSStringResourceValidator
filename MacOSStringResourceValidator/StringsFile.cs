using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MacOSStringResourceValidator
{
    class StringsFile
    {
        private string filePath;
        private StringBuilder validationMessages = new StringBuilder();

        public string ValidationMessage
        {
            get
            {
                return this.validationMessages.ToString();
            }
        }

        public StringsFile(string path)
        {
            this.filePath = path;
        }

        public bool Validate()
        {
            this.validationMessages.Clear();

            var encoding = GetAndValidateFileEncoding();
            if (null == encoding)
            {
                return false;
            }

            var text = File.ReadAllText(this.filePath, encoding);
            var clearedText = RemoveUselessContent(text);
            var lines = clearedText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                ValidateLine(line);
            }
            if (this.validationMessages.Length > 0)
            {
                return false;
            }
            return true;
        }

        private static bool IsUnicodeEncoding(Encoding encoding)
        {
            return encoding.EncodingName.Contains("UTF") == false && encoding.EncodingName.Contains("Unicode") == false;
        }

        private Encoding GetAndValidateFileEncoding()
        {
            var encoding = GetFileEncoding(this.filePath);
            var bytes = File.ReadAllBytes(this.filePath);
            try
            {
                TryDecodeStringWithCodePage(bytes, encoding.CodePage);
            }
            catch(DecoderFallbackException e)
            {
                this.validationMessages.AppendLine("Error, file loaded with " + encoding.EncodingName + ", but some chars can not be parsed, details below:");
                this.validationMessages.AppendLine("\t" + e.Message);
                this.validationMessages.AppendLine("\tRecommended encoding is UTF8.");
                return null;
            }
            return encoding;
        }

        private void ValidateLine(string line)
        {
            ValidateFormat(line);
        }

        private void ValidateFormat(string line)
        {
            var pattern = "^\\\"((?:\\\\.|[^\\\\\"])*?)\\\"\\s*=\\s*\\\"((?:\\\\.|[^\\\\\"])*?)\\\";";
            var match = Regex.Match(line, pattern);
            if (match.Success == false)
            {
                this.validationMessages.AppendLine("Error, invalid format: " + line);
            }
            else if (String.IsNullOrEmpty(match.Groups[2].Value))
            {
                this.validationMessages.AppendLine("Warning, empty translated string: " + line);
            }
        }

        private string RemoveUselessContent(string content)
        {
            return Regex.Replace(content, "\\/\\*.*?\\*\\/|\\/\\/.*?$", String.Empty, RegexOptions.Multiline);
        }

        private static Encoding GetFileEncoding(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8, true))
            {
                reader.Peek(); // you need this!
                return reader.CurrentEncoding;
            }
        }

        /// <summary>
        /// 用指定的编码方式解码原始字节数据，如果遇到无法解码的数据，则抛出DecoderFallbackException异常。
        /// </summary>
        /// <param name="data">待检测的字节数据</param>
        /// <param name="codePage">文本编码方式代码</param>
        /// <exception cref="DecoderFallbackException"></exception>
        /// <returns></returns>
        private static string TryDecodeStringWithCodePage(byte[] data, int codePage)
        {
            var encoder = Encoding.GetEncoding(codePage, new EncoderExceptionFallback(), new DecoderExceptionFallback());
            return encoder.GetString(data);
        }
    }
}
