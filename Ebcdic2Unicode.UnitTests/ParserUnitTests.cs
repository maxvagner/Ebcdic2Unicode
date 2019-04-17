using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ebcdic2Unicode.UnitTests
{
    [TestClass]
    public class ParserUnitTests
    {
        [TestMethod]
        public void ParseAllLines_ParsesTestData()
        {
            // ASSERT:
            // Processor can parse the whole file at once.

            var sourceBytes = TestData.GetSampleEbcidicDataWithFixedRecordLength();
            var lineTemplate = TestData.GetLineTemplateToParseEbcidicDataWithFixedRecordLength();

            Assert.IsNotNull(sourceBytes);
            Assert.IsTrue(sourceBytes.Any());

            var processor = new EbcdicParser();
            ParsedLine[] parsedLines = processor.ParseAllLines(lineTemplate, sourceBytes);

            Assert.IsNotNull(parsedLines);
            Assert.IsTrue(parsedLines.Length > 1);

            var parsedLine = parsedLines.First();

            Assert.IsTrue(parsedLine[TestData.FieldReservationNumber].Equals("04416365US2"));
            Assert.IsTrue(parsedLine[TestData.FieldCheckInDate].Equals("2015-01-23"));
            Assert.IsTrue(parsedLine[TestData.FieldCalcNetAmount].Equals("437.39"));
            Assert.IsTrue(parsedLine[TestData.FieldCustomerName].Equals("KAY KENG,LOW"));
            Assert.IsTrue(parsedLine[TestData.FieldRunDate].Equals("2015-01-28"));
            Assert.IsTrue(parsedLine[TestData.FieldCurrencyConvRate].Equals("0.762728"));
            Assert.IsTrue(parsedLine[TestData.FieldUsDollarAmountDue].Equals("220.26"));
            Assert.IsTrue(parsedLine[TestData.FieldDateOfBirth].Equals("1972-08-09"));
        }

        [TestMethod]
        public void ParseSingleLine_ParsesTestData()
        {
            // ASSERT:
            // Processor can parse individual line.

            var sourceBytes = TestData.GetSampleEbcidicDataWithFixedRecordLength();
            var lineTemplate = TestData.GetLineTemplateToParseEbcidicDataWithFixedRecordLength();

            Assert.IsNotNull(sourceBytes);
            Assert.IsTrue(sourceBytes.Any());

            var processor = new EbcdicParser();

            int count = 0;
            int position = 0;

            while (position < sourceBytes.Length)
            {
                byte[] lineBytes = ParserUtilities.ReadBytesRange(
                    sourceBytes, 
                    length: lineTemplate.LineSize, 
                    startPosition: position);

                position += lineTemplate.LineSize;

                var parsedLine = processor.ParseSingleLine(lineTemplate, lineBytes);

                if (count == 0)
                {
                    Assert.IsTrue(parsedLine[TestData.FieldReservationNumber].Equals("04416365US2"));
                    Assert.IsTrue(parsedLine[TestData.FieldCheckInDate].Equals("2015-01-23"));
                    Assert.IsTrue(parsedLine[TestData.FieldCalcNetAmount].Equals("437.39"));
                    Assert.IsTrue(parsedLine[TestData.FieldCustomerName].Equals("KAY KENG,LOW"));
                    Assert.IsTrue(parsedLine[TestData.FieldRunDate].Equals("2015-01-28"));
                    Assert.IsTrue(parsedLine[TestData.FieldCurrencyConvRate].Equals("0.762728"));
                    Assert.IsTrue(parsedLine[TestData.FieldUsDollarAmountDue].Equals("220.26"));
                    Assert.IsTrue(parsedLine[TestData.FieldDateOfBirth].Equals("1972-08-09"));
                }

                Debug.WriteLine(parsedLine.ToXmlString());
                count++;
            }

            Assert.IsTrue(count > 1);
        }

        [Ignore]
        [TestMethod]
        public void Used_For_Manual_Testing()
        {
            var lineBytes = ParserUtilities.ConvertHexStringToByteArray("F1-F5-F0-F1-F2-F3");
            var lineTemplate = new LineTemplate(lineBytes.Length);
            var fieldName = "TEST_ITEM";

            lineTemplate.AddFieldTemplate(
                new FieldTemplate(
                    fieldName: fieldName, 
                    fieldType: FieldType.DateString, 
                    startPosition: 0, 
                    fieldSize: lineBytes.Length,
                    decimalPlaces: 0));

            var processor = new EbcdicParser();

            var parsedLine = processor.ParseSingleLine(lineTemplate, lineBytes);

            var testOutput = parsedLine[fieldName];

            Debug.WriteLine(parsedLine.ToXmlString());
        }
    }
}
