using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CalciteCsv
{
    public class CsvWriter : IDisposable
    {
        /// <summary>
        /// The current CsvSpec that the writer is following
        /// </summary>
        public CsvSpec Spec = new CsvSpec();
        /// <summary>
        /// Number of lines written to the file (incl. headers, units and commented lines)
        /// </summary>
        public int FileLineCount = 0;
        /// <summary>
        /// Number of rows of data written to the file (not incl. headers, units, commented lines)
        /// </summary>
        public int DataRowCount = 0;
        /// <summary>
        /// String to write to the file, n.b. may be trimmed to conform with fixed width specification
        /// </summary>
        public List<string> Columns = new List<string>();
        /// <summary>
        /// Returns the string representation of the assembled CSV file so far
        /// </summary>
        public string Output
        {
            get 
            {
                // TODO: Will this return the string for TextWriter and StreamWriter?
                return this._Stream.ToString();
            }
        }

        private TextWriter _Stream;

        /// <summary>
        /// Generates a CsvWriter instance given a StreamWriter instance
        /// </summary>
        /// <param name="stream">A StreamWriter or StringWriter instance</param>
        /// <param name="spec">CsvSpec instance</param>
        public CsvWriter(TextWriter stream, CsvSpec spec)
        {
            this.Spec = spec;
            this._Stream = stream;
            // TODO: Implement writing of headers 
        }


        /// <summary>
        /// Write the cached column data to the file
        /// </summary>
        public void WriteColumnsLine()
        {
            this.WriteColumnsLine(string.Empty);
        }

        /// <summary>
        /// Write the cached column data to the file
        /// </summary>
        /// <param name="endOfLineComment">A commented string to append at the end of this row of data</param>
        public void WriteColumnsLine(string endOfLineComment)
        {
            string line = String.Empty;
            line = this.JoinLine(this.Columns);
            // Send off to generic text writing function for writing and reset the variables
            this._WriteTextLine(line, false, endOfLineComment);
            // Incremement the number of data lines
            this.DataRowCount = this.DataRowCount + 1;
            // Clear the cache
            this.Columns.Clear();
        }

        // TODO: Make 'internal' when discover how to make NUnit access it
        public string JoinLine(List<string> columns)
        {
            string line = String.Empty;
            // Check that we can work with the current CsvSpec
            this.Spec.Validate();
            // Assemble the string to be written
            if (this.Spec.IsFixedWidth == false)
            {
                line = String.Join(this.Spec.ColumnDelimiter, columns.ToArray());
            }
            else
            {
                string cellBuffer = String.Empty;
                int i, colWidth;
                for (i = 0; i < this.Spec.FixedWidthColumnWidths.Count; ++i)
                {
                    colWidth = this.Spec.FixedWidthColumnWidths[i];
                    // See whether need to include a delimiter or not
                    if (this.Spec.IsFixedWidthDelimiter == false)
                    {
                        // First expand if necessary ...
                        cellBuffer = columns[i].PadRight(colWidth);
                        // ... then contract to right size
                        cellBuffer = cellBuffer.Substring(0, colWidth);
                    }
                    else
                    {
                        // First expand if necessary, leaving space for column delimiter ...
                        cellBuffer = columns[i].PadRight(colWidth - this.Spec.ColumnDelimiter.Length);
                        // ... then contract with space for the delimiter ...
                        cellBuffer = cellBuffer.Substring(0, colWidth - this.Spec.ColumnDelimiter.Length);
                        // ... then add delimiter
                        cellBuffer = cellBuffer + this.Spec.ColumnDelimiter;
                    }
                    line = line + cellBuffer;
                }
            }
            return line;
        }

        /// <summary>
        /// Write an arbitrary line to the file ignoring contents of Column
        /// </summary>
        /// <param name="comment">line to write</param>
        public void WriteTextLine(string line)
        {
            this._WriteTextLine(line, false, String.Empty);
        }

        /// <summary>
        /// Write an arbitrary line to the file ignoring contents of Column
        /// </summary>
        /// <param name="comment">line to write</param>
        /// <param name="isCommented">If true, the line(s) will be commented, false it will be written as is</param>
        public void WriteTextLine(string line, bool isCommented)
        {
            this._WriteTextLine(line, isCommented, String.Empty);
        }

        public void WriteTextLine(string line, string endOfLineComment)
        {
            this._WriteTextLine(line, false, endOfLineComment);
        }

        private void _WriteTextLine(string line, bool isCommented, string endOfLineComment)
        {
            // Write headers and units if required
            if (this.IsHeaderLine(this.FileLineCount + 1))
            {
                this.__WriteTextLine(this.JoinLine(this.Spec.Headers), false, String.Empty);
                this.FileLineCount = this.FileLineCount + 1;
            }
            else if (this.IsUnitsLine(this.FileLineCount + 1))
            {
                this.__WriteTextLine(this.JoinLine(this.Spec.Units), false, String.Empty);
                this.FileLineCount = this.FileLineCount + 1;
            }
            // Finally write the line to the stream
            this.__WriteTextLine(line, isCommented, endOfLineComment);
        }

        private void __WriteTextLine(string line, bool isCommented, string endOfLineComment) 
        {

            // Prepend a comment string if necessary
            if (isCommented == true)
            {
                line = this.Spec.CommentString + line;
            }
            // Add an end of line comment if specified
            if (endOfLineComment != String.Empty)
            {
                line = line + this.Spec.CommentString + endOfLineComment;
            }
            // If a particular line delimiter is specified then use that to end the line, else use the system default
            if (this.Spec.LineDelimiter == String.Empty)
            {
                this._Stream.WriteLine(line);
            }
            else
            {
                line = line + this.Spec.LineDelimiter;
                this._Stream.Write(line);
            }
            // Increment the file line count
            this.FileLineCount = this.FileLineCount + 1;
            
        }

        private bool IsHeaderLine(int lineNum)
        {
            // 1st case: Headers specified but no row, write as first line in file
            if (this.Spec.Headers.Count > 0 && this.Spec.HeaderRow == -1 && lineNum == 1) 
            {
                return true;
            }
            // 2nd case: Header row specified explicitly
            else if (this.Spec.HeaderRow == lineNum)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsUnitsLine(int lineNum)
        {
            // 1st case: Units specified but no row, write on first line only if no headers due to go there
            if (this.Spec.Units.Count > 0 && this.Spec.UnitsRow == -1 
                && lineNum == 1 && this.IsHeaderLine(lineNum) == false) 
            {
                return true;
            } 
            // 2nd case: Units specified but no row, write on second line only if headers were due on first line
            else if (this.Spec.Units.Count > 0 && this.Spec.UnitsRow == -1 
                && lineNum == 2 && this.IsHeaderLine(lineNum - 1) == true)
            {
                return true;
            } 
            // 3rd case: Units row specified explicitly
            else if (this.Spec.UnitsRow == lineNum)
            {
                return true;
            } 
            else 
            {
                return false;
            }
        }

        
        
        /// <summary>
        /// Finish writing to the stream
        /// </summary>
        public void EndWriting()
        {
            this._Stream.Close();
        }

        

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            this.EndWriting();
        }

        #endregion
    }
}
