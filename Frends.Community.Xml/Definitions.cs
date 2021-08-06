#pragma warning disable 1591

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.Xml
{
    public class CombineXmlInput
    {
        /// <summary>
        /// An array of XMLs with child element names
        /// </summary>
        public CombineXmlInputXml[] InputXmls { set; get; }

        /// <summary>
        /// The name of the root element
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("\"Root\"")]
        public string XmlRootElementName { set; get; }
    }

    public class CombineXmlInputXml
    {
        /// <summary>
        /// XML input as string or an XML document
        /// </summary>
        public object Xml { set; get; }

        /// <summary>
        /// Child element name where the XML document will be written in
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

    public class ConvertToXmlJsonInputParameters
    {
        /// <summary>
        /// The name of the root element on the XML
        /// </summary>
        public string XMLRootElementName { get; set; }

        /// <summary>
        /// Append numeric JSON fields with prefix
        /// </summary>
        public string AppendToFieldName { get; set; }
    }

    public class ConvertToXmlCsvInputParameters
    {
        /// <summary>
        /// Separator used in the CSV
        /// </summary>
        public string CSVSeparator { get; set; }

        /// <summary>
        /// Output column lengths
        /// </summary>
        public ColumnLength[] ColumnLengths { get; set; }

        /// <summary>
        /// Input CSV has a header row
        /// </summary>
        public bool InputHasHeaderRow { get; set; }

        /// <summary>
        /// Trim output columns
        /// </summary>
        public bool TrimOuputColumns { get; set; }
    }
    public class ConvertToXmlParameters
    {
        /// <summary>
        /// Input data. Supported formats JSON, CSV and fixed length
        /// </summary>
        public string Input { get; set; }
    }

    public class ConvertToXmlOutput
    {
        /// <summary>
        /// Result string
        /// </summary>
        public string Result { get; set; }
    }

    public class ConvertXmlToCsvInput
    {
        /// <summary>
        /// XML string to be converted into csv
        /// </summary>
        [DisplayName("Input XML as string")]
        public string InputXmlString { get; set; }

        /// <summary>
        /// Separator for the output columns.
        /// </summary>
        [DefaultValue("\",\"")]
        public string CsvSeparator { get; set; }

        /// <summary>
        /// True if the column headers should be included in the output.
        /// </summary>
        [DefaultValue(true)]
        public bool IncludeHeaders { get; set; }
    }

    public class ConvertXmlToCsvOutput
    {
        /// <summary>
        /// Result csv
        /// </summary>
        public string Result { get; set; }
    }

    public class SplitXmlFileInput
    {
        /// <summary>
        /// Path to the input file
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

    public class SplitXmlFileOptions
    {
        /// <summary>
        /// Maximum number of chosen elements to be written in each file
        /// </summary>
        [DefaultValue(5000)]
        public int ElementCountInEachFile { get; set; }

        /// <summary>
        /// The name of the root element for the output file
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("Root")]
        public string OutputFileRootNodeName { get; set; }
    }

    public class SplitXmlFileResult
    {
        /// <summary>
        /// List of filepaths to the new files
        /// </summary>
        public List<string> FilePaths;
    }
}
