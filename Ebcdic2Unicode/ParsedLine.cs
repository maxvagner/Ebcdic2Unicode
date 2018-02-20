using Ebcdic2Unicode.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Ebcdic2Unicode
{
    public class ParsedLine
    {
        public LineTemplate Template { get; private set; }
        public Dictionary<string, ParsedField> ParsedFields = new Dictionary<string, ParsedField>();

        public string this[string fieldName]
        {
            get
            {
                ParsedField field = this.ParsedFields[fieldName];
                return field.Text.Trim();
            }
        }

        public string this[int fieldIndex]
        {
            get
            {
                int count = 0;
                foreach (ParsedField parsedField in this.ParsedFields.Values)
                {
                    if (count == fieldIndex)
                    {
                        return parsedField.Text.Trim();
                    }
                    count++;
                }
                throw new IndexOutOfRangeException();
            }
        }



        //Constructor
        public ParsedLine(LineTemplate template, byte[] lineBytes)
        {
            this.Template = template;
            this.ParseLine(lineBytes, template);
        }


        private void ParseLine(byte[] lineBytes, LineTemplate lineTemplate)
        {
            this.ValidateInputParameters(lineBytes, lineTemplate);

            foreach (var fieldTemplate in lineTemplate.FieldTemplates)
            {
                ParsedField parsedField = new ParsedField(lineBytes, lineTemplate.FieldTemplates[fieldTemplate.Key]);
                this.ParsedFields.Add(fieldTemplate.Key, parsedField);
            }
        }

        private void ValidateInputParameters(byte[] lineBytes, LineTemplate template)
        {
            if (lineBytes == null)
            {
                throw new ArgumentNullException(Messages.LineBytesRequired);
            }
            if (lineBytes.Length < template.LineSize)
            {
                //TODO:Do something maybe??
                Console.WriteLine(String.Format("Bytes provided: {0}, line size: {1}", lineBytes.Length, template.LineSize));
            }
            if (template == null)
            {
                throw new ArgumentNullException(Messages.LineTemplateRequired);
            }
            if (template.FieldsCount == 0)
            {
                throw new Exception(Messages.FieldTemplatesNotDefined);
            }
        }

        public string[] GetFieldValues(bool addQuotes)
        {
            var query = (from f in ParsedFields.Values
                         select String.Format("{1}{0}{1}", f.Text.Replace("\"", "").Trim(), addQuotes ? "\"" : String.Empty)).ToArray();
            return query;
        }

        public string GetField(string fieldName)
        {
            if (String.IsNullOrWhiteSpace(fieldName))
            {
                throw new Exception(Messages.InvalidFieldName);
            }
            return this.ParsedFields[fieldName].Text.Trim();
        }

        public string GetField(string fieldName, bool surroundWithQuotes)
        {
            return String.Format("\"{0}\"", this.GetField(fieldName));
        }

        public string GetField(string fieldName, bool surroundWithQuotes, int paddedLength)
        {
            if (paddedLength < 0)
            {
                paddedLength = 0;
            }

            if (surroundWithQuotes)
            {
                return String.Format("\"{0}\"", this.GetField(fieldName).PadRight(paddedLength));
            }
            else
            {
                return this.GetField(fieldName).PadRight(paddedLength);
            }
        }

        public XElement ToXml(bool includeSrcBytesInHex = false)
        {
            XElement lineEl = new XElement(Fields.XmlLine);
            lineEl.Add(new XAttribute(Fields.TemplateName, this.Template.LineTemplateName));

            XElement fields = new XElement(Fields.XmlFields);
            lineEl.Add(fields);

            foreach (ParsedField parsedField in this.ParsedFields.Values)
            {
                fields.Add(parsedField.ToXml(includeSrcBytesInHex));
            }

            return lineEl;
        }

        public string ToXmlString()
        {
            return this.ToXml().ToString();
        }

        public string ToCsvString(bool addQuotes = true, char separator = ',')
        {
            StringBuilder sb = new StringBuilder();
            bool addSeparator = false;

            foreach (ParsedField parsedField in this.ParsedFields.Values)
            {
                if (addSeparator)
                {
                    sb.Append(separator);
                }
                if (addQuotes)
                {
                    sb.Append('"');
                }

                sb.Append(parsedField.Text.Trim());

                if (addQuotes)
                {
                    sb.Append('"');
                }

                addSeparator = true;
            }
            return sb.ToString();
        }
    }
}
