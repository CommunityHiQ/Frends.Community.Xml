#pragma warning disable 1591

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.Xml
{
    public class CombineXMLInput
    {
        /// <summary>
        /// Array of xmls with child element names
        /// </summary>
        public CombineXMLInputXml[] InputXmls { set; get; }

        /// <summary>
        ///  Name for root element
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("\"Root\"")]
        public string XmlRootElementName { set; get; }
    }

    public class CombineXMLInputXml
    {
        /// <summary>
        /// Xml input as string or xml document
        /// </summary>
        public object Xml { set; get; }

        /// <summary>
        /// Child element name where the xml document will be written in
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("\"ChildElement\"")]
        public string ChildElementName { set; get; }
    }

    public class ColumnLength
    {
        /// <summary>
        /// Column length
        /// </summary>
        public int Length { set; get; }
    }

    public class JsonInputParameters
    {
        /// <summary>
        /// XML root element name
        /// </summary>
        public string XMLRootElementName { get; set; }

        /// <summary>
        /// Json change numeric keys
        /// </summary>
        public string AppendToFieldName { get; set; }
    }

    public class CsvInputParameters
    {
        /// <summary>
        /// CSV separator
        /// </summary>
        public string CSVSeparator { get; set; }

        /// <summary>
        /// Output column lengths
        /// </summary>
        public ColumnLength[] ColumnLengths { get; set; }

        /// <summary>
        /// Input has header row
        /// </summary>
        public bool InputHasHeaderRow { get; set; }

        /// <summary>
        /// Trim ouput columns
        /// </summary>
        public bool TrimOuputColumns { get; set; }
    }
    public class Parameters
    {
        /// <summary>
        /// Input data. Supported formats JSON, CSV and fixed length
        /// </summary>
        public string Input { get; set; }
    }

    public class ConvertToXMLOutput
    {
        /// <summary>
        /// Result string
        /// </summary>
        public string Result { get; set; }
    }

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
