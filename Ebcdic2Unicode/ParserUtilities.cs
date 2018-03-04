using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Ebcdic2Unicode
{
    public static class ParserUtilities
    {
        /// <summary>
        /// Gets a sub-array from an existing array
        /// </summary>
        /// <param name="sourceBytes">Input bytes</param>
        /// <param name="startPosition">Start index</param>
        /// <param name="length">Size of the output array</param>
        /// <param name="throwExceptionIfSourceArrayIsTooShort"></param>
        /// <returns>Slice of an array</returns>
        public static byte[] ReadBytesRange(byte[] sourceBytes, int startPosition, int length, bool throwExceptionIfSourceArrayIsTooShort = true)
        {
            byte[] resultBytes;

            if (length <= 0)
            {
                throw new Exception("Invalid array length: " + length);
            }
            if (startPosition < 0)
            {
                throw new Exception("Invalid start position: " + length);
            }
            if (sourceBytes.Length < startPosition)
            {
                throw new Exception("Start position is outside of array bounds");
            }
            if (sourceBytes.Length - startPosition - length < 0)
            {
                if (throwExceptionIfSourceArrayIsTooShort)
                {
                    throw new Exception("End position is outside of array bounds");
                }
                else
                {
                    //Shorten the length of output array for remaining bytes
                    length = sourceBytes.Length - startPosition;
                }
            }

            resultBytes = new byte[length];
            Array.Copy(sourceBytes, startPosition, resultBytes, 0, length);
            return resultBytes;
        }

        public static string ConvertBytesToStringBase16(byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }

        public static string ConvertBytesToStringBase10(byte[] bytes)
        {
            var sb = new StringBuilder();

            foreach (byte b in bytes)
            {
                sb.Append(b); 
                sb.Append(" ");
            }
            //string result = "";
            //foreach (byte b in bytes)
            //{
            //    result += (int)b + " ";
            //}
            return sb.ToString().Trim();
        }

        public static string ConvertBytesToStringBase2(byte[] bytes)
        {
            var bitArray = new BitArray(bytes);
            var sb = new StringBuilder();

            foreach (bool bit in bitArray)
            {
                sb.Insert(0, (bit ? "1" : "0"));
            }
            return sb.ToString();
        }

        public static byte[] ConvertHexStringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
            {
                throw new Exception("HEX string is not valid. String cannot contain odd number of digits");
            }
            if (Regex.IsMatch(hex, @"[^0-9A-F]"))
            {
                throw new Exception("HEX string contains invalid chars. Use only 0-9 and A-F (upper case)");
            }

            byte[] arr = new byte[hex.Length / 2];

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = (byte)((ConvertHexCharToInt(hex[i << 1]) << 4) + (ConvertHexCharToInt(hex[(i << 1) + 1])));
            }
            return arr;
        }

        private static int ConvertHexCharToInt(char hex)
        {
            int val = hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }



        public static string GetCompulsoryAttributeValue(XElement element, string attributeName)
        {
            XAttribute attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                throw new Exception(string.Format("Attribute '{0}' does not exist", attributeName));
            }
            else
            {
                return attribute.Value;
            }
        }

        public static string GetNullableAttributeValue(XElement element, string attributeName)
        {
            XAttribute attribute = element.Attribute(attributeName);
            if (attribute == null)
            {
                return null;
            }
            else
            {
                return attribute.Value;
            }
        }

        public static int GetAttributeNumericValue(XElement element, string attributeName)
        {
            string numStr = GetCompulsoryAttributeValue(element, attributeName);
            int numValue;

            if (Int32.TryParse(numStr, out numValue))
            {
                return numValue;
            }
            else
            {
                throw new Exception(string.Format("Unable to convert attribute '{0}' (value '{1}') to integer", attributeName, numStr));
            }
        }

        public static FieldType GetFieldType(XElement element, string attributeName)
        {
            string fieldTypeStr = GetCompulsoryAttributeValue(element, attributeName);

            try
            {
                FieldType type = (FieldType)Enum.Parse(typeof(FieldType), fieldTypeStr);
                return type;
            }
            catch
            {
                throw new Exception(string.Format("'{0}' is not a member of the FieldType enumeration", fieldTypeStr));
            }
        }



        public static bool WriteParsedLineArrayToXml(ParsedLine[] lines, string outputFilePath, bool includeSrcBytesInHex = false)
        {
            Console.WriteLine("{0}: Writing output XML file...", DateTime.Now);

            if (lines == null || lines.Length == 0)
            {
                Console.WriteLine("{0}: Nothing to write", DateTime.Now);
                return true;
            }

            try
            {
                XElement parsedFile = new XElement("parsedFile");

                foreach (ParsedLine line in lines)
                {
                    parsedFile.Add(line.ToXml(includeSrcBytesInHex));
                }

                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }

                using (StreamWriter sw = new StreamWriter(outputFilePath))
                {
                    parsedFile.Save(sw);
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
                return false;
            }

            Console.WriteLine("{1}: Output file created {0}", Path.GetFileName(outputFilePath), DateTime.Now);
            return true;
        }

        public static bool WriteParsedLineArrayToTxt(ParsedLine[] lines, string outputFilePath, string delimiter = "\t", bool includeColumnNames = true, bool addQuotes = true)
        {
            Console.WriteLine("{0}: Writing output TXT file...", DateTime.Now);

            if (lines == null || lines.Length == 0)
            {
                Console.WriteLine("{0}: Nothing to write", DateTime.Now);
                return true;
            }

            try
            {
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }

                using (TextWriter tw = new StreamWriter(outputFilePath, true))
                {
                    if (includeColumnNames && lines.Length > 0)
                    {
                        tw.WriteLine(string.Join(delimiter, lines[0].Template.GetFieldNames(addQuotes)));
                    }
                    foreach (ParsedLine line in lines)
                    {
                        tw.WriteLine(string.Join(delimiter, line.GetFieldValues(addQuotes)));
                    }
                }
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
                return false;
            }

            Console.WriteLine("{1}: Output file created {0}", Path.GetFileName(outputFilePath), DateTime.Now);
            return true;
        }

        public static bool WriteParsedLineArrayToCsv(ParsedLine[] lines, string outputFilePath, bool includeColumnNames = true, bool addQuotes = true)
        {
            return WriteParsedLineArrayToTxt(lines, outputFilePath, ",", includeColumnNames, addQuotes);
        }

        public static string RemoveNonAsciiChars(string text)
        {
            char[] chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] < 32 || chars[i] > 126)
                {
                    chars[i] = ' '; //Convert non-printable char to "space"
                }
                //else if (chars[i] == '"' || chars[i] == '^') //Remove quote char
                //{
                //    chars[i] = ' ';
                //}
            }

            return new String(chars).Trim();
        }



        public static void PrintError(string errMsg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errMsg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static string Right(string text, int length) //To support VB Right() function
        {
            string result;
            if (string.IsNullOrEmpty(text) || length <= 0)
            {
                return string.Empty;
            }
            if (text.Length < length)
            {
                length = text.Length;
            }

            result = text.Substring(text.Length - length);
            return result;
        }
    }
}
