﻿using CalciteCsv;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using System;

namespace CalciteCsvTesting
{
    
    
    /// <summary>
    ///This is a test class for CsvReaderTest and is intended
    ///to contain all CsvReaderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CsvReaderTest
    {
        string TabSeparatedFileBasicDosFilename = @"C:\PhD\code\misc\CalciteCsv\CalciteCsvTesting\TestFiles\TabSeperatedFileBasicFromVS.txt";
        string TabSeparatedFileBasicUnixFilename = @"C:\PhD\code\misc\CalciteCsv\CalciteCsvTesting\TestFiles\TabSeperatedFileBasicFromVim.txt";

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
        /// Test CsvReader.SplitLine() behaviour with simple string
        /// </summary>
        [TestMethod()]
        public void SplitLineTestBasic()
        {
            // stream and spec are necessary to instantiate a CsvReader instance
            StreamReader stream = new StreamReader(this.TabSeparatedFileBasicDosFilename);
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            
            // First try a simple test
            CsvReader target = new CsvReader(stream, spec);
            string line = "flim,flam,flop";
            List<string> expected = new List<string>() {"flim", "flam", "flop"};
            List<string> actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed simple parsing of csv string");
        }

        /// <summary>
        /// Test CsvReader.SplitLine() behaviour when given an empty string
        /// </summary>
        [TestMethod()]
        public void SplitLineTestEmpty()
        {
            StreamReader stream = new StreamReader(this.TabSeparatedFileBasicDosFilename);
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            CsvReader target = new CsvReader(stream, spec);

            // See how it handles an empty string
            string line = String.Empty;
            List<string> expected = new List<string>() { "" };
            List<string> actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to deal with an empty line correctly");
        }

        /// <summary>
        /// Test CsvReader.SplitLine() behaviour when given wrong delimiter
        /// </summary>
        [TestMethod()]
        public void SplitLineTestWrongDelimiter() 
        {
            StreamReader stream = new StreamReader(this.TabSeparatedFileBasicDosFilename);
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            spec.ColumnDelimiter = ",";
            CsvReader target = new CsvReader(stream, spec);

            // More difficult, should result in single line (is comma spec)
            string line = "flim\tflam\tflop";
            List<string> expected = new List<string>() { "flim\tflam\tflop" };
            List<string> actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse a non matching string as expected");
        }

        /// <summary>
        /// Test CsvReader.SplitLine() ability to deal with comments
        /// </summary>
        [TestMethod()]
        public void SplitLineTestComment() 
        {
            StreamReader stream = new StreamReader(this.TabSeparatedFileBasicDosFilename);
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            spec.CommentString = "#";
            CsvReader target = new CsvReader(stream, spec);

            // Test the comment parsing
            string line = "flim,flam#,flop";
            List<string> expected = new List<string>() { "flim", "flam" };
            List<string> actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse comments correctly");

            // Test for lnger comment sequences
            spec.CommentString = @"//"; // C style comment sequence
            line = @"flim,flam/,flop//,flaz";
            expected = new List<string>() { "flim", "flam/", "flop" };
            actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse multi character comments correctly");
        }

        /// <summary>
        /// Test CsvReader.SplitLine() ability to deal with escaping characters
        /// </summary>
        [TestMethod()]
        public void SplitLineTestEscaping() 
        {
            StreamReader stream = new StreamReader(this.TabSeparatedFileBasicDosFilename);
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            spec.EscapeString = @"\";
            spec.CommentString = "#";
            CsvReader target = new CsvReader(stream, spec);

            // Test the escaping
            string line = @"flim,flam\#,flop";
            List<string> expected = new List<string>() { "flim", "flam#", "flop" };
            List<string> actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to sucessfully escape a comment character");

            // Test multi-character escape strings
            spec.EscapeString = @"\\";
            line = @"flim,flam\\#,flop";
            expected = new List<string>() { "flim", "flam#", "flop" };
            CollectionAssert.AreEqual(expected, actual, "Failed to sucessfully escape using a multi-character escape sequence");
        }

        /// <summary>
        /// Test CsvReader.SplitLine() ability to handle quoted sequences in string
        /// </summary>
        [TestMethod()]
        public void SplitLineTestQuoting()
        {
            StreamReader stream = new StreamReader(this.TabSeparatedFileBasicDosFilename);
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            spec.QuoteString = "\"";
            spec.ColumnDelimiter = ",";
            spec.CommentString = "#";
            CsvReader target = new CsvReader(stream, spec);

            // Test basic quoting ignores comment characters and column delimiters
            string line = "flim,\"flam#,flop\",flaz";
            List<string> expected = new List<string>() { "flim", "flam#,flop", "flaz" };
            List<string> actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse quoting correctly");

            // Test a more typical usage scenario
            line = "\"flim flam\",\"flam flam\",\"flop flam\"";
            expected = new List<string>() { "flim flam", "flam flam", "flop flam" };
            actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse quoting of all elements correctly");

            // Test using quote strings of length > 1
            spec.QuoteString = @"''";
            line = @"flim,''flam#,flop'',flaz";
            expected = new List<string>() { "flim", "flam#,flop", "flaz" };
            actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse using multi-character quoting correctly");

            // Test using quote strings of length > 1 in a more typical scenario
            line = @"''flim flam'',''flam# flam'',''flop flam''";
            expected = new List<string>() { "flim flam", "flam# flam", "flop flam" };
            actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse using multi-character quoting of all elements correctly");

        }

        /// <summary>
        /// Test CsvReader.SplitLine() with unusual character inputs
        /// </summary>
        [TestMethod()]
        public void SplitLineTestStrangeCharacters()
        {
            // TODO: Implement extended ascii for greek chaarcters and how they are interpreted

            StreamReader stream = new StreamReader(this.TabSeparatedFileBasicDosFilename);
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            spec.QuoteString = "\"";
            CsvReader target = new CsvReader(stream, spec);

            // See how deals with newline character
            string line = "flim\nflam";
            List<string> expected = new List<string>() { "flim\nflam" };
            List<string> actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse newline character correctly");

            // Encode 'f' in unicode to see if correctly parsed
            line = "\u0066lim";
            expected = new List<string>() { "flim" };
            actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse 'f' defined in unicode correctly");

            // Encode some greek characters in unicode to see if correctly parsed
            line = "\u03b8 (T),\u03abchic";
            expected = new List<string>() { "\u03b8 (T)", "\u03abchic" };
            actual = target.SplitLine(line);
            CollectionAssert.AreEqual(expected, actual, "Failed to parse greek unicode characters correctly");

        }


        /// <summary>
        ///A test for CsvReader Constructor
        ///</summary>
        [TestMethod()]
        public void CsvReaderConstructorTest()
        {
            CsvSpec spec = new CsvSpec(CsvTypes.CommaSeperatedFile);
            // Try intialising with StreamReader
            StreamReader stream = new StreamReader(this.TabSeparatedFileBasicDosFilename);
            CsvReader target = new CsvReader(stream, spec);
            Assert.IsInstanceOfType(target, typeof(CalciteCsv.CsvReader));
            // Try initialising with StringReader
            StringReader stringStream = new StringReader("flim flam flop");
            CsvReader stringTarget = new CsvReader(stringStream, spec);
            Assert.IsInstanceOfType(stringTarget, typeof(CalciteCsv.CsvReader));
        }

        /// <summary>
        /// A test for Reset
        /// </summary>
        [TestMethod()]
        public void ResetTest()
        {
            CsvSpec spec = new CsvSpec(CsvTypes.TabSeperatedFile);

            // Test the StreamReader on DOS textfile
            StreamReader stream = new StreamReader(this.TabSeparatedFileBasicDosFilename);
            CsvReader target = new CsvReader(stream, spec);
            target.ReadNextRow(); // Move onto first row
            string[] expected = target.Columns.ToArray();
            target.ReadNextRow(); // Move to second row
            target.Reset();
            target.ReadNextRow(); // Should read first row again
            string[] actual = target.Columns.ToArray();
            CollectionAssert.AreEqual(expected, actual, "First lines of StreamReader on DOS textfile do not match after reset");

            // Test the StreamReader on UNIX textfile
            stream = new StreamReader(this.TabSeparatedFileBasicUnixFilename);
            target = new CsvReader(stream, spec);
            target.ReadNextRow(); // Move onto first row
            expected = target.Columns.ToArray();
            target.ReadNextRow(); // Move to second row
            target.Reset();
            target.ReadNextRow(); // Should read first row again
            actual = target.Columns.ToArray();
            CollectionAssert.AreEqual(expected, actual, "First lines of StreamReader on UNIX text file do not match after reset");

            // Test StringReader
            StringReader stringStream = new StringReader("flim	flam\nflop	flaz");
            CsvReader stringTarget = new CsvReader(stringStream, spec);
            stringTarget.ReadNextRow(); // Move onto first row
            expected = stringTarget.Columns.ToArray();
            stringTarget.ReadNextRow(); // Move to second row
            stringTarget.Reset();
            stringTarget.ReadNextRow(); // Should read first row again
            actual = stringTarget.Columns.ToArray();
            CollectionAssert.AreEqual(expected, actual, "First lines of StringReader do not match after reset");


        }
    }
}
