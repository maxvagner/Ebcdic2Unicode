using Ebcdic2Unicode.Constants;
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
            string fieldName = ParserUtilities.GetCompulsoryAttributeValue(fieldTemplateXml, Fields.Name);
            FieldType fieldType = ParserUtilities.GetFieldType(fieldTemplateXml, Fields.Type);
            int startPosition = ParserUtilities.GetAttributeNumericValue(fieldTemplateXml, Fields.StartPosition);
            int fieldSize = ParserUtilities.GetAttributeNumericValue(fieldTemplateXml, Fields.Size);
            string decimalPlacesStr = ParserUtilities.GetNullableAttributeValue(fieldTemplateXml, Fields.DecimalPlaces);

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
                throw new ArgumentNullException(Messages.FieldNameRequired);
            }
            if (startPosition < 0)
            {
                throw new ArgumentOutOfRangeException(String.Format(Messages.NegativeStartPosition, fieldName));
            }
            if (fieldSize <= 0)
            {
                throw new ArgumentOutOfRangeException(String.Format(Messages.InvalidFieldSize, fieldName));
            }
            if (fieldType == FieldType.BinaryNum)
            {
                if (fieldSize != 1 && fieldSize != 2 && fieldSize != 4)
                {
                    throw new ArgumentException(String.Format(Messages.InvalidInputBytes, fieldName, fieldSize));
                }
            }
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException(String.Format(Messages.NegativeDecimalPlaces, fieldName));
            }
        }


        public XElement GetFieldTemplateXml()
        {
            XElement fieldXml = new XElement(Fields.XmlFieldTemplate);
            fieldXml.Add(new XAttribute(Fields.Name, this.FieldName));
            fieldXml.Add(new XAttribute(Fields.Type, this.Type));
            fieldXml.Add(new XAttribute(Fields.StartPosition, this.StartPosition));
            fieldXml.Add(new XAttribute(Fields.Size, this.FieldSize));

            if (this.DecimalPlaces > 0)
            {
                fieldXml.Add(new XAttribute(Fields.DecimalPlaces, this.DecimalPlaces));
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
