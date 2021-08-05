using System;
using NUnit.Framework;
using System.Threading;
using System.IO;
using System.Xml;
using System.Linq;
using System.Threading.Tasks;

namespace Frends.Community.Xml.Tests
{
    [TestFixture]
    public class XmlTasksTests
    {
        private string _xmlString1;
        private string _xmlString2;
        private XmlDocument _xmlDoc1 = new XmlDocument();
        private XmlDocument _xmlDoc2 = new XmlDocument();

        private CombineXMLInput _input;
        private CombineXMLInputXml[] _inputXmls = new CombineXMLInputXml[2];

        private string _minifiedInputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\12_products_minified.xml");
        private string _prettyInputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestFiles\12_products_pretty.xml");
        private string _tempOutputFolder;


        [SetUp]
        public void TestSetup()
        {
            // CombineXML setup
            _xmlString1 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><bar1>foo1</bar1>";
            _xmlString2 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><bar2>foo2</bar2>";
            _xmlDoc1.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><foo1>bar1</foo1>");
            _xmlDoc2.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><foo2>bar2</foo2>");

            _inputXmls[0] = new CombineXMLInputXml { ChildElementName = "XML1" };
            _inputXmls[1] = new CombineXMLInputXml { ChildElementName = "XML2" };

            _input = new CombineXMLInput { InputXmls = _inputXmls, XmlRootElementName = "Root" };

            // SplitXml setup
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
        public async Task ShouldCombineXmlStrings()
        {
            _inputXmls[0].Xml = _xmlString1;
            _inputXmls[1].Xml = _xmlString2;

            var result = await XmlTasks.CombineXML(_input, new CancellationToken());
            Assert.That(result, Is.EqualTo("<Root><XML1><bar1>foo1</bar1></XML1><XML2><bar2>foo2</bar2></XML2></Root>"));
        }

        [Test]
        public async Task ShouldCombineXmlDocs()
        {
            _inputXmls[0].Xml = _xmlDoc1;
            _inputXmls[1].Xml = _xmlDoc2;

            var result = await XmlTasks.CombineXML(_input, new CancellationToken());
            Assert.That(result, Is.EqualTo("<Root><XML1><foo1>bar1</foo1></XML1><XML2><foo2>bar2</foo2></XML2></Root>"));
        }

        [Test]
        public async Task ShouldCombineXmlStringAndXmlDoc()
        {
            _inputXmls[0].Xml = _xmlString1;
            _inputXmls[1].Xml = _xmlDoc1;

            var result = await XmlTasks.CombineXML(_input, new CancellationToken());
            Assert.That(result, Is.EqualTo("<Root><XML1><bar1>foo1</bar1></XML1><XML2><foo1>bar1</foo1></XML2></Root>"));
        }

        [Test]
        public async Task ShouldCombineWithNewRootAndElementNames()
        {
            _inputXmls[0].Xml = _xmlString1;
            _inputXmls[0].ChildElementName = "NEW_ELEMENT1";
            _inputXmls[1].Xml = _xmlString2;
            _inputXmls[1].ChildElementName = "NEW_ELEMENT2";
            _input.XmlRootElementName = "NEW_ROOT";

            var result = await XmlTasks.CombineXML(_input, new CancellationToken());
            Assert.That(result, Is.EqualTo("<NEW_ROOT><NEW_ELEMENT1><bar1>foo1</bar1></NEW_ELEMENT1><NEW_ELEMENT2><bar2>foo2</bar2></NEW_ELEMENT2></NEW_ROOT>"));
        }

        [Test]
        public void ShouldNotCombineOtherObjects()
        {
            _inputXmls[0].Xml = _xmlString1;
            _inputXmls[1].Xml = 123456;
            Assert.ThrowsAsync<FormatException>(() => XmlTasks.CombineXML(_input, new CancellationToken()));

            _inputXmls[0].Xml = new object();
            _inputXmls[1].Xml = _xmlDoc2;
            Assert.ThrowsAsync<FormatException>(() => XmlTasks.CombineXML(_input, new CancellationToken()));

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
