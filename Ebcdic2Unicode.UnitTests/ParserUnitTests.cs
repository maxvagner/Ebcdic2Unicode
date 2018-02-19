using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ebcdic2Unicode.UnitTests
{
    [TestClass]
    public class ParserUnitTests
    {

        private byte[] sourceBytes;
        private LineTemplate lineTemplate;
        private EbcdicParser parser;

        [TestInitialize]
        public void Init()
        {
            sourceBytes = TestData.GetSampleEbcidicDataWithFixedRecordLength();
            lineTemplate = TestData.GetLineTemplateToParseEbcidicDataWithFixedRecordLength();
            parser = new EbcdicParser();
        }

        [TestMethod]
        public void ParseAllLines_ParsesTestData()
        {
            ParsedLine[] parsedLines = parser.ParseAllLines(lineTemplate, sourceBytes);

            Assert.IsNotNull(parsedLines);
            Assert.IsTrue(parsedLines.Any());

            var firstRecord = parsedLines.First();
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
