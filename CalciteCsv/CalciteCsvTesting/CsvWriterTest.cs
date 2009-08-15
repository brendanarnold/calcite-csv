using CalciteCsv;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using System;

namespace CalciteCsvTesting
{


    /// <summary>
    ///This is a test class for CsvWriterTest and is intended
    ///to contain all CsvWriterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CsvWriterTest
    {

        string TabSeparatedFileBasicDosFilename = @"..\..\..\CalciteCsvTesting\TestFiles\TabSeperatedFileBasicFromVS.txt";

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for CsvWriter Constructor
        ///</summary>
        [TestMethod()]
        public void CsvWriterConstructorTest()
        {
            CsvSpec spec = new CsvSpec(CsvTypes.TabSeperatedFile);
            Console.WriteLine(System.Environment.CurrentDirectory);
            // Test when constructing using a StringWriter instance
            StringWriter stringStream = new StringWriter();
            CsvWriter stringTarget = new CsvWriter(stringStream, spec);
            Assert.IsInstanceOfType(stringTarget, typeof(CalciteCsv.CsvWriter), "Constructor from StringWriter does not function");

            // Test when constructing using a StreamWriter instance
            StreamWriter streamStream = new StreamWriter(this.TabSeparatedFileBasicDosFilename);
            CsvWriter streamTarget = new CsvWriter(streamStream, spec);
            Assert.IsInstanceOfType(streamTarget, typeof(CalciteCsv.CsvWriter), "Constructor from StreamWriter does not function");

        }


        /// <summary>
        ///A test for Output
        ///</summary>
        [TestMethod()]
        public void OutputTest()
        {
            CsvSpec spec = new CsvSpec(CsvTypes.TabSeperatedFile);
            spec.Headers.AddRange(new List<string>() { "foo", "bar", "flim" });
            spec.ColumnDelimiter = "\t";
            spec.CommentString = "#";
            // Test when constructing using a StringWriter instance
            StringWriter stringStream = new StringWriter();
            CsvWriter stringTarget = new CsvWriter(stringStream, spec);
            stringTarget.Columns.AddRange(new List<string>() { "0.1", "0e-5", "0.0" });
            stringTarget.WriteColumnsLine();
            stringTarget.WriteTextLine("Some commented text", true);
            stringTarget.WriteTextLine("Some uncommented text", false);
            string actual = stringTarget.Output;
            string expected = "foo\tbar\tflim" + System.Environment.NewLine
                + "0.1\t0e-5\t0.0" + System.Environment.NewLine
                + "#Some commented text" + System.Environment.NewLine
                + "Some uncommented text";
            Assert.AreEqual(expected, actual, "CsvWriter does not generate expected string");

        }



        [TestMethod()]
        public void BasicJoinLineTest()
        {
            // Basic test
            CsvSpec spec = new CsvSpec(CsvTypes.TabSeperatedFile);
            spec.ColumnDelimiter = "\t";
            StringWriter stringStream = new StringWriter();
            CsvWriter stringTarget = new CsvWriter(stringStream, spec);
            string actual = stringTarget.JoinLine(new List<string>() { "foo", "bar", "flim" });
            string expected = "foo\tbar\tflim";
            Assert.AreEqual(actual, expected, "Basic JoinLine test did not match");
        }

        [TestMethod()]
        public void FixedWidthJoinLineTest()
        {
            // Test behaviour of fixed width with delimiter
            CsvSpec spec = new CsvSpec(CsvTypes.FixedWidthFile, new List<int>() { 5, 5, 6, 7 });
            spec.ColumnDelimiter = "  ";
            spec.IsFixedWidthDelimiter = true;
            StringWriter stringStream = new StringWriter();
            CsvWriter stringTarget = new CsvWriter(stringStream, spec);
            string actual = stringTarget.JoinLine(new List<string>() { "foooooo", "barrrr", "flim", "flam" });
            string expected = "foo  bar  flim  flam   ";
            Assert.AreEqual(actual, expected, "JoinLine with fixed width specification and delimiter does not match expected output");

            // Test behaviour of fixed width without delimiter
            spec = new CsvSpec(CsvTypes.FixedWidthFile, new List<int>() { 5, 5, 6, 7 });
            spec.ColumnDelimiter = "  ";
            spec.IsFixedWidthDelimiter = false;
            stringStream = new StringWriter();
            stringTarget = new CsvWriter(stringStream, spec);
            actual = stringTarget.JoinLine(new List<string>() { "foooooo", "barrrr", "flim", "flam" });
            expected = "foooobarrrflim  flam   ";
            Assert.AreEqual(actual, expected, "JoinLine with fixed width specification and no delimiter does not match expected output");

        }



    }
}
