using CalciteCsv;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;

namespace CalciteCsvTesting
{
    
    
    /// <summary>
    ///This is a test class for CsvReaderTest and is intended
    ///to contain all CsvReaderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CsvReaderTest
    {


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
        ///A test for SplitLine
        ///</summary>
        [TestMethod()]
        public void SplitLineTest()
        {
            // stream and spec are necessary to instantiate a CsvReader instance
            StreamReader stream = new StreamReader("C:\\PhD\\code\\misc\\Calcite\\CalciteCsvTesting\\TestFiles\\TabSeperatedFileBasic.DAT");
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            
            // First try a simple test
            CsvReader target = new CsvReader(stream, spec);
            string line = "flim,flam,flop";
            List<string> expected = new List<string>() {"flim", "flam", "flop"}; // TODO: Initialize to an appropriate value
            List<string> actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual);

            // More difficult, should result in single line (is comma spec)
            line = "flim\tflam\tflop";
            expected = new List<string>() { "flim\tflam\tflop" };
            actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual);

            // Test the comment parsing
            line = "flim,flam#,flop";
            expected = new List<string>() { "flim", "flam" };
            actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual);

            // Test the escaping
            // TODO: Fix escaping in CsvReader!
            line = "flim,flam\\#,flop";
            expected = new List<string>() { "flim", "flam\\#", "flop" };
            actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual);

            // TODO: Test quoting
            // TODO: Test multiline
            // TODO: Test combination of above
            // TODO: Test esoteric charaters

        }



        /// <summary>
        ///A test for CsvReader Constructor
        ///</summary>
        [TestMethod()]
        public void CsvReaderConstructorTest()
        {
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            // Try intialising with StreamReader
            StreamReader stream = new StreamReader("C:\\PhD\\code\\misc\\Calcite\\CalciteCsvTesting\\TestFiles\\TabSeperatedFileBasic.DAT");
            CsvReader target = new CsvReader(stream, spec);
            Assert.IsInstanceOfType(target, typeof(CalciteCsv.CsvReader));
            // Try initialising with StringReader
            StringReader stringStream = new StringReader("flim flam flop");
            CsvReader stringTarget = new CsvReader(stringStream, spec);
            Assert.IsInstanceOfType(stringTarget, typeof(CalciteCsv.CsvReader));
        }
    }
}
