using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalciteCsv
{
    public class CsvReader : IDisposable
    {
        // TODO: Deal with blank lines and lines with whitespace only

        /// <summary>
        /// The CsvSpec passed when the reader was generated
        /// </summary>
        public CsvSpec Spec = new CsvSpec();
        private TextReader Stream;
        /// <summary>
        /// The headers for the columns, read from the file or from the Spec
        /// </summary>
        public List<string> Headers = new List<string>();
        /// <summary>
        /// The units for the columns, read from the file or from the Spec
        /// </summary>
        public List<string> Units = new List<string>();
        /// <summary>
        /// The line number in the actual text file the reader is on
        /// </summary>
        public int FileLineCount = 0;
        /// <summary>
        /// The line number of actual data read 
        ///   Does not include rows skipped in the Spec and using .SkipRow()
        /// </summary>
        public int DataRowCount = 0;
        private string _LineCache = String.Empty;
        private bool _IsLineSplit = false;
        private List<string> _ColumnsCache = new List<string>();
        private bool _IsDoublesConverted = false;
        private List<double> _ColumnsAsDoublesCache = new List<double>();
        private string _CsvString = String.Empty;


        public CsvReader(TextReader stream, CsvSpec spec)
        {
            int i;
            this.Spec = spec;
            this.Stream = stream;
            // If is a StringReader (does not support BaseStream resetting) then
            // cache the full string. OK for reasonable sized strings...
            if (stream is StringReader)
            {
                string line = String.Empty;
                while (true) {
                    line = stream.ReadLine();
                    if (line != null)
                    {
                        this._CsvString = this._CsvString + line;
                    }
                    else
                    {
                        break;
                    }
                }
                this.Reset();
            }
            // Populate the Header/Unit properties if they are manually defined in csv specification
            if (this.Spec.Headers.Count > 0 && this.Spec.HeaderRow < 0)
            {
                this.Headers = this.Spec.Headers;
            }
            if (this.Spec.Units.Count > 0 && this.Spec.UnitsRow < 0)
            {
                this.Units = this.Spec.Units;
            }
            // If line specified in csv spec, read in line and reset stream
            if (this.Spec.HeaderRow > 0 || this.Spec.UnitsRow > 0)
            {
                string lineBuffer = String.Empty;
                int stopLine = System.Math.Max(this.Spec.HeaderRow, this.Spec.UnitsRow);
                for (i = 1; i <= stopLine; i++)
                {
                    lineBuffer = this.Stream.ReadLine();
                    if (i == this.Spec.HeaderRow)
                    {
                        this.Headers = this.SplitLine(lineBuffer);
                    }
                    if (i == this.Spec.UnitsRow)
                    {
                        this.Units = this.SplitLine(lineBuffer);
                    }
                }
            }
            this.Reset();
            
            // Parse the SkipRowsFormat to get a list of integers - probably should use 
            // something more intelligent than this
            if (this.Spec.RowsToSkip.Count < 1 && this.Spec.RowsToSkipFormat.Length > 0)
            {
                this.Spec.RowsToSkip = CalciteCsv.Helpers.ParseRangeFormat(this.Spec.RowsToSkipFormat);
            }

                        
        }



        /// <summary>
        /// Reads the next line of data into cache, returns true if is another row, false otherwise
        /// </summary>
        public bool ReadNextRow()
        {
            while (this.Spec.RowsToSkip.Contains(this.FileLineCount)) 
            {
                this.SkipRow();
                this.FileLineCount = this.FileLineCount + 1;
            }
            this._LineCache = this.Stream.ReadLine();
            this._IsLineSplit = false;
            this._ColumnsCache.Clear();
            this._IsDoublesConverted = false;
            this._ColumnsAsDoublesCache.Clear();
            if (this._LineCache == null)
            {
                return false;
            }
            else
            {
                // Only advance the line number if a line was read
                this.DataRowCount = this.DataRowCount + 1;
                this.FileLineCount = this.FileLineCount + 1;
                return true;
            }
        }

        /// <summary>
        /// Is a list of strings containing all the column values for this line
        /// </summary>
        public List<string> Columns 
        {
            get 
            {
                if (this._IsLineSplit == false) 
                {
                    this._ColumnsCache = this.SplitLine(this._LineCache);
                    this._IsLineSplit = true;
                }
                return this._ColumnsCache;
            }
        }

        /// <summary>
        /// Is a list of doubles containing all the column values for this line
        /// Unparseable values are given as 0.0
        /// </summary>
        public List<double> ColumnsAsDoubles 
        {
            get 
            {
                if (this._IsDoublesConverted == false)
                {
                    double result;
                    foreach (string element in this.Columns)
                    {
                        if (System.Double.TryParse(element, out result))
                        {
                            this._ColumnsAsDoublesCache.Add(result);
                        }
                        else
                        {
                            this._ColumnsAsDoublesCache.Add(0.0);
                        }
                    }
                    this._IsDoublesConverted = true;
                }
                return this._ColumnsAsDoublesCache;
            }
        }
        
        /// <summary>
        /// Move on a row without reading any data
        /// </summary>
        public void SkipRow()
        {
            this.FileLineCount = this.FileLineCount + 1;
            this.Stream.ReadLine();
        }

        /// <summary>
        /// Parse the text string into a List of strings using the CsvSpec of the object
        /// </summary>
        /// <param name="line">Text to be parsed</param>
        /// <returns>List of strings</returns>
        internal List<string> SplitLine(string line)
        {
            
            int i;
            bool inQuotes = false;
            bool ignoreNext = false;
            if (this.Spec.IsFixedWidth == true && this.Spec.FixedWidthFormat != String.Empty)
            {
                // TODO: Handle fixed width formats
                return new List<string>();
            }
            else
            {
                // Most times will not require parsing of this special strings
                if (line.Contains(this.Spec.EscapeString)
                    || line.Contains(this.Spec.CommentString)
                    || line.Contains(this.Spec.QuoteString))
                {
                    List<char> elementBuffer = new List<char>();
                    List<string> lineElements = new List<string>();
                    // Cache the lengths to avoid overhead each time
                    int lenEscapeString = this.Spec.EscapeString.Length;
                    int lenQuoteString = this.Spec.QuoteString.Length;
                    int lenColumnDelimiter = this.Spec.ColumnDelimiter.Length;
                    int lenCommentString = this.Spec.CommentString.Length;

                    // Iterate char by char of the string
                    for (i = 0; i < line.Length; ++i)
                    {
                        // First test for escape sequence ...
                        if (line.Length >= i + lenEscapeString &&
                            line.Substring(i, lenEscapeString) == this.Spec.EscapeString)
                        {
                            if (ignoreNext == false)
                            {
                                ignoreNext = true;
                                // Advance past the sequence
                                i = i + lenEscapeString - 1;
                            }
                            else
                            {
                                elementBuffer.Add(line[i]);
                                ignoreNext = false;
                            }
                        }
                        // .. then for the quote sequence ...
                        else if (line.Length >= i + lenQuoteString && 
                            line.Substring(i, lenQuoteString) == this.Spec.QuoteString)
                        {
                            if (ignoreNext == false)
                            {
                                // Toggle the quotes (why isn't .Toggle() a bool method?)
                                if (inQuotes == true)
                                {
                                    inQuotes = false;
                                }
                                else
                                {
                                    inQuotes = true;
                                }
                                i = i + lenQuoteString - 1;
                            }
                            else
                            {
                                elementBuffer.Add(line[i]);
                                ignoreNext = false;
                            }

                        }
                        // ... then for comment sequence ...
                        else if (line.Length >= i + lenCommentString && 
                            line.Substring(i, lenCommentString) == this.Spec.CommentString)
                        {
                            if (ignoreNext == false && inQuotes == false)
                            {
                                break;
                            }
                            else
                            {
                                elementBuffer.Add(line[i]);
                                ignoreNext = false;
                            }
                        }
                        // .. then for column delimiters ...
                        else if (line.Length >= i + lenColumnDelimiter && 
                            line.Substring(i, lenColumnDelimiter) == this.Spec.ColumnDelimiter)
                        {
                            if (ignoreNext == false && inQuotes == false)
                            {
                                lineElements.Add(new string(elementBuffer.ToArray<char>()));
                                elementBuffer.Clear();
                                i = i + lenColumnDelimiter - 1;
                            }
                            else
                            {
                                elementBuffer.Add(line[i]);
                                ignoreNext = false;
                            }

                        }
                        // .. finally since it is not special, add the character to the buffer
                        else
                        {
                            elementBuffer.Add(line[i]);
                            ignoreNext = false;
                        }

                    }
                    lineElements.Add(new string(elementBuffer.ToArray<char>()));
                    return lineElements.ToList<string>();
                }
                // bypass all of the above for a quicker(?) route
                else
                {
                    return line.Split(new string[] { this.Spec.ColumnDelimiter }, StringSplitOptions.None).ToList<string>();
                }
            }
        }

        /// <summary>
        /// Reset the CsvReader back to the beginning of the stream
        /// </summary>
        public void Reset()
        // TODO: Test this!
        {
            if (this.Stream is StringReader)
            {
                TextReader stringStream = new StringReader(this._CsvString);
                this.Stream = stringStream;
            }
            else if (this.Stream is StreamReader)
            {
                StreamReader stream = this.Stream as StreamReader;
                stream.BaseStream.Position = 0;
                stream.DiscardBufferedData();
                this.Stream = stream as TextReader;
            }
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            this.Stream.Dispose();
        }

        #endregion
    }
}
