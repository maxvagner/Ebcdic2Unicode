using System;
using System.Xml.Linq;

namespace Ebcdic2Unicode
{
    public class FieldTemplate
    {
        public string FieldName { get; private set; }
        public FieldType Type { get; private set; }
        public int StartPosition { get; private set; }
        public int FieldSize { get; private set; }
        public int DecimalPlaces { get; private set; }



        //Constructor 1
        public FieldTemplate(string fieldName, FieldType fieldType, int startPosition, int fieldSize, int decimalPlaces = 0)
        {
            this.PopulateAllProperties(fieldName, fieldType, startPosition, fieldSize, decimalPlaces);
        }

        //Constructor 2
        public FieldTemplate(XElement fieldTemplateXml)
        {
            //Input XML: <fieldTemplate Name="RecordType" Type="AlphaNum" StartPosition="13" Size="2" DecimalPlaces="0" />
            string fieldName = ParserUtilities.GetCompulsoryAttributeValue(fieldTemplateXml, "Name");
            FieldType fieldType = ParserUtilities.GetFieldType(fieldTemplateXml, "Type");
            int startPosition = ParserUtilities.GetAttributeNumericValue(fieldTemplateXml, "StartPosition");
            int fieldSize = ParserUtilities.GetAttributeNumericValue(fieldTemplateXml, "Size");
            string decimalPlacesStr = ParserUtilities.GetNullableAttributeValue(fieldTemplateXml, "DecimalPlaces");

            int decimalPlaces = 0;
            if (!String.IsNullOrEmpty(decimalPlacesStr))
            {
                decimalPlaces = Int32.Parse(decimalPlacesStr);
            }

            this.PopulateAllProperties(fieldName, fieldType, startPosition, fieldSize, decimalPlaces);
        }



        private void PopulateAllProperties(string fieldName, FieldType fieldType, int startPosition, int fieldSize, int decimalPlaces)
        {
            this.ValidateInputParameters(fieldName, fieldType, startPosition, fieldSize, decimalPlaces);
            this.FieldName = fieldName.Trim();
            this.Type = fieldType;
            this.StartPosition = startPosition;
            this.FieldSize = fieldSize;
            this.DecimalPlaces = decimalPlaces;
        }

        private void ValidateInputParameters(string fieldName, FieldType fieldType, int startPosition, int fieldSize, int decimalPlaces)
        {
            if (String.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentNullException("Field name is required for a template");
            }
            if (startPosition < 0)
            {
                throw new ArgumentOutOfRangeException(String.Format("Start position cannot be negative for a field template \"{0}\"", fieldName));
            }
            if (fieldSize <= 0)
            {
                throw new ArgumentOutOfRangeException(String.Format("Filed size must be greater than zero for a field template \"{0}\"", fieldName));
            }
            if (fieldType == FieldType.BinaryNum)
            {
                if (fieldSize != 1 && fieldSize != 2 && fieldSize != 4)
                {
                    throw new Exception(String.Format("Incorrect number of bytes provided for a binary field template \"{0}\": {1}", fieldName, fieldSize));
                }
            }
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException(String.Format("Number of decimal places cannot be negative for a field template \"{0}\"", fieldName));
            }
            if (decimalPlaces > 6)
            {
                throw new ArgumentOutOfRangeException(String.Format("Number of decimal places exceeds limit for a field template \"{0}\"", fieldName));
            }
        }


        public XElement GetFieldTemplateXml()
        {
            XElement fieldXml = new XElement("fieldTemplate");
            fieldXml.Add(new XAttribute("Name", this.FieldName));
            fieldXml.Add(new XAttribute("Type", this.Type));
            fieldXml.Add(new XAttribute("StartPosition", this.StartPosition));
            fieldXml.Add(new XAttribute("Size", this.FieldSize));

            if (this.DecimalPlaces > 0)
            {
                fieldXml.Add(new XAttribute("DecimalPlaces", this.DecimalPlaces));
            }

            return fieldXml;
            //Output XML: <fieldTemplate Name="RecordType" Type="AlphaNum" StartPosition="13" Size="2" DecimalPlaces="0" />
        }

        public string GetFieldTemplateXmlString()
        {
            return GetFieldTemplateXml().ToString();
        }
    }
}
