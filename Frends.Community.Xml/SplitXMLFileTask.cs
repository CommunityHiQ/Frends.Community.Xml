using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Xml;

#pragma warning disable 1591

namespace Frends.Community.Xml
{
    public static class XmlTasks
    {
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
                            string strFileName = fileInfo.Name + "." + seqNr++ + ".part";
                            string outputFilePath = Path.Combine(dirInfo.FullName, strFileName);
                            newDoc.Save(outputFilePath);
                            returnArray.Add(outputFilePath);
                            loopSeqNr = 0;
                            newDoc = InitiateNewDocument(Options.OutputFileRootNodeName);
                        }
                    }
                }

                // If there are any leftover elements we create one last file
                if (processDoc.ReadState == ReadState.EndOfFile && loopSeqNr != 0)
                {
                    string strFileName = fileInfo.Name + "." + seqNr + ".part";
                    string outputFilePath = Path.Combine(dirInfo.FullName, strFileName);
                    newDoc.Save(outputFilePath);
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
    }
}
