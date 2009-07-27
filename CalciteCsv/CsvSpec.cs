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
        public List<int> FixedWidthColumnWidths = new List<int>();
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
                case "FixedWidthFile":
                    this.IsFixedWidth = true;
                    this.CommentString = "#";
                    break;
                default:
                    break;
            }
        }

        public CsvSpec(string csvType, List<int> fixedWidthColumnWidths)
        {
            if (csvType == "FixedWidthFile")
            {
                this.IsFixedWidth = true;
                this.CommentString = "#";
                this.FixedWidthColumnWidths = fixedWidthColumnWidths;
            }
            else
            {
                throw new ArgumentException("Second argument only valid with 'CsvType.FixedWidthFile' for first arguemnt");
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
                if (this.FixedWidthColumnWidths.Count <= 0)
                {
                    throw new NoFixedWidthColumnWidthsDefinedException();
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
    public class NoFixedWidthColumnWidthsDefinedException : ApplicationException
    {
        public NoFixedWidthColumnWidthsDefinedException()
            : base("IsFixedWidth is 'true' yet no fixed width format is specified")
        {
        }
    }


    #endregion
    /// <summary>
    /// Preconfigured names of csv file types to be passed to the the 
    /// CsvSpec constructor in order to obtain preconfigured CSV file types
    /// </summary>
    public class CsvTypes
    {   
        public static string TabSeperatedFile = "TabSeperatedFile";
        public static string CommaSeperatedFile = "CommaSeperatedFile";
        public static string FixedWidthFile = "FixedWidthFile";
    }
    
}
