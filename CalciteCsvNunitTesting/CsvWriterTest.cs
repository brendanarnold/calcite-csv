using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using CalciteCsv;

namespace CalciteCsvNunitTesting
{
    [TestFixture]
    class CsvWriterTest
    {
        /// <summary>
        ///A test for CsvWriter Constructor
        ///</summary>
        [Test]
        public void CsvWriterConstructorTest()
        {
            CsvSpec spec = new CsvSpec(CsvTypes.TabSeperatedFile);
            Console.WriteLine(System.Environment.CurrentDirectory);
            // Test when constructing using a StringWriter instance
            StringWriter stringStream = new StringWriter();
            CsvWriter stringTarget = new CsvWriter(stringStream, spec);
            Assert.That(stringTarget, Is.TypeOf(typeof(CalciteCsv.CsvWriter)), "Constructor from StringWriter does not function");
            stringTarget.EndWriting();

            // Test when constructing using a StreamWriter instance
            StreamWriter streamStream = new StreamWriter(Variables.TabSeparatedFileBasicDosFilename);
            CsvWriter streamTarget = new CsvWriter(streamStream, spec);
            Assert.That(streamTarget, Is.TypeOf(typeof(CalciteCsv.CsvWriter)), "Constructor from StreamWriter does not function");
            streamTarget.EndWriting();

        }


        /// <summary>
        ///A test for Output
        ///</summary>
        [Test]
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
                + "Some uncommented text" + System.Environment.NewLine;
            Assert.AreEqual(expected, actual, "CsvWriter does not generate expected string");
            stringTarget.EndWriting();

        }



        [Test]
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
            stringTarget.EndWriting();
        }

        [Test]
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
            stringTarget.EndWriting();

            // Test behaviour of fixed width without delimiter
            spec = new CsvSpec(CsvTypes.FixedWidthFile, new List<int>() { 5, 5, 6, 7 });
            spec.ColumnDelimiter = "  ";
            spec.IsFixedWidthDelimiter = false;
            stringStream = new StringWriter();
            stringTarget = new CsvWriter(stringStream, spec);
            actual = stringTarget.JoinLine(new List<string>() { "foooooo", "barrrr", "flim", "flam" });
            expected = "foooobarrrflim  flam   ";
            Assert.AreEqual(actual, expected, "JoinLine with fixed width specification and no delimiter does not match expected output");
            stringTarget.EndWriting();
        }



    }
}
