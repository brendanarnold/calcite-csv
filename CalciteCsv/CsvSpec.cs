using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalciteCsv
{
    public class CsvSpec
    {
        // TODO: Implement TypeConvertor so can serialize a CsvSpec to XML and the settings dialog
        public string ColumnDelimiter = String.Empty;
        public string CommentString = String.Empty;
        public string QuoteString = String.Empty;
        public string EscapeString = String.Empty;
        public string RowsToSkipFormat = String.Empty;
        public List<int> RowsToSkip = new List<int>();
        public List<string> Headers = new List<string>();
        public List<string> Units = new List<string>();
        public bool IsFixedWidth = false;
        public string FixedWidthFormat = String.Empty;
        public int HeaderRow = -1;
        public int UnitsRow = -1;

        public CsvSpec() : this(String.Empty) { }

        // Not going to bother with all these constructors until C# 4.0 
        // and the named parameters, until then use this hardcoded cludge
        public CsvSpec(string csvType)
        {
            switch (csvType)
            {
                case "TabSeperatedFile":
                    this.ColumnDelimiter = "\t";
                    this.CommentString = "#";
                    this.QuoteString = "\"";
                    this.EscapeString = "\\";
                    this.IsFixedWidth = false;
                    break;
                case "CommaSeperatedFile":
                    this.ColumnDelimiter = ",";
                    this.CommentString = "#";
                    this.QuoteString = "\"";
                    this.EscapeString = "\\";
                    this.IsFixedWidth = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// A check to determine if the spec does not have any obvious inconsistancies
        /// </summary>
        public bool IsValid() 
        {
            try
            {
                this.Validate();
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks the spec for obvious inconsistancies and raises an error as appropriate
        /// </summary>
        public void Validate()
        {
            // TODO: Make some more exception types
            if (this.IsFixedWidth == true)
            {
                if (this.FixedWidthFormat == String.Empty)
                {
                    throw new NoFixedWidthFormatDefinedException();
                }
            }
            else
            {
                if (this.ColumnDelimiter == String.Empty)
                {
                    throw new NoDelimiterDefinedException();
                }
            }
        }

    }

    #region CsvSpec exception classes

    [Serializable]
    public class NoDelimiterDefinedException : ApplicationException
    {
        public NoDelimiterDefinedException() 
            : base("A Delimiter string was not defined for a non-fixed width CSV specification")
        {
        }
    }

    [Serializable]
    public class NoFixedWidthFormatDefinedException : ApplicationException
    {
        public NoFixedWidthFormatDefinedException()
            : base("IsFixedWidth is 'true' yet no fixed width format is specified")
        {
        }
    }

    #endregion

    public class CsvTypes
    {
        public static string TabSeperatedFile = "TabSeperatedFile";
        public static string CommaSeperatedFile = "CommaSeperatedFile";
    }

}
