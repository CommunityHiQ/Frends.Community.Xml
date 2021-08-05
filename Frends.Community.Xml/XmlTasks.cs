using GenericParsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

#pragma warning disable 1591

namespace Frends.Community.Xml
{
    public static class XmlTasks
    {
        /// <summary>
        /// Combines 2 or more xml strings or documents to 1 xml string
        /// </summary>
        /// <param name="input">Xml strings or xml documents that will be merged</param>
        /// <returns>string</returns>
        public static async Task<string> CombineXML(CombineXMLInput input, CancellationToken cancellationToken)
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
        /// Task parses input data into XML data.
        /// Supported input formats JSON, CSV and fixed-length.
        /// </summary>
        /// <param name="parameters">JSON or CSV string to be converted.</param>
        /// <param name="csvInputParameters">Parameters for conversion from CSV to XML.</param>
        /// <param name="jsonInputParameters">Parameters for conversion from JSON to XML.</param>
        /// <returns>Object { string Result }</returns>
        public static ConvertToXMLOutput ConvertToXML(ConvertToXMLInput parameters, [PropertyTab] ConvertCSVtoXMLParameters csvInputParameters, [PropertyTab] ConvertJsonToXMLParameters jsonInputParameters, CancellationToken cancellationToken)
        {
            if (parameters.Input.GetType() != typeof(string))
                throw new InvalidDataException("The input data string was not in correct format. Supported formats are JSON, CSV and fixed length.");

            if (parameters.Input.StartsWith("{") || parameters.Input.StartsWith("["))
            {
                if (string.IsNullOrEmpty(jsonInputParameters.XMLRootElementName))
                    throw new MissingFieldException("Root element name missing. Required with JSON input");

                if (jsonInputParameters.AppendToFieldName == null)
                    return new ConvertToXMLOutput { Result = JsonConvert.DeserializeXmlNode(parameters.Input, jsonInputParameters.XMLRootElementName).OuterXml };

                cancellationToken.ThrowIfCancellationRequested();

                var jsonObject = (JObject) JsonConvert.DeserializeObject(parameters.Input);
                var newObject = ChangeNumericKeys(jsonObject, jsonInputParameters.AppendToFieldName);
                return new ConvertToXMLOutput { Result = JsonConvert.DeserializeXmlNode(JsonConvert.SerializeObject(newObject), jsonInputParameters.XMLRootElementName).OuterXml };
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
                    return new ConvertToXMLOutput { Result = parser.GetXml().OuterXml };
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
                return new ConvertToXMLOutput { Result = parser.GetXml().OuterXml };
            }
        }

        /// <summary>
        /// Splits XML file into smaller files. See https://github.com/CommunityHiQ/Frends.Community.Xml
        /// </summary>
        /// <param name="Input">Input XML to be split.</param>
        /// <param name="Options">Configuration for splitting the XML.</param>
        /// <returns>Object { List&lt;string&gt; FilePaths } </returns>
        public static SplitXMLFileResult SplitXMLFile([PropertyTab]SplitXMLFileInput Input, [PropertyTab]SplitXMLFileOptions Options, CancellationToken cancellationToken)
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

            return new SplitXMLFileResult() { FilePaths = returnArray };
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
    }
}
