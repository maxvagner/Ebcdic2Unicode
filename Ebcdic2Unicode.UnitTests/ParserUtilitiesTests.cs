using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ebcdic2Unicode.UnitTests
{
    [TestClass]
    public class ParserUtilitiesTests
    {
        readonly byte[] sourceBytes = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15};


        [TestMethod]
        public void ParserUtilities_ReadBytesRange_ReturnsSlicedArray()
        {
            // ASSERT:
            // Method returns a slice of the input array

            var input = sourceBytes;
            byte[] result = ParserUtilities.ReadBytesRange(input, startPosition: 2, length: 5);
            byte[] expected = { 2, 3, 4, 5, 6};

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.Length == expected.Length);

            for (int i = 0; i < result.Length; i++)
            {
                Assert.IsTrue(result[i] == expected[i]);
            } 
        }
    }
}
