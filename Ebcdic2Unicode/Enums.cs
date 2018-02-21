using System;

namespace Ebcdic2Unicode
{
    /// <summary>FieldType provides instruction to the parser how to process source bytes for a given field</summary>
    public enum FieldType {
        /// <summary>"Normal" EBCDIC text (IBM037 encoding) - 1 byte per char</summary>
        String,

        /// <summary>"Normal" EBCDIC text representing a number (either signed or unsigned)</summary>
        NumericString,

        /// <summary>"Normal" EBCDIC text representing a date in YYYYMMDD, YYMMDD format or CYYMMDD format (C is Century)</summary>
        DateString,

        /// <summary>"Normal" EBCDIC text representing a date in MMddyy format (American date format)</summary>
        DateStringMMDDYY, 

        /// <summary>COMP-3 packed binary number (1 number is encoded by 4 bits. 1 byte contains 2 numbers)</summary>
        Packed,

        /// <summary>COMP-3 Value in CYYMMDD format (C is Century) (e.g. 1140608 => 2014-06-08) </summary>
        PackedDate,

        /// <summary>1, 2, or 4 bytes representing Byte, UInt16, or Int32 respectively </summary>
        BinaryNum,

        /// <summary>Source bytes converted to Hex string (base 16) </summary>
        SourceBytesBase16,

        /// <summary>Source bytes converted to decimal string (base 10) </summary>
        SourceBytesBase10, 

        /// <summary>Source bytes converted to binary string (base 2). Sometimes data is encoded on bit level (i.e. 1 byte may represent 8 true/false values)</summary>
        SourceBytesBase2,    

        /// <summary>EBCDIC text (IBM935 encoding). English: 1 byte per char. Chinese: 2 bytes per char</summary>
        StringEncIbm935,

        /// <summary>ASCII text</summary>
        StringUnicode
    }
}
