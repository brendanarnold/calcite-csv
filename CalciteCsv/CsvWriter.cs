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
        /// A comment string to be appended at the end of a row of data, erased after each write
        /// </summary>
        public string EndOfLineComment = String.Empty;
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
            string line = String.Empty;
            line = this.JoinLine(this.Columns);
            // Send off to generic text writing function for writing and reset the variables
            this.WriteTextLine(line, false);
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
        /// <param name="isCommented">If true, the line(s) will be commented, false it will be written as is</param>
        public void WriteTextLine(string line, bool isCommented) 
        {
            // TODO: Write headers and units

            // Prepend a comment string if necessary
            if (isCommented == true)
            {
                line = this.Spec.CommentString + line;
            }
            // Add an end of line comment if specified
            if (this.EndOfLineComment != String.Empty)
            {
                line = line + this.Spec.CommentString + this.EndOfLineComment;
                this.EndOfLineComment = String.Empty;
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
        
        /// <summary>
        /// Finish writing to the stream
        /// </summary>
        public void EndWriting()
        {
            this._Stream.Close();
        }


        #region IDisposable Members

        public void Dispose()
        {
            this.EndWriting();
        }

        #endregion
    }
}
