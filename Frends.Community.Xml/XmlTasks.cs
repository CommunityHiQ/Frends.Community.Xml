using GenericParsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

#pragma warning disable 1591

namespace Frends.Community.Xml
{
    public static class XmlTasks
    {
        /// <summary>
        /// Combines multiple XML strings or documents into a single XML. See: https://github.com/CommunityHiQ/Frends.Community.Xml
        /// </summary>
        /// <param name="input">XML strings or XML documents that will be merged together.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>string</returns>
        public static async Task<string> CombineXml([PropertyTab] CombineXmlInput input, CancellationToken cancellationToken)
        {
            var inputXmls = input.InputXmls;

            // Check invalid inputs
            var invalids = inputXmls.Where(f => f.Xml.GetType() != typeof(string) && f.Xml.GetType() != typeof(XmlDocument)).Select(f => f.ChildElementName).ToList();
            if (invalids.Any())
            {
                throw new FormatException("Unsupported input type found in ChildElements: " + string.Join(", ", invalids) + ". The supported types are XmlDocument and String.");
            }

            // Combine
            using (var sw = new StringWriter())
            {
                using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, OmitXmlDeclaration = true }))
                {
                    xw.WriteStartDocument();
                    xw.WriteStartElement(input.XmlRootElementName);

                    foreach (var xml in inputXmls)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        xw.WriteStartElement(xml.ChildElementName);

                        var xmlDoc = new XmlDocument();
                        if (xml.Xml.GetType() == typeof(XmlDocument))
                        {
                            xmlDoc = (XmlDocument)xml.Xml;
                        }
                        else
                        {
                            xmlDoc.LoadXml((string)xml.Xml);
                        }
                        using (var xr = new XmlNodeReader(xmlDoc))
                        {
                            xr.Read();
                            if (xr.NodeType.Equals(XmlNodeType.XmlDeclaration))
                            {
                                xr.Read();
                            }
                            await xw.WriteNodeAsync(xr, false).ConfigureAwait(false);
                        }
                        cancellationToken.ThrowIfCancellationRequested();
                        xw.WriteEndElement();
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    xw.WriteEndElement();
                    xw.WriteEndDocument();
                }
                return sw.ToString();
            }
        }

        /// <summary>
        /// Task parses input data into XML data. Supported input formats JSON, CSV and fixed-length.
        /// See: https://github.com/CommunityHiQ/Frends.Community.Xml
        /// </summary>
        /// <param name="parameters">JSON or CSV string to be converted.</param>
        /// <param name="csvInputParameters">Parameters for conversion from CSV to XML.</param>
        /// <param name="jsonInputParameters">Parameters for conversion from JSON to XML.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object { string Result }</returns>
        public static ConvertToXmlOutput ConvertToXml(ConvertToXmlParameters parameters, [PropertyTab] ConvertToXmlCsvInputParameters csvInputParameters, [PropertyTab] ConvertToXmlJsonInputParameters jsonInputParameters, CancellationToken cancellationToken)
        {
            if (parameters.Input.GetType() != typeof(string))
                throw new InvalidDataException("The input data string was not in correct format. Supported formats are JSON, CSV and fixed length.");

            if (parameters.Input.StartsWith("{") || parameters.Input.StartsWith("["))
            {
                if (string.IsNullOrEmpty(jsonInputParameters.XMLRootElementName))
                    throw new MissingFieldException("Root element name missing. Required with JSON input");

                if (jsonInputParameters.AppendToFieldName == null)
                    return new ConvertToXmlOutput { Result = JsonConvert.DeserializeXmlNode(parameters.Input, jsonInputParameters.XMLRootElementName).OuterXml };

                cancellationToken.ThrowIfCancellationRequested();

                var jsonObject = (JObject) JsonConvert.DeserializeObject(parameters.Input);
                var newObject = ChangeNumericKeys(jsonObject, jsonInputParameters.AppendToFieldName);
                return new ConvertToXmlOutput { Result = JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(newObject), jsonInputParameters.XMLRootElementName).OuterXml };
            }

            if (!string.IsNullOrEmpty(csvInputParameters.CSVSeparator) && parameters.Input.Contains(csvInputParameters.CSVSeparator))
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var parser = new GenericParserAdapter())
                {
                    char? separator = Convert.ToChar(csvInputParameters.CSVSeparator);
                    parser.SetDataSource(new StringReader(parameters.Input));
                    parser.ColumnDelimiter = separator;
                    parser.FirstRowHasHeader = csvInputParameters.InputHasHeaderRow;
                    parser.MaxBufferSize = 4096;
                    parser.TrimResults = csvInputParameters.TrimOuputColumns;
                    return new ConvertToXmlOutput { Result = parser.GetXml().OuterXml };
                }
            }

            if (csvInputParameters.ColumnLengths == null)
                throw new InvalidDataException("The input was recognized as fixed length file, but no column lengths were supplied.");

            using (var parser = new GenericParserAdapter())
            {
                var headerList = new List<int>();
                foreach (var column in csvInputParameters.ColumnLengths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    headerList.Add(column.Length);
                }
                var headerArray = headerList.ToArray();

                parser.SetDataSource(new StringReader(parameters.Input));
                parser.ColumnWidths = headerArray;
                parser.FirstRowHasHeader = csvInputParameters.InputHasHeaderRow;
                parser.MaxBufferSize = 4096;
                parser.TrimResults = csvInputParameters.TrimOuputColumns;
                return new ConvertToXmlOutput { Result = parser.GetXml().OuterXml };
            }
        }

        /// <summary>
        /// Convert XML or JSON data into CSV formatted data. See: https://github.com/CommunityHiQ/Frends.Community.Xml
        /// </summary>
        /// <param name="input">Input XML</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object {string Result }</returns>
        public static ConvertXmlToCsvOutput ConvertXmlToCsv([PropertyTab] ConvertXmlToCsvInput input, CancellationToken cancellationToken)
        {
            DataSet dataset;
            dataset = new DataSet();

            cancellationToken.ThrowIfCancellationRequested();

            dataset.ReadXml(XmlReader.Create(new StringReader(input.InputXmlString)));

            cancellationToken.ThrowIfCancellationRequested();

            return new ConvertXmlToCsvOutput { Result = ConvertDataTableToCsv(dataset.Tables[0], input.CsvSeparator, input.IncludeHeaders, cancellationToken) };
        }

        /// <summary>
        /// A task to sign an XML document. See: https://github.com/CommunityHiQ/Frends.Community.Xml
        /// </summary>
        /// <param name="input">Parameters for input XML.</param>
        /// <param name="output">Parameters for output XML.</param>
        /// <param name="options">Options for the signing operation.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static SigningResult SignXml([PropertyTab] SignXmlInput input, [PropertyTab] SignXmlOutput output, [PropertyTab] SignXmlOptions options, CancellationToken cancellationToken)
        {
            var result = new SigningResult();
            var xmldoc = new XmlDocument() { PreserveWhitespace = options.PreserveWhitespace };
            StreamReader xmlStream = null;

            cancellationToken.ThrowIfCancellationRequested();

            if (input.XmlInputType == XmlParamType.File)
            {
                xmlStream = new StreamReader(input.XmlFilePath);
                xmldoc.Load(xmlStream);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(input.Xml))
                    throw new System.ArgumentException("Invalid input xml");
                xmldoc.LoadXml(input.Xml);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var signedXml = new SignedXml(xmldoc);

            // determine signature method
            switch (options.XmlSignatureMethod)
            {
                case XmlSignatureMethod.RSASHA1:
                    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
                    break;
                case XmlSignatureMethod.RSASHA256:
                    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;
                    break;
                case XmlSignatureMethod.RSASHA384:
                    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA384Url;
                    break;
                case XmlSignatureMethod.RSASHA512:
                    signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA512Url;
                    break;
            }

            // determine how to sign
            switch (input.SigningStrategy)
            {
                case SigningStrategyType.PrivateKeyCertificate:
                    var cert = new X509Certificate2(input.CertificatePath, input.PrivateKeyPassword);
                    signedXml.SigningKey = cert.GetRSAPrivateKey();

                    // public key certificate is submitted with the xml document
                    var keyInfo = new KeyInfo();
                    keyInfo.AddClause(new KeyInfoX509Data(cert));
                    signedXml.KeyInfo = keyInfo;
                    break;
            }

            var reference = new Reference();
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform(options.IncludeComments));

            // add different transforms
            foreach (var transform in options.TransformMethods)
            {
                switch (transform)
                {
                    case TransformMethod.DsigBase64:
                        reference.AddTransform(new XmlDsigBase64Transform());
                        break;
                    case TransformMethod.DsigC14:
                        reference.AddTransform(new XmlDsigC14NTransform());
                        break;
                    case TransformMethod.DsigC14WithComments:
                        reference.AddTransform(new XmlDsigC14NWithCommentsTransform());
                        break;
                    case TransformMethod.DsigExcC14:
                        reference.AddTransform(new XmlDsigExcC14NTransform());
                        break;
                    case TransformMethod.DsigExcC14WithComments:
                        reference.AddTransform(new XmlDsigExcC14NWithCommentsTransform());
                        break;
                }
            }

            // target the whole xml document
            reference.Uri = "";

            // add digest method
            switch (options.DigestMethod)
            {
                case DigestMethod.SHA1:
                    reference.DigestMethod = SignedXml.XmlDsigSHA1Url;
                    break;
                case DigestMethod.SHA256:
                    reference.DigestMethod = SignedXml.XmlDsigSHA256Url;
                    break;
                case DigestMethod.SHA384:
                    reference.DigestMethod = SignedXml.XmlDsigSHA384Url;
                    break;
                case DigestMethod.SHA512:
                    reference.DigestMethod = SignedXml.XmlDsigSHA512Url;
                    break;
            }

            // add references to signed xml
            signedXml.AddReference(reference);

            // compute the signature
            signedXml.ComputeSignature();

            // as this is Xml Enveloped Signature,
            // add the signature element to the original xml as the last child of the root element
            xmldoc.DocumentElement.AppendChild(xmldoc.ImportNode(signedXml.GetXml(), true));

            cancellationToken.ThrowIfCancellationRequested();

            // output results either to a file or result object
            if (output.OutputType == XmlParamType.File)
            {
                xmlStream.Dispose();

                if (output.AddSignatureToSourceFile)
                {
                    // signed xml document is written in target destination
                    xmldoc.Save(input.XmlFilePath);

                    // and result will indicate the source file path
                    result.Result = input.XmlFilePath;
                }
                else
                {
                    // signed xml document is written in target destination
                    using (var writer = new XmlTextWriter(output.OutputFilePath, Encoding.GetEncoding(output.OutputEncoding)))
                    {
                        xmldoc.Save(writer);
                    }

                    // and result will indicate the document path
                    result.Result = output.OutputFilePath;
                }
            }
            else
            {
                // signed xml document is returned from task
                result.Result = xmldoc.OuterXml;
            }

            return result;
        }

        /// <summary>
        /// Splits XML file into smaller files. See: https://github.com/CommunityHiQ/Frends.Community.Xml
        /// </summary>
        /// <param name="Input">Input XML to be split.</param>
        /// <param name="Options">Configuration for splitting the XML.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Object { List&lt;string&gt; FilePaths } </returns>
        public static SplitXmlFileResult SplitXmlFile([PropertyTab] SplitXmlFileInput Input, [PropertyTab] SplitXmlFileOptions Options, CancellationToken cancellationToken)
        {
            int seqNr = 0;
            int loopSeqNr = 0;
            List<string> returnArray = new List<string>();

            FileInfo fileInfo = new FileInfo(Input.InputFilePath);
            DirectoryInfo dirInfo = new DirectoryInfo(Input.OutputFilesDirectory);

            using (XmlReader processDoc = XmlReader.Create(Input.InputFilePath, new XmlReaderSettings() { DtdProcessing = DtdProcessing.Ignore }))
            {

                XmlDocument newDoc = InitiateNewDocument(Options.OutputFileRootNodeName);

                while (processDoc.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Second while is needed because Read() skips elements if there is no white space between them.
                    // This happens because ReadInnerXml() (below) moves the reader to the next element and then Read() will skip it
                    while (processDoc.Name == Input.SplitAtElementName && processDoc.NodeType == XmlNodeType.Element && processDoc.ReadState != ReadState.EndOfFile)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        XmlElement outputNode = newDoc.CreateElement(processDoc.Name);
                        outputNode.InnerXml = processDoc.ReadInnerXml();
                        newDoc.LastChild.AppendChild(outputNode);

                        // Write new file when the max element count is reached
                        if (++loopSeqNr >= Options.ElementCountInEachFile)
                        {
                            string outputFilePath = WriteToFile(fileInfo.Name, seqNr++, dirInfo.FullName, newDoc);
                            returnArray.Add(outputFilePath);
                            loopSeqNr = 0;
                            newDoc = InitiateNewDocument(Options.OutputFileRootNodeName);
                        }
                    }
                }

                // If there are any leftover elements we create one last file
                if (processDoc.ReadState == ReadState.EndOfFile && loopSeqNr != 0)
                {
                    string outputFilePath = WriteToFile(fileInfo.Name, seqNr, dirInfo.FullName, newDoc);
                    returnArray.Add(outputFilePath);
                }
            }

            return new SplitXmlFileResult() { FilePaths = returnArray };
        }

        /// <summary>
        /// A task to verify the signature of a signed XML. See: https://github.com/CommunityHiQ/Frends.Community.Xml
        /// </summary>
        /// <param name="input">Parameters for input XML.</param>
        /// <param name="options">Additional options for verifications.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static VerifySignatureResult VerifySignedXml([PropertyTab] VerifySignatureInput input, [PropertyTab] VerifySignatureOptions options, CancellationToken cancellationToken)
        {
            var result = new VerifySignatureResult();
            var xmldoc = new XmlDocument() { PreserveWhitespace = options.PreserveWhitespace };
            StreamReader xmlStream = null;

            cancellationToken.ThrowIfCancellationRequested();

            if (input.XmlInputType == XmlParamType.File)
            {
                xmlStream = new StreamReader(input.XmlFilePath);
                xmldoc.Load(xmlStream);
            }
            else
            {
                xmldoc.LoadXml(input.Xml);
            }

            // load the signature node
            var signedXml = new SignedXml(xmldoc);
            signedXml.LoadXml((XmlElement)xmldoc.GetElementsByTagName("Signature")[0]);

            X509Certificate2 certificate = null;

            foreach (KeyInfoClause clause in signedXml.KeyInfo)
            {
                if (clause is KeyInfoX509Data)
                {
                    if (((KeyInfoX509Data)clause).Certificates.Count > 0)
                    {
                        certificate = (X509Certificate2)((KeyInfoX509Data)clause).Certificates[0];
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            // Check the signature and return the result.
            result.IsValid = signedXml.CheckSignature(certificate, true);

            // close stream if input was a file
            if (input.XmlInputType == XmlParamType.File)
            {
                xmlStream.Dispose();
            }

            return result;
        }

        private static XmlDocument InitiateNewDocument(string Rootname)
        {
            XmlDocument newDoc = new XmlDocument();
            XmlDeclaration xmlDeclaration = newDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = newDoc.DocumentElement;
            newDoc.InsertBefore(xmlDeclaration, root);

            XmlElement rootElement = newDoc.CreateElement(string.Empty, Rootname, string.Empty);
            newDoc.AppendChild(rootElement);

            return newDoc;
        }

        public static string WriteToFile(string InputName, int SeqNr, string OutputFolder, XmlDocument NewDoc)
        {
            string strFileName = InputName + "." + SeqNr + ".part";
            string outputFilePath = Path.Combine(OutputFolder, strFileName);
            NewDoc.Save(outputFilePath);

            return outputFilePath;
        }

        private static JObject ChangeNumericKeys(JObject o, string appendWith)
        {
            var newO = new JObject();

            foreach (var node in o)
            {
                switch (node.Value.Type)
                {
                    case JTokenType.Array:
                        var newArray = new JArray();
                        foreach (var item in node.Value)
                        {
                            newArray.Add(ChangeNumericKeys(JObject.FromObject(item), appendWith));
                        }
                        newO[node.Key] = newArray;
                        break;
                    case JTokenType.Object:
                        newO[node.Key] = ChangeNumericKeys(JObject.FromObject(node.Value), appendWith);
                        break;
                    default:
                        if (char.IsNumber(node.Key[0]))
                        {
                            var newName = appendWith + node.Key;
                            newO[newName] = node.Value;
                        }
                        else
                        {
                            newO[node.Key] = node.Value;
                        }
                        break;
                }
            }

            return newO;
        }

        private static string ConvertDataTableToCsv(DataTable datatable, string separator, bool includeHeaders, CancellationToken cancellationToken)
        {
            var stringBuilder = new StringBuilder();

            if (includeHeaders)
            {
                var columnNames = datatable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
                stringBuilder.AppendLine(string.Join(separator, columnNames));
            }

            foreach (DataRow row in datatable.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fields = row.ItemArray.Select(field => field.ToString());
                fields = fields.Select(x => (x.Contains(separator) || x.Contains("\n") || x.Contains("\"")) ? "\"" + x.Replace("\"", "\"\"") + "\"" : x); // Fixes cases where input field contains special characters
                stringBuilder.AppendLine(string.Join(separator, fields));
            }

            return stringBuilder.ToString();
        }
    }
}
