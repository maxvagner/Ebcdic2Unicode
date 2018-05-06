using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ebcdic2Unicode.UnitTests
{
    [TestClass]
    public class FieldTemplateValidationTests
    {
        [TestMethod]
        public void FieldTemplate_InvalidFieldName_ThowsException()
        {
            // ASSERT:
            // Field name cannot be null or empty

            FieldTemplate fieldTemplate = null;

            try
            {
                Debug.WriteLine("Testing empty field name");
                fieldTemplate = new FieldTemplate(fieldName: "", fieldType: FieldType.String, startPosition: 0, fieldSize: 1);
                Assert.IsTrue(false, "No exception was thrown");
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.IsNull(fieldTemplate);

            try
            {
                Debug.WriteLine("Testing null field name");
                fieldTemplate = new FieldTemplate(fieldName: null, fieldType: FieldType.String, startPosition: 0, fieldSize: 1);
                Assert.IsTrue(false, "No exception was thrown");
            }
            catch (ArgumentNullException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.IsNull(fieldTemplate);
        }

        [TestMethod]
        public void FieldTemplate_InvalidFieldStartPosition_ThowsException()
        {
            // ASSERT:
            // Field start position cannot be negative

            FieldTemplate fieldTemplate = null;

            try
            {
                Debug.WriteLine("Testing field start position");
                fieldTemplate = new FieldTemplate(fieldName: "TEST", fieldType: FieldType.String, startPosition: -1, fieldSize: 1);
                Assert.IsTrue(false, "No exception was thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.IsNull(fieldTemplate);
        }

        [TestMethod]
        public void FieldTemplate_InvalidFieldSize_ThowsException()
        {
            // ASSERT:
            // Field size needs to be grater than zero

            FieldTemplate fieldTemplate = null;

            try
            {
                Debug.WriteLine("Testing field size");
                fieldTemplate = new FieldTemplate(fieldName: "TEST", fieldType: FieldType.String, startPosition: 0, fieldSize: 0);
                Assert.IsTrue(false, "No exception was thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.IsNull(fieldTemplate);

            try
            {
                Debug.WriteLine("Testing field size");
                fieldTemplate = new FieldTemplate(fieldName: "TEST", fieldType: FieldType.String, startPosition: 0, fieldSize: -1);
                Assert.IsTrue(false, "No exception was thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.IsNull(fieldTemplate);
        }

        [TestMethod]
        public void FieldTemplate_InvalidFieldDecimalPlaces_ThowsException()
        {
            // ASSERT:
            // Field decimal places cannot be less than zero

            FieldTemplate fieldTemplate = null;

            try
            {
                Debug.WriteLine("Testing field decimal places");
                fieldTemplate = new FieldTemplate(fieldName: "TEST", fieldType: FieldType.BinaryNum, startPosition: 1, fieldSize: 1, decimalPlaces: -1);
                Assert.IsTrue(false, "No exception was thrown");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.IsNull(fieldTemplate);
        }

        [TestMethod]
        public void FieldTemplate_InvalidBinaryNumFieldSize_ThowsException()
        {
            // ASSERT:
            // Field BinaryNum field size is expected to be 1, 2 or 4 bytes

            FieldTemplate fieldTemplate = null;

            try
            {
                Debug.WriteLine("Testing BinaryNum field size");
                fieldTemplate = new FieldTemplate(fieldName: "TEST", fieldType: FieldType.BinaryNum, startPosition: 1, fieldSize: 3);
                Assert.IsTrue(false, "No exception was thrown");
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.IsNull(fieldTemplate);
        }

    }
}
