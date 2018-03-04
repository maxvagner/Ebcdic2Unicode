using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml.Linq;
using Ebcdic2Unicode.Constants;

namespace Ebcdic2Unicode
{
    public class ParsedField
    {
        public FieldTemplate Template { get; private set; }

        public string Text { get; private set; }

        public byte[] OriginalBytes { get; private set; }

        public string OriginalBytesBase16
        {
            get
            {
                return ParserUtilities.ConvertBytesToStringBase16(this.OriginalBytes);
            }
        }

        public string OriginalBytesBase10
        {
            get
            {
                return ParserUtilities.ConvertBytesToStringBase10(this.OriginalBytes);
            }
        }

        public string OriginalBytesBase2
        {
            get
            {
                return ParserUtilities.ConvertBytesToStringBase2(this.OriginalBytes);
            }
        }

        // TODO: What's the purpose of this property??
        public bool ParsedSuccessfully { get; private set; }


        //Constructor
        public ParsedField(byte[] lineBytes, FieldTemplate fieldTemplate)
        {
            try
            {
                this.Template = fieldTemplate;
                bool isParsedSuccessfully;
                this.Text = this.ParseField(lineBytes, fieldTemplate, out isParsedSuccessfully);
                this.ParsedSuccessfully = isParsedSuccessfully;
            }
            catch (Exception ex)
            {
                //Used for debugging
                Console.WriteLine("Exception parsing field {0} ({1})", fieldTemplate.FieldName, fieldTemplate.Type.ToString());
                throw ex;
            }
        }

        // TODO: PasedField should be like a data transfer object. Move parsing logic into dedicated class.
        private string ParseField(byte[] lineBytes, FieldTemplate fieldTemplate, out bool isParsedSuccessfully)
        {
            if (lineBytes == null || lineBytes.Length == 0)
            {
                ParserUtilities.PrintError("lineBytes array is null or empty");
                isParsedSuccessfully = false;
                return string.Empty;
            }
            if (lineBytes.Length < (fieldTemplate.StartPosition + fieldTemplate.FieldSize))
            {
                throw new Exception(String.Format(Messages.FieldOutsideLineBoundary, fieldTemplate.FieldName));
            }

            byte[] fieldBytes = new byte[fieldTemplate.FieldSize];
            Array.Copy(lineBytes, fieldTemplate.StartPosition, fieldBytes, 0, fieldTemplate.FieldSize);
            this.OriginalBytes = fieldBytes;

            string result = string.Empty;
            isParsedSuccessfully = true;

            switch (fieldTemplate.Type)
            {
                case FieldType.String:
                    result = this.ConvertEbcdicString(fieldBytes);
                    break;

                case FieldType.NumericString:
                    result = this.ConvertEbcdicNumericString(fieldBytes, fieldTemplate.DecimalPlaces, out isParsedSuccessfully);
                    break;

                case FieldType.Packed:
                    result = this.Unpack(fieldBytes, fieldTemplate.DecimalPlaces, out isParsedSuccessfully);
                    break;

                case FieldType.BinaryNum:
                    result = this.ConvertBinaryNumber(fieldBytes, fieldTemplate.DecimalPlaces, out isParsedSuccessfully);
                    break;

                case FieldType.PackedDate:
                    result = this.ConvertMainframePackedDate(fieldBytes, out isParsedSuccessfully);
                    break;

                case FieldType.DateString:
                    result = ConvertEbcdicDateString(fieldBytes, out isParsedSuccessfully);
                    break;

                case FieldType.DateStringMMDDYY:
                    result = this.ConvertEbcdicCustomDateStr(fieldBytes, Formats.MMDDYY, Formats.YYYY_MM_DD, out isParsedSuccessfully);
                    break;

                case FieldType.SourceBytesBase16:
                    result = ParserUtilities.ConvertBytesToStringBase16(fieldBytes);
                    break;

                case FieldType.SourceBytesBase10:
                    result = ParserUtilities.ConvertBytesToStringBase10(fieldBytes);
                    break;

                case FieldType.SourceBytesBase2:
                    result = ParserUtilities.ConvertBytesToStringBase2(fieldBytes);
                    break;

                case FieldType.StringEncIbm935:
                    result = CustomIbm935Mapper.GetUnicodeString(fieldBytes).Trim();
                    break;

                case FieldType.StringUnicode:
                    result = System.Text.Encoding.Default.GetString(fieldBytes);
                    break;

                default:
                    isParsedSuccessfully = false;
                    new Exception(String.Format(Messages.ParserNotImplemented, fieldTemplate.FieldName, fieldTemplate.Type.ToString()));
                    break;
            }

            return result;
        }

        private string ConvertEbcdicString(byte[] ebcdicBytes)
        {
            if (ebcdicBytes.All(p => p == 0x00 || p == 0xFF))
            {
                //Every byte is either 0x00 or 0xFF (fillers)
                return string.Empty;
            }

            Encoding ebcdicEnc = Encoding.GetEncoding("IBM037");
            string result = ebcdicEnc.GetString(ebcdicBytes); // convert EBCDIC Bytes -> Unicode string
            return result;
        }

        private string ConvertEbcdicNumericString(byte[] ebcdicBytes, int decimalPlaces, out bool isParsedSuccessfully)
        {
            string tempNumStr = this.ConvertEbcdicString(ebcdicBytes).Trim();

            if (tempNumStr == string.Empty)
            {
                isParsedSuccessfully = true;
                return string.Empty;
            }
            if (String.IsNullOrWhiteSpace(tempNumStr))
            {
                isParsedSuccessfully = false;
                return string.Empty;
            }

            if (Regex.IsMatch(tempNumStr, @"^\d+$")) //Unsigned integer
            {
                isParsedSuccessfully = true;
                string result = this.AdjustDecimals(Int64.Parse(tempNumStr), decimalPlaces);
                return result;
            }
            else if (Regex.IsMatch(tempNumStr, @"^\d*[A-R}{]$")) //Signed integer. Last characrer is A-R, or curly braces
            {
                string lastChar = ParserUtilities.Right(tempNumStr, 1);
                long parsedNumber;

                switch (lastChar)
                {
                    case "{":
                        tempNumStr = tempNumStr.Replace("{", "0");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "A":
                        tempNumStr = tempNumStr.Replace("A", "1");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "B":
                        tempNumStr = tempNumStr.Replace("B", "2");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "C":
                        tempNumStr = tempNumStr.Replace("C", "3");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "D":
                        tempNumStr = tempNumStr.Replace("D", "4");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "E":
                        tempNumStr = tempNumStr.Replace("E", "5");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "F":
                        tempNumStr = tempNumStr.Replace("F", "6");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "G":
                        tempNumStr = tempNumStr.Replace("G", "7");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "H":
                        tempNumStr = tempNumStr.Replace("H", "8");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "I":
                        tempNumStr = tempNumStr.Replace("I", "9");
                        parsedNumber = Int64.Parse(tempNumStr);
                        break;
                    case "}": //Negative numbers
                        tempNumStr = tempNumStr.Replace("}", "0");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    case "J":
                        tempNumStr = tempNumStr.Replace("J", "1");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    case "K":
                        tempNumStr = tempNumStr.Replace("K", "2");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    case "L":
                        tempNumStr = tempNumStr.Replace("L", "3");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    case "M":
                        tempNumStr = tempNumStr.Replace("M", "4");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    case "N":
                        tempNumStr = tempNumStr.Replace("N", "5");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    case "O":
                        tempNumStr = tempNumStr.Replace("O", "6");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    case "P":
                        tempNumStr = tempNumStr.Replace("P", "7");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    case "Q":
                        tempNumStr = tempNumStr.Replace("Q", "8");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    case "R":
                        tempNumStr = tempNumStr.Replace("R", "9");
                        parsedNumber = Int64.Parse(tempNumStr) * (-1);
                        break;
                    default:
                        throw new Exception(String.Format(Messages.UnableToConvertStringToNumber, tempNumStr));
                }

                isParsedSuccessfully = true;
                if (decimalPlaces > 0)
                {
                    return this.AdjustDecimals(parsedNumber, decimalPlaces);
                }
                return parsedNumber.ToString();
            }
            else
            {
                isParsedSuccessfully = false;
                return tempNumStr;
            }
        }

        private string ConvertEbcdicDateString(byte[] ebcdicBytes, out bool isParsedSuccessfully)
        {
            string text = this.ConvertEbcdicString(ebcdicBytes).Trim();

            if (text == string.Empty || Regex.IsMatch(text, "^0+$") || Regex.IsMatch(text, "^9+$"))
            {
                isParsedSuccessfully = true;
                return string.Empty;
            }

            DateTime tempDate;
            if (text.Length == 6 && Regex.IsMatch(text, @"^\d{6}$"))
            {
                if (DateTime.TryParseExact(text, Formats.YYMMDD, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                {
                    isParsedSuccessfully = true;
                    return tempDate.ToString(Formats.YYYY_MM_DD);
                }
            }
            if (text.Length == 7 && Regex.IsMatch(text, @"^\d{7}$"))
            {
                //cyyMMdd (c = century)
                return this.ConvertMainframeDateFormat(text, out isParsedSuccessfully);
            }
            if (text.Length == 8 && Regex.IsMatch(text, @"^\d{8}$"))
            {
                if (DateTime.TryParseExact(text, Formats.YYYYMMDD, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                {
                    isParsedSuccessfully = true;
                    return tempDate.ToString(Formats.YYYY_MM_DD);
                }
            }

            isParsedSuccessfully = false;
            return text;
        }

        private string ConvertEbcdicCustomDateStr(byte[] ebcdicBytes, string sourceDateStringFormat, string outputDateFormat, out bool isParsedSuccessfully) //string outputDateFormat = "yyyy-MM-dd"
        {
            string dateStr = this.ConvertEbcdicString(ebcdicBytes).Trim();
            DateTime tempDate;

            if (DateTime.TryParseExact(dateStr, sourceDateStringFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
            {
                isParsedSuccessfully = true;
                return tempDate.ToString(outputDateFormat);
            }
            else
            {
                isParsedSuccessfully = false;
                return dateStr;
            }
        }

        private string ConvertMainframePackedDate(byte[] packedBytes, out bool isParsedSuccessfully)
        {
            bool isUnpackedSuccessfully;
            string unpackedNumberStr = this.Unpack(packedBytes, 0, out isUnpackedSuccessfully).Trim();

            if (isUnpackedSuccessfully)
            {
                return this.ConvertMainframeDateFormat(unpackedNumberStr, out isParsedSuccessfully);
            }
            else
            {
                isParsedSuccessfully = false;
                return unpackedNumberStr;
            }
        }

        private string ConvertMainframeDateFormat(string cyyMMdd_dateStr, out bool isParsedSuccessfully) //cyyMMdd (c = century)
        {
            string originalString = cyyMMdd_dateStr;
            cyyMMdd_dateStr = cyyMMdd_dateStr.PadLeft(7, '0');

            if (cyyMMdd_dateStr.Length != 7)
            {
                isParsedSuccessfully = false;
                return cyyMMdd_dateStr;
            }
            if (cyyMMdd_dateStr == "0000000" || cyyMMdd_dateStr == "0999999" || cyyMMdd_dateStr == "9999999") //cyyMMdd_dateStr == "0"
            {
                isParsedSuccessfully = true;
                return string.Empty;
            }

            Match match = Regex.Match(cyyMMdd_dateStr, @"^(?<Year>\d{3})(?<Month>\d{2})(?<Day>\d{2})$"); //E.g.: 0801232 = 1980-12-31; 1811231 = 2080-12-31

            if (match.Success)
            {
                try
                {
                    int year = Int32.Parse(match.Groups["Year"].Value) + 1900; //013 = 1913, 113 = 2013...
                    int month = Int32.Parse(match.Groups["Month"].Value);
                    int day = Int32.Parse(match.Groups["Day"].Value);

                    DateTime tempDate = new DateTime(year, month, day);
                    isParsedSuccessfully = true;
                    return tempDate.ToString(Formats.YYYY_MM_DD);
                }
                catch { }
            }

            isParsedSuccessfully = false;
            return originalString;
        }

        private string ConvertBinaryNumber(byte[] bytes, int decimalPlaces, out bool isParsedSuccessfully) //Be careful about the order of bytes!
        {
            //BitConverter requires low order bytes goes first, followed by the higher order bytes. 
            //Bytes are stored in the file in the opposite order, thus need to reverse bytes
            Array.Reverse(bytes);
            long tempNum;

            if (bytes.Length == 1)
            {
                //If 2 bytes are provided -- assume it's a byte
                //tempNum = BitConverter.ToChar(ebcdicBytes, 0);
                tempNum = bytes[0];
            }
            else if (bytes.Length == 2)
            {
                //If 2 bytes are provided -- assume it's a short
                tempNum = BitConverter.ToUInt16(bytes, 0);
            }
            else if (bytes.Length == 4)
            {
                //If 4 bytes are provided -- assume it's an int
                tempNum = BitConverter.ToInt32(bytes, 0);
            }
            else
            {
                //Just in case
                throw new Exception(String.Format(Messages.IncorrectNumberOfBytes, decimalPlaces));
            }

            string result = this.AdjustDecimals(tempNum, decimalPlaces);
            isParsedSuccessfully = true;
            return result;
        }

        private string AdjustDecimals(long numericValue, int decimalPlaces)
        {
            if (decimalPlaces <= 0)
            {
                return numericValue.ToString();
            }
            double result = numericValue / Math.Pow(10, decimalPlaces);
            return result.ToString();
        }

        private string Unpack(byte[] packedBytes, int decimalPlaces, out bool isParsedSuccessfully)
        {
            // There is a trick to see the value of the packed number in HEX (Base16). 
            // For example, if you look at packed number "38489" in HEX you will see "00-00-00-38-48-9C. 
            // If you remove dashes, you can actually see "38489" number followed by "C". 
            // "C" indicates it's a positive number and it must always come last in the second half of the last byte.
            // The last nibble may be either "C" (positive), "D" (negative) or "F" (unsigned). 
            // Just uncomment the following line and put a break point on it...
            // string hexString = ParserUtilities.ConvertBytesToStringBase16(packedBytes);

            if (packedBytes.All(p => p == 0x00 || p == 0xFF))
            {
                //Every byte is either 0x00 or 0xFF (fillers)
                isParsedSuccessfully = true;
                return string.Empty;
            }

            long lo = 0;
            long mid = 0;
            long hi = 0;
            bool isNegative;

            // this nybble stores only the sign, not a digit.  
            // "0x0C" hex is positive, "0x0D" hex is negative, and "0x0F" hex is unsigned. 
            switch (this.Nibble(packedBytes, 0))
            {
                case 0x0D: 
                    isNegative = true;
                    break;
                case 0x0F:
                case 0x0C:
                    isNegative = false;
                    break;
                default:
                    isParsedSuccessfully = false;
                    return this.ConvertEbcdicString(packedBytes);
            }
            long intermediate;
            long carry;
            long digit;
            for (int j = packedBytes.Length * 2 - 1; j > 0; j--)
            {
                // multiply by 10
                intermediate = lo * 10;
                lo = intermediate & 0xffffffff;
                carry = intermediate >> 32;
                intermediate = mid * 10 + carry;
                mid = intermediate & 0xffffffff;
                carry = intermediate >> 32;
                intermediate = hi * 10 + carry;
                hi = intermediate & 0xffffffff;
                carry = intermediate >> 32;
                // By limiting input length to 14, we ensure overflow will never occur

                digit = this.Nibble(packedBytes, j);
                if (digit > 9)
                {
                    isParsedSuccessfully = false;
                    return this.ConvertEbcdicString(packedBytes);
                }
                intermediate = lo + digit;
                lo = intermediate & 0xffffffff;
                carry = intermediate >> 32;
                if (carry > 0)
                {
                    intermediate = mid + carry;
                    mid = intermediate & 0xffffffff;
                    carry = intermediate >> 32;
                    if (carry > 0)
                    {
                        intermediate = hi + carry;
                        hi = intermediate & 0xffffffff;
                        carry = intermediate >> 32;
                        // carry should never be non-zero. Back up with validation
                    }
                }
            }

            decimal result = new Decimal((int)lo, (int)mid, (int)hi, isNegative, (byte)decimalPlaces);
            isParsedSuccessfully = true;
            return result.ToString();
        }

        private int Nibble(byte[] packedBytes, int nibbleNo)
        {
            int b = packedBytes[packedBytes.Length - 1 - nibbleNo / 2];
            return (nibbleNo % 2 == 0) ? (b & 0x0000000F) : (b >> 4);
        }

        public XElement ToXml(bool includeSrcBytesInHex = false)
        {
            XElement element = new XElement(Fields.XmlFields);
            element.Add(new XAttribute(Fields.Name, this.Template.FieldName));

            if (includeSrcBytesInHex)
            {
                element.Add(new XAttribute(Fields.SrcHex, this.OriginalBytesBase16)); //For testing
            }

            if (this.Template.Type != FieldType.StringEncIbm935)
            {
                element.Value = ParserUtilities.RemoveNonAsciiChars(this.Text);
            }
            else
            {
                element.Add(this.Text);
            }
            return element;
        }
    }
}
