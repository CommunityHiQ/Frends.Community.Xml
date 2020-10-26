using System;
using NUnit.Framework;
using Frends.Community.Xml;
using System.Threading;
using System.IO;
using System.Xml;
using System.Linq;

namespace Frends.Split.Tests
{
    [TestFixture]
    public class SplitXMLFileTests
    {
        private string _minifiedInputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\12_products_minified.xml");
        private string _prettyInputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\12_products_pretty.xml");
        private string _tempOutputFolder;


        [SetUp]
        public void TestSetup()
        {
            _tempOutputFolder = Path.Combine(Path.GetTempPath(), "splitxml_tests");

            if (!Directory.Exists(_tempOutputFolder))
            {
                Directory.CreateDirectory(_tempOutputFolder);
            }            
        }

        [TearDown]
        public void TestTearDown()
        {
            Directory.Delete(_tempOutputFolder, true);
        }

        private string ExecuteXpath(string filepath, string xpath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filepath);
            var nav = xmlDoc.CreateNavigator();
            var expr = nav.Compile(xpath);
            var result = (string) nav.Evaluate(expr);

            return result;
        }
        
        [Test]
        public void TestMinifiedXML_even()
        {
            var input = new SplitXMLFileInput() { InputFilePath = _minifiedInputPath, OutputFilesDirectory = _tempOutputFolder, SplitAtElementName = "Product" };
            var opt = new SplitXMLFileOptions() {ElementCountInEachFile = 4, OutputFileRootNodeName = "root"};

            var result = XmlTasks.SplitXMLFile(input, opt, new CancellationToken());

            //12 products should be split into 3 files
            Assert.AreEqual(3, result.FilePaths.Count);

            //Last file should have 4 Products
            var productCount = ExecuteXpath(result.FilePaths.Last(), "string(count(/root/Product))");
            Assert.AreEqual("4", productCount);

            //The Last Product of first file should have id 4
            var id = ExecuteXpath(result.FilePaths.First(), "string(/root/Product[last()]/id)");
            Assert.AreEqual("4", id);

            //The second file should have 4 products
            var productCount2nd = ExecuteXpath(result.FilePaths[1], "string(count(/root/Product))");
            Assert.AreEqual("4", productCount2nd);

            //The Last Product of second file should have id 8
            var id2nd = ExecuteXpath(result.FilePaths[1], "string(/root/Product[last()]/id)");
            Assert.AreEqual("8", id2nd);

        }

        [Test]
        public void TestPrettyXML_uneven()
        {
            var input = new SplitXMLFileInput() { InputFilePath = _prettyInputPath, OutputFilesDirectory = _tempOutputFolder, SplitAtElementName = "Product" };
            var opt = new SplitXMLFileOptions() { ElementCountInEachFile = 10, OutputFileRootNodeName = "root" };

            var result = XmlTasks.SplitXMLFile(input, opt, new CancellationToken());

            //12 products should be split into 2 files
            Assert.AreEqual(2, result.FilePaths.Count);

            //Last file should have 2 Products
            var productCount = ExecuteXpath(result.FilePaths.Last(), "string(count(/root/Product))");
            Assert.AreEqual("2", productCount);

            //The Last Product of first file should have id 10
            var id = ExecuteXpath(result.FilePaths.First(), "string(/root/Product[last()]/id)");
            Assert.AreEqual("10", id);
        }

        [Test]
        public void TestPrettyXML_notfull()
        {
            string expectedRootElement = "TestRoot";

            var input = new SplitXMLFileInput() { InputFilePath = _prettyInputPath, OutputFilesDirectory = _tempOutputFolder, SplitAtElementName = "Product" };
            var opt = new SplitXMLFileOptions() { ElementCountInEachFile = 20, OutputFileRootNodeName = expectedRootElement };

            var result = XmlTasks.SplitXMLFile(input, opt, new CancellationToken());

            //Check root element
            var rootElementName = ExecuteXpath(result.FilePaths.Last(), "local-name(/*)");
            Assert.AreEqual(expectedRootElement, rootElementName);

            //The Last Product of first file should have id 12
            var id = ExecuteXpath(result.FilePaths.First(), "string(/TestRoot/Product[last()]/id)");
            Assert.AreEqual("12", id);
        }        
    }
}
