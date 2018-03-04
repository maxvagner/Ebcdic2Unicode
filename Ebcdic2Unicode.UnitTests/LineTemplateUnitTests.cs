using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Xml.Linq;

namespace Ebcdic2Unicode.UnitTests
{
    [TestClass]
    public class LineTemplateUnitTests
    {
        [TestMethod]
        public void LineTemplate_InitialisedFromXmlCorrectly()
        {
            // ASSERT:
            // LineTemplate object can be converted to XML and also initialized from XML file.

            var lineTemplate1 = TestData.GetLineTemplateToParseEbcidicDataWithFixedRecordLength();

            // Convert sample template to XML
            var xmlString = lineTemplate1.GetLineTemplateXmlString();

            // Initialize template from XML
            var lineTemplateXml = XElement.Parse(xmlString);
            var lineTemplate2 = new LineTemplate(lineTemplateXml);

            Assert.IsNotNull(lineTemplate1);
            Assert.IsNotNull(lineTemplate2);
            Assert.AreEqual(lineTemplate1.FieldsCount, lineTemplate2.FieldsCount);
            Assert.AreEqual(
                lineTemplate1.GetLineTemplateXmlString(),
                lineTemplate2.GetLineTemplateXmlString());
        }
    }
}
