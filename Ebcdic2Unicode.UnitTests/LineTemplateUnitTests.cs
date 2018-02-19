using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Ebcdic2Unicode.UnitTests
{
    [TestClass]
    public class LineTemplateUnitTests
    {
        [TestMethod]
        public void LineTemplate_InitialisedFromXmlCorrectly()
        {
            var lineTemplate1 = TestData.GetLineTemplateToParseEbcidicDataWithFixedRecordLength();
            var lineTemplate2 = TestData.GetLineTemplateToParseEbcidicDataWithFixedRecordLengthFromXml();

            Assert.IsNotNull(lineTemplate1);
            Assert.IsNotNull(lineTemplate2);
            Assert.AreEqual(lineTemplate1.FieldsCount, lineTemplate2.FieldsCount);
            Assert.AreEqual(
                lineTemplate1.GetLineTemplateXmlString(),
                lineTemplate2.GetLineTemplateXmlString());
        }
    }
}
