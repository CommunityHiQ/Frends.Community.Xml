#pragma warning disable 1591

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.Xml
{
    public class SplitXMLFileInput
    {
        /// <summary>
        /// Path of input file
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(@"F:\myfile.xml")]
        public string InputFilePath { get; set; }

        /// <summary>
        /// Name of the XML elements which are copied to output files
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(@"Product")]
        public string SplitAtElementName { get; set; }

        /// <summary>
        /// Output directory for new files
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue(@"F:\output")]
        public string OutputFilesDirectory { get; set; }
    }

    public class SplitXMLFileOptions
    {
        /// <summary>
        /// Maximum number of chosen elements to be written in each file
        /// </summary>
        [DefaultValue(5000)]
        public int ElementCountInEachFile { get; set; }

        /// <summary>
        /// Root element of output file
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("Root")]
        public string OutputFileRootNodeName { get; set; }
    }


    public class SplitXMLFileResult
    {
        /// <summary>
        /// List of new files
        /// </summary>
        public List<string> FilePaths;
    }
}
