#pragma warning disable CS1591

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Community.Xml
{
    /// <summary>
    /// Input-class for CombineXML-task
    /// </summary>
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

    /// <summary>
    /// Class for an array of XMLs with child element names
    /// </summary>
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

    /// <summary>
    /// An array of XMLs with child element names
    /// </summary>
    public class ColumnLength
    {
        /// <summary>
        /// Column length
        /// </summary>
        public int Length { set; get; }
    }

    /// <summary>
    /// JSON input class for ConvertToXml-task
    /// </summary>
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

    /// <summary>
    /// CSV input class for ConvertToXml-task
    /// </summary>
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

    /// <summary>
    /// Input class for ConvertToXml-task
    /// </summary>
    public class ConvertToXmlParameters
    {
        /// <summary>
        /// Input data. Supported formats JSON, CSV and fixed length
        /// </summary>
        public string Input { get; set; }
    }

    /// <summary>
    /// Output class for ConvertToXml-task
    /// </summary>
    public class ConvertToXmlOutput
    {
        /// <summary>
        /// Result string
        /// </summary>
        public string Result { get; set; }
    }

    /// <summary>
    /// Input class for ConvertXmlToCsv-task
    /// </summary>
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

    /// <summary>
    /// Output class for ConvertXmlToCsv-task
    /// </summary>
    public class ConvertXmlToCsvOutput
    {
        /// <summary>
        /// Result csv
        /// </summary>
        public string Result { get; set; }
    }

    /// <summary>
    /// Input class for SignXml-task
    /// </summary>
    public class SignXmlInput
    {
        /// <summary>
        /// Input type. Possible types are File and XmlString
        /// </summary>
        public XmlParamType XmlInputType { get; set; }

        /// <summary>
        /// Path to xml document to sign
        /// </summary>
        [DefaultValue("c:\\temp\\document.xml")]
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(XmlInputType), "", XmlParamType.File)]
        public string XmlFilePath { get; set; }

        /// <summary>
        /// XML to sign
        /// </summary>
        [DefaultValue("<root><value>123</value></root>")]
        [DisplayFormat(DataFormatString = "Xml")]
        [UIHint(nameof(XmlInputType), "", XmlParamType.XmlString)]
        public string Xml { get; set; }

        /// <summary>
        /// XML signing technique to use
        /// </summary>
        public XmlEnvelopingType XmlEnvelopingType { get; set; }

        /// <summary>
        /// How to sign the document
        /// </summary>
        public SigningStrategyType SigningStrategy { get; set; }

        /// <summary>
        /// Path to certificate with private key
        /// </summary>
        [DefaultValue("c:\\certificates\\signingcertificate.pfx")]
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(SigningStrategy), "", SigningStrategyType.PrivateKeyCertificate)]
        public string CertificatePath { get; set; }

        /// <summary>
        /// Private key password
        /// </summary>
        [PasswordPropertyText]
        [UIHint(nameof(SigningStrategy), "", SigningStrategyType.PrivateKeyCertificate)]
        public string PrivateKeyPassword { get; set; }
    }

    /// <summary>
    /// Output class for SignXml-task
    /// </summary>
    public class SignXmlOutput
    {
        /// <summary>
        /// Output to file or xml string?
        /// </summary>
        public XmlParamType OutputType { get; set; }

        /// <summary>
        /// A filepath for the output XML
        /// </summary>
        [DefaultValue("c:\\temp\\signedOutput.xml")]
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(OutputType), "", XmlParamType.File)]
        public string OutputFilePath { get; set; }

        /// <summary>
        /// The encoding for the output file
        /// </summary>
        [DefaultValue("UTF-8")]
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(OutputType), "", XmlParamType.File)]
        public string OutputEncoding { get; set; }

        /// <summary>
        /// If source is file, then you can add signature to it
        /// </summary>
        [UIHint(nameof(OutputType), "", XmlParamType.File)]
        public bool AddSignatureToSourceFile { get; set; }
    }

    /// <summary>
    /// Options class for SignXml-task
    /// </summary>
    public class SignXmlOptions
    {
        /// <summary>
        /// Switch to include comments
        /// </summary>
        public bool IncludeComments { get; set; }

        /// <summary>
        /// Should whitespace be preserved when loading the XML?
        /// </summary>
        public bool PreserveWhitespace { get; set; }

        /// <summary>
        /// Signature methods to be used with signing
        /// </summary>
        public XmlSignatureMethod XmlSignatureMethod { get; set; }

        /// <summary>
        /// Digest methods to be used
        /// </summary>
        public DigestMethod DigestMethod { get; set; }

        /// <summary>
        /// Transform methods to be used
        /// </summary>
        public TransformMethod[] TransformMethods { get; set; }
    }

    /// <summary>
    /// Output class for SignXml-task
    /// </summary>
    public class SigningResult
    {
        /// <summary>
        /// If output type is file, this will be a filepath. Otherwise, this will be the signed XML as string.
        /// </summary>
        public string Result { get; set; }
    }

    /// <summary>
    /// Can be either a file or an XML string
    /// </summary>
    public enum XmlParamType
    {
        File,
        XmlString
    }

    /// <summary>
    /// XML signature strategy for SignXml-task
    /// </summary>
    public enum SigningStrategyType
    {
        PrivateKeyCertificate
    }

    /// <summary>
    /// Signature enveloping type for SignXml-task
    /// </summary>
    public enum XmlEnvelopingType
    {
        XmlEnvelopedSignature
    }

    /// <summary>
    /// Signature methods for XMLDSIG
    /// </summary>
    public enum XmlSignatureMethod
    {
        RSASHA1,
        RSASHA256,
        RSASHA384,
        RSASHA512
    }

    /// <summary>
    /// Transform methods
    /// </summary>
    public enum TransformMethod
    {
        DsigC14,
        DsigC14WithComments,
        DsigExcC14,
        DsigExcC14WithComments,
        DsigBase64
    }

    /// <summary>
    /// Digest methods
    /// </summary>
    public enum DigestMethod
    {
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

    /// <summary>
    /// Input class for SplitXmlFile-task
    /// </summary>
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

    /// <summary>
    /// Options class for SplitXmlFile-task
    /// </summary>
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

    /// <summary>
    /// Output class for SplitXml-task
    /// </summary>
    public class SplitXmlFileResult
    {
        /// <summary>
        /// List of filepaths to the new files
        /// </summary>
        public List<string> FilePaths;
    }

    /// <summary>
    /// Input class for VerifySignedXml-task
    /// </summary>
    public class VerifySignatureInput
    {
        /// <summary>
        /// Either an XML string or a filepath
        /// </summary>
        public XmlParamType XmlInputType { get; set; }

        /// <summary>
        /// Path to the XML document
        /// </summary>
        [DefaultValue("c:\\temp\\documentToVerify.xml")]
        [DisplayFormat(DataFormatString = "Text")]
        [UIHint(nameof(XmlInputType), "", XmlParamType.File)]
        public string XmlFilePath { get; set; }

        /// <summary>
        /// XML in string format
        /// </summary>
        [DisplayFormat(DataFormatString = "Xml")]
        [UIHint(nameof(XmlInputType), "", XmlParamType.XmlString)]
        public string Xml { get; set; }
    }

    /// <summary>
    /// Options class for VerifySignedXml-task
    /// </summary>
    public class VerifySignatureOptions
    {
        /// <summary>
        /// Should whitespace be preserved when loading the XML?
        /// </summary>
        public bool PreserveWhitespace { get; set; }
    }

    /// <summary>
    /// Output class for VerifySignedXml-task
    /// </summary>
    public class VerifySignatureResult
    {
        /// <summary>
        /// True, if valid. Otherwise, false.
        /// </summary>
        public bool IsValid { get; set; }
    }
}
