namespace Ebcdic2Unicode.Constants
{
    public static class Messages
    {
        // EbcdicParser Error Messages
        public const string DataNotProvided = "Ebcdic data is not provided.";
        public const string LineTemplateNotProvided = "Line template is not provided.";
        public const string LineTemplateHasNoFields = "Line template must contain at least one field template.";
        public const string DataShorterThanExpected = "Data length is shorter than the line size.";
        public const string DataLengthDifferentThanExpected = "Bytes count doesn't equal to line size.";
        public const string ExpectedNumberOfRows = "Data bytes = {0}; line size = {1}; line count check = {2:#,###.00}.\r\nExpected number of rows is not a whole number.";

        // FieldTemplate Error Messages
        public const string FieldNameRequired = "Field name is required for a template.";
        public const string NegativeStartPosition = "Start position cannot be negative for a field template \"{0}\".";
        public const string InvalidFieldSize = "Field size must be greater than zero for a field template \"{0}\".";
        public const string InvalidInputBytes = "Incorrect number of bytes provided for a binary field template \"{0}\": {1}";
        public const string NegativeDecimalPlaces = "Number of decimal places cannot be negative for a field template \"{0}\".";
        public const string DecimalPlacesLimitExceeded = "Number of decimal places exceeds limit for a field template \"{0}\".";

        // LineTemplate Error Messages
        public const string LineLengthTooShort = "Line length must be greater than zero.";
        public const string FieldExceedsLineBoundary = "Field \"{0}\" exceeds line boundary.";
        public const string FileAlreadyExists = "File '{0}' alredy exists.";

        // ParsedField Error Messages
        public const string FieldOutsideLineBoundary = "Field \"{0}\" length falls outside the line length.";
        public const string ParserNotImplemented = "Unable to parse field \"{0}\". Parser not implemented for field type \"{1}\".";
        public const string UnableToConvertStringToNumber = "Unable to convert \"{0}\" string to number.";
        public const string IncorrectNumberOfBytes = "Incorrect number of bytes provided for a binary field: {1}.";

        // ParsedLine Error Messages
        public const string LineBytesRequired = "Line bytes required.";
        public const string LineTemplateRequired = "Line template is required.";
        public const string FieldTemplatesNotDefined = "Field templates have not been defined in the line template.";
        public const string InvalidFieldName = "Field name is NULL or empty.";
        public const string xx = ".";
    }
}
