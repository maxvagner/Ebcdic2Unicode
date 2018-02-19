using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ebcdic2Unicode.UnitTests
{
    public static class TestData
    {
        public const string FieldReservationNumber = "RESERVATION-NUMBER";
        public const string FieldCheckInDate = "CHECKIN-DATE";
        public const string FieldCalcNetAmount = "CALC-NET-AMOUNT";
        public const string FieldCustomerName = "CUSTOMER-NAME";
        public const string FieldRunDate = "RUNDATE";
        public const string FieldCurrencyConvRate = "CURRENCY-CONV-RATE";
        public const string FieldUsDollarAmountDue = "US-DOLLAR-AMOUNT-DUE";
        public const string FieldDateOfBirth = "DATE-OF-BIRTH";

        public static LineTemplate GetLineTemplateToParseEbcidicDataWithFixedRecordLength()
        {
            var lineTemplate = new LineTemplate(73, "ReservationsData");
            lineTemplate.AddFieldTemplate(new FieldTemplate(FieldReservationNumber, FieldType.String, 0, 11));
            lineTemplate.AddFieldTemplate(new FieldTemplate(FieldCheckInDate, FieldType.DateString, 11, 6));
            lineTemplate.AddFieldTemplate(new FieldTemplate(FieldCalcNetAmount, FieldType.BinaryNum, 17, 4, 2));
            lineTemplate.AddFieldTemplate(new FieldTemplate(FieldCustomerName, FieldType.String, 21, 30));
            lineTemplate.AddFieldTemplate(new FieldTemplate(FieldRunDate, FieldType.DateStringMMDDYY, 51, 6));
            lineTemplate.AddFieldTemplate(new FieldTemplate(FieldCurrencyConvRate, FieldType.Packed, 57, 6, 6));
            lineTemplate.AddFieldTemplate(new FieldTemplate(FieldUsDollarAmountDue, FieldType.Packed, 63, 6, 2));
            lineTemplate.AddFieldTemplate(new FieldTemplate(FieldDateOfBirth, FieldType.PackedDate, 69, 4));
            return lineTemplate;
        }

        public static LineTemplate GetLineTemplateToParseEbcidicDataWithFixedRecordLengthFromXml()
        {
            var xmlString = GetLineTemplateToParseEbcidicDataWithFixedRecordLength().GetLineTemplateXmlString();
            //"<lineTemplate Name=\"ReservationsData\" Length=\"73\">" +
            //"  <fields>" +
            //"    <fieldTemplate Name=\""+ FieldReservationNumber + "\" Type=\"String\" StartPosition=\"0\" Size=\"11\" />" +
            //"    <fieldTemplate Name=\""+ FieldCheckInDate + "\" Type=\"DateString\" StartPosition=\"11\" Size=\"6\" />" +
            //"    <fieldTemplate Name=\""+ FieldCalcNetAmount + "\" Type=\"BinaryNum\" StartPosition=\"17\" Size=\"4\" DecimalPlaces=\"2\" />" +
            //"    <fieldTemplate Name=\""+ FieldCustomerName + "\" Type=\"String\" StartPosition=\"21\" Size=\"30\" />" +
            //"    <fieldTemplate Name=\""+ FieldRunDate + "\" Type=\"DateStringMMDDYY\" StartPosition=\"51\" Size=\"6\" />" +
            //"    <fieldTemplate Name=\""+ FieldCurrencyConvRate + "\" Type=\"Packed\" StartPosition=\"57\" Size=\"6\" DecimalPlaces=\"6\" />" +
            //"    <fieldTemplate Name=\""+ FieldUsDollarAmountDue + "\" Type=\"Packed\" StartPosition=\"63\" Size=\"6\" DecimalPlaces=\"2\" />" +
            //"    <fieldTemplate Name=\""+ FieldDateOfBirth + "\" Type=\"PackedDate\" StartPosition=\"69\" Size=\"4\" />" +
            //"  </fields>" +
            //"</lineTemplate>";

            var lineTemplateXml = XElement.Parse(xmlString);

            return new LineTemplate(lineTemplateXml);
        }


        /// <summary>
        /// Sample EBCDIC binary data.
        /// </summary>
        /// <returns></returns>
        public static byte[] GetSampleEbcidicDataWithFixedRecordLength()
        {
            var sb = new StringBuilder();

            //First record
            sb.Append("F0-F4-F4-F1-F6-F3-F6-F5-E4-E2-F2"); //RESERVATION-NUMBER (Type="String"): 04416365US2
            sb.Append("F1-F5-F0-F1-F2-F3"); //CHECKIN-DATE (Type="DateString"): 2015-01-23
            sb.Append("00-00-AA-DB");       //CALC-NET-AMOUNT (Type="BinaryNum"): 437.39
            sb.Append("D2-C1-E8-40-D2-C5-D5-C7-6B-D3-D6-E6-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40"); //CUSTOMER-NAME (Type="String"): KAY KENG,LOW
            sb.Append("F0-F1-F2-F8-F1-F5"); //RUNDATE (Type="DateStringMMDDYY"): 2015-01-28
            sb.Append("00-00-07-62-72-8F"); //CURRENCY-CONV-RATE (Type="Packed"): 0.762728
            sb.Append("00-00-00-22-02-6C"); //US-DOLLAR-AMOUNT-DUE (Type="Packed"): 220.26
            sb.Append("07-20-80-9C");       //DATE-OF-BIRTH (Type="PackedDate"): 1972-08-09

            //Second record
            sb.Append("F0-F4-F4-F8-F2-F9-F3-F3-E2-C7-F0"); //04482933SG0
            sb.Append("F1-F5-F0-F1-F2-F4"); //2015-01-24
            sb.Append("00-00-42-35");       //169.49
            sb.Append("C8-D6-6B-D9-D6-E8-E2-E3-D6-D5-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40-40"); //HO,ROYSTON
            sb.Append("F0-F1-F2-F8-F1-F5"); //2015-01-28
            sb.Append("00-00-07-62-72-8F"); //0.762728
            sb.Append("00-00-00-12-08-1C"); //120.81
            sb.Append("07-50-62-3C");       //1975-06-23

            //Third record
            sb.Append("F0-F4-F4-F2-F8-F3-F2-F6-E2-C7-F0"); //04428326SG0
            sb.Append("F1-F5-F0-F1-F2-F6"); //2015-01-26
            sb.Append("00-00-D2-A8");       //539.28
            sb.Append("E5-C1-E9-C8-C1-D7-D7-C1-D3-D3-E8-6B-D4-C1-D5-E4-C5-D3-40-C6-D9-C1-D5-C3-C9-E2-40-40-40-40"); //VAZHAPPALLY,MANUEL FRANCIS
            sb.Append("F0-F1-F2-F8-F1-F5"); //2015-01-28
            sb.Append("00-00-07-62-72-8F"); //0.762728
            sb.Append("00-00-00-38-48-9C"); //384.89
            sb.Append("06-70-50-2C");       //1967-05-02

            byte[] ebcdicBytes = ParserUtilities.ConvertHexStringToByteArray(sb.ToString().Replace("-", ""));
            return ebcdicBytes;
        }

    }
}
