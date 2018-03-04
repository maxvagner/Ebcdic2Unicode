using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ebcdic2Unicode.UnitTests
{
    [TestClass]
    public class ParserUnitTests
    {

        private byte[] sourceBytes;
        private LineTemplate lineTemplate;
        private EbcdicParser processor;

        [TestInitialize]
        public void Init()
        {
            sourceBytes = TestData.GetSampleEbcidicDataWithFixedRecordLength();
            lineTemplate = TestData.GetLineTemplateToParseEbcidicDataWithFixedRecordLength();
        }

        [TestMethod]
        public void ParseAllLines_ParsesTestData()
        {
            // ASSERT:
            // Processor can parse the whole file at once.

            Assert.IsNotNull(sourceBytes);
            Assert.IsTrue(sourceBytes.Any());

            processor = new EbcdicParser();
            ParsedLine[] parsedLines = processor.ParseAllLines(lineTemplate, sourceBytes);

            Assert.IsNotNull(parsedLines);
            Assert.IsTrue(parsedLines.Length > 1);

            AssertDataForTheFirstRecord(parsedLines.First());
        }

        [TestMethod]
        public void ParseSingleLine_ParsesTestData()
        {
            // ASSERT:
            // Processor can parse individual line.

            Assert.IsNotNull(sourceBytes);
            Assert.IsTrue(sourceBytes.Any());

            processor = new EbcdicParser();

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
                    AssertDataForTheFirstRecord(parsedLine);
                }

                Debug.WriteLine(parsedLine.ToXmlString());
                count++;
            }

            Assert.IsTrue(count > 1);
        }

        private void AssertDataForTheFirstRecord(ParsedLine firstRecord)
        {
            Assert.IsTrue(firstRecord[TestData.FieldReservationNumber].Equals("04416365US2"));
            Assert.IsTrue(firstRecord[TestData.FieldCheckInDate].Equals("2015-01-23"));
            Assert.IsTrue(firstRecord[TestData.FieldCalcNetAmount].Equals("437.39"));
            Assert.IsTrue(firstRecord[TestData.FieldCustomerName].Equals("KAY KENG,LOW"));
            Assert.IsTrue(firstRecord[TestData.FieldRunDate].Equals("2015-01-28"));
            Assert.IsTrue(firstRecord[TestData.FieldCurrencyConvRate].Equals("0.762728"));
            Assert.IsTrue(firstRecord[TestData.FieldUsDollarAmountDue].Equals("220.26"));
            Assert.IsTrue(firstRecord[TestData.FieldDateOfBirth].Equals("1972-08-09"));
        }
    }
}
