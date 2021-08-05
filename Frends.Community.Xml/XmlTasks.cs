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
        /// Splits XML file into smaller files. See https://github.com/CommunityHiQ/Frends.Community.Xml
        /// </summary>
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
    }
}
