using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalciteCsv
{
    class Helpers
    {
        /// <summary>
        /// Return the list of integers specified by the range string.  Default uses '-' as range operator 
        /// and comma or space to separate clauses. 
        /// i.e. 1-3,5,7,10-12 would give 1,2,3,5,7,10,11,12
        /// Spaces are ignored, invalid syntax is ignored
        /// </summary>
        /// <param name="rangeFormat">Range format string</param>
        /// <returns></returns>
        internal static List<int> ParseRangeFormat(string rangeFormat)
        {
            // If clause and range separator not specified, use comma or space and hyphen
            return ParseRangeFormat(rangeFormat, new string[] { ",", " " }, new string[] { "-" });
        }

        /// <summary>
        /// Return the list of integers specified by the range string.  Default uses '-' as range operator 
        /// and comma to separate clauses. 
        /// i.e. 1-3,5,7,10-12 would give 1,2,3,5,7,10,11,12
        /// Spaces are ignored, invalid syntax is ignored 
        /// </summary>
        /// <param name="rangeFormat">Range format string</param>
        /// <param name="clauseSeparator">Default is ","</param>
        /// <param name="rangeSeparator">Default is "-"</param>
        /// <returns></returns>
        internal static List<int> ParseRangeFormat(string rangeFormat, string[] clauseSeparators, string[] rangeSeparators)
        {
            // Format can be a range 1-5 or separate integers, separated by commas
            List<int> intsInRange = new List<int>();
            int result, lower, upper, i;
            foreach (string clause in rangeFormat.Split(clauseSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] nums = clause.Split(rangeSeparators, StringSplitOptions.RemoveEmptyEntries);
                // Trim off any whitespace
                Array.ForEach<string>(nums, x => x.Trim());
                if (nums.Length == 1)
                {
                    if (Int32.TryParse(nums[0], out result))
                    {
                        intsInRange.Add(result);
                    }
                }
                else if (nums.Length == 2)
                {
                    if (Int32.TryParse(nums[0], out lower)
                        && Int32.TryParse(nums[1], out upper)
                        && upper >= lower)
                    {
                        for (i = lower; i <= upper; i++)
                        {
                            intsInRange.Add(i);
                        }
                    }
                }
            }
            return intsInRange.Distinct().ToList<int>();
        }
    }
}
