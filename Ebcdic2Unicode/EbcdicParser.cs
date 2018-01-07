using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ebcdic2Unicode
{
    public class EbcdicParser
    {
        public ParsedLine[] ParsedLines { get; private set; }


        #region Constructors

        //Constractor 1
        public EbcdicParser()
        {
            //Empty
        }

        //Constractor 2
        public EbcdicParser(string sourceFilePath, LineTemplate lineTemplate)
            : this(File.ReadAllBytes(sourceFilePath), lineTemplate)
        {
            //Read all file bytes and call 3rd constructor
        }

        //Constractor 3
        public EbcdicParser(byte[] allBytes, LineTemplate lineTemplate)
        {
            this.ParsedLines = this.ParseAllLines(lineTemplate, allBytes);
        }
        #endregion


        public ParsedLine[] ParseAllLines(LineTemplate lineTemplate, byte[] allBytes)
        {
            Console.WriteLine("{0}: Parsing...", DateTime.Now);
            this.ValidateInputParameters(lineTemplate, allBytes, false);

            double expectedRows = (double)allBytes.Length / lineTemplate.LineSize;
            if (expectedRows % 1 == 0)
            {
                Console.WriteLine("{1}: Line count est {0:#,###.00}", expectedRows, DateTime.Now);
            }

            byte[] lineBytes = new byte[lineTemplate.LineSize];
            List<ParsedLine> linesList = new List<ParsedLine>();
            ParsedLine parsedLine = null;
            int lineIndex = 0;

            for (int i = 0; i < allBytes.Length; i += lineTemplate.LineSize)
            {
                try
                {
                    Array.Copy(allBytes, i, lineBytes, 0, lineTemplate.LineSize);

                    parsedLine = this.ParseSingleLine(lineTemplate, lineBytes);

                    if (parsedLine != null)
                    {
                        linesList.Add(parsedLine);
                    }

                    lineIndex++;

                    if (lineIndex % 1000 == 0)
                    {
                        Console.Write(lineIndex + "\r");
                    }
                }
                catch (Exception ex)
                {
                    //Used for dubugging 
                    Console.WriteLine("Exception at line index {0}", lineIndex);
                    throw ex;
                }
            }
            Console.WriteLine("{1}: {0} line(s) have been parsed", linesList.Count(), DateTime.Now);
            return linesList.ToArray();
        }

        public ParsedLine[] ParseAllLines(LineTemplate lineTemplate, string sourceFilePath)
        {
            Console.WriteLine("{1}: Reading {2}...", sourceFilePath, DateTime.Now);
            return this.ParseAllLines(lineTemplate, File.ReadAllBytes(sourceFilePath));
        }

        public ParsedLine ParseSingleLine(LineTemplate lineTemplate, byte[] lineBytes)
        {
            bool isSingleLine = true;
            this.ValidateInputParameters(lineTemplate, lineBytes, isSingleLine);
            ParsedLine parsedLine = new ParsedLine(lineTemplate, lineBytes);
            return parsedLine;
        }

        private bool ValidateInputParameters(LineTemplate lineTemplate, byte[] allBytes, bool isSingleLine)
        {
            if (allBytes == null)
            {
                throw new ArgumentNullException("Ebcdic data is not provided");
            }
            if (lineTemplate == null)
            {
                throw new ArgumentNullException("Line template is not provided");
            }
            if (lineTemplate.FieldsCount == 0)
            {
                throw new Exception("Line template must contain at least one field template");
            }
            if (allBytes.Length > 0 && allBytes.Length < lineTemplate.LineSize)
            {
                throw new Exception("Data length is shorter than the line size");
            }
            if (isSingleLine && allBytes.Length != lineTemplate.LineSize)
            {
                throw new Exception("Bytes count doesn't equal to line size");
            }

            double expectedRows = (double)allBytes.Length / lineTemplate.LineSize;

            if (expectedRows % 1 != 0) //Expected number of rows is not a whole number
            {
                string errMsg = String.Format("Data bytes = {0}; line size = {1}; line count check = {2:#,###.00}.\r\nExpected number of rows is not a whole number. Check line template.", allBytes.Length, lineTemplate.LineSize, expectedRows);
                throw new Exception(errMsg);
            }

            return true;
        }

        public bool SaveParsedLinesAsCsvFile(string outputFilePath, bool includeColumnNames = true, bool addQuotes = true)
        {
            return ParserUtilities.WriteParsedLineArrayToCsv(this.ParsedLines, outputFilePath, includeColumnNames, addQuotes);
        }

        public bool SaveParsedLinesAsTxtFile(string outputFilePath, string delimiter = "\t", bool includeColumnNames = true, bool addQuotes = true)
        {
            return ParserUtilities.WriteParsedLineArrayToTxt(this.ParsedLines, outputFilePath, delimiter, includeColumnNames, addQuotes);
        }

        public bool SaveParsedLinesAsXmlFile(string outputFilePath)
        {
            return ParserUtilities.WriteParsedLineArrayToXml(this.ParsedLines, outputFilePath);
        }
    }
}
