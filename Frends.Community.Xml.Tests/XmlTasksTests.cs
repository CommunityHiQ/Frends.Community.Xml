using System;
using NUnit.Framework;
using System.Threading;
using System.IO;
using System.Xml;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Frends.Community.Xml.Tests
{
    [TestFixture]
    public class SplitXMLFileTests
    {
        private readonly string _minifiedInputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "12_products_minified.xml");
        private readonly string _prettyInputPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", "12_products_pretty.xml");
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
            var result = (string)nav.Evaluate(expr);

            return result;
        }

        [Test]
        public void TestMinifiedXML_even()
        {
            var input = new SplitXmlFileInput { InputFilePath = _minifiedInputPath, OutputFilesDirectory = _tempOutputFolder, SplitAtElementName = "Product" };
            var opt = new SplitXmlFileOptions {ElementCountInEachFile = 4, OutputFileRootNodeName = "root"};

            var result = XmlTasks.SplitXmlFile(input, opt, new CancellationToken());

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
            var input = new SplitXmlFileInput { InputFilePath = _prettyInputPath, OutputFilesDirectory = _tempOutputFolder, SplitAtElementName = "Product" };
            var opt = new SplitXmlFileOptions { ElementCountInEachFile = 10, OutputFileRootNodeName = "root" };

            var result = XmlTasks.SplitXmlFile(input, opt, new CancellationToken());

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

            var input = new SplitXmlFileInput { InputFilePath = _prettyInputPath, OutputFilesDirectory = _tempOutputFolder, SplitAtElementName = "Product" };
            var opt = new SplitXmlFileOptions { ElementCountInEachFile = 20, OutputFileRootNodeName = expectedRootElement };

            var result = XmlTasks.SplitXmlFile(input, opt, new CancellationToken());

            //Check root element
            var rootElementName = ExecuteXpath(result.FilePaths.Last(), "local-name(/*)");
            Assert.AreEqual(expectedRootElement, rootElementName);

            //The Last Product of first file should have id 12
            var id = ExecuteXpath(result.FilePaths.First(), "string(/TestRoot/Product[last()]/id)");
            Assert.AreEqual("12", id);
        }        
    }

    [TestFixture]
    public class CombineXMLTest
    {
        private string _xmlString1;
        private string _xmlString2;
        private readonly XmlDocument _xmlDoc1 = new XmlDocument();
        private readonly XmlDocument _xmlDoc2 = new XmlDocument();

        private CombineXmlInput _input;
        private CombineXmlInputXml[] _inputXmls = new CombineXmlInputXml[2];

        [SetUp]
        public void TestSetup()
        {
            // CombineXML setup
            _xmlString1 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><bar1>foo1</bar1>";
            _xmlString2 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><bar2>foo2</bar2>";
            _xmlDoc1.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><foo1>bar1</foo1>");
            _xmlDoc2.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><foo2>bar2</foo2>");

            _inputXmls[0] = new CombineXmlInputXml { ChildElementName = "XML1" };
            _inputXmls[1] = new CombineXmlInputXml { ChildElementName = "XML2" };

            _input = new CombineXmlInput { InputXmls = _inputXmls, XmlRootElementName = "Root" };
        }

        [TearDown]
        public void Down()
        {
        }

        [Test]
        public async Task ShouldCombineXmlStrings()
        {
            _inputXmls[0].Xml = _xmlString1;
            _inputXmls[1].Xml = _xmlString2;

            var result = await XmlTasks.CombineXml(_input, new CancellationToken());
            Assert.That(result, Is.EqualTo("<Root><XML1><bar1>foo1</bar1></XML1><XML2><bar2>foo2</bar2></XML2></Root>"));
        }

        [Test]
        public async Task ShouldCombineXmlDocs()
        {
            _inputXmls[0].Xml = _xmlDoc1;
            _inputXmls[1].Xml = _xmlDoc2;

            var result = await XmlTasks.CombineXml(_input, new CancellationToken());
            Assert.That(result, Is.EqualTo("<Root><XML1><foo1>bar1</foo1></XML1><XML2><foo2>bar2</foo2></XML2></Root>"));
        }

        [Test]
        public async Task ShouldCombineXmlStringAndXmlDoc()
        {
            _inputXmls[0].Xml = _xmlString1;
            _inputXmls[1].Xml = _xmlDoc1;

            var result = await XmlTasks.CombineXml(_input, new CancellationToken());
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

            var result = await XmlTasks.CombineXml(_input, new CancellationToken());
            Assert.That(result, Is.EqualTo("<NEW_ROOT><NEW_ELEMENT1><bar1>foo1</bar1></NEW_ELEMENT1><NEW_ELEMENT2><bar2>foo2</bar2></NEW_ELEMENT2></NEW_ROOT>"));
        }

        [Test]
        public void ShouldNotCombineOtherObjects()
        {
            _inputXmls[0].Xml = _xmlString1;
            _inputXmls[1].Xml = 123456;
            Assert.ThrowsAsync<FormatException>(() => XmlTasks.CombineXml(_input, new CancellationToken()));

            _inputXmls[0].Xml = new object();
            _inputXmls[1].Xml = _xmlDoc2;
            Assert.ThrowsAsync<FormatException>(() => XmlTasks.CombineXml(_input, new CancellationToken()));

        }
    }

    [TestFixture]
    public class ConvertToXMLTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Down()
        {
        }

        [Test]
        public void TestConvertToXMLUsingFlatFileInputWithoutSeparator()
        {
            var columns = new[]
            {
                new ColumnLength { Length = 3 },
                new ColumnLength { Length = 3 },
                new ColumnLength { Length = 2 },
                new ColumnLength { Length = 4 },
                new ColumnLength { Length = 4 }
            };

            var options = new ConvertToXmlParameters { Input = "asd123as1234asdf" };
            var csvOptions = new ConvertToXmlCsvInputParameters { CSVSeparator = null, ColumnLengths = columns, InputHasHeaderRow = false, TrimOuputColumns = false };
            var result = XmlTasks.ConvertToXml(options, csvOptions, null, new CancellationToken());
            Assert.IsTrue(result.Result.StartsWith("<NewDataSet><Table1><Column1>asd</Column1>"));
        }

        [Test]
        public void TestConvertToXMLUsingSeparatorAndTrim()
        {
            var options = new ConvertToXmlParameters { Input = "asd ;as;asdf" };
            var csvOptions = new ConvertToXmlCsvInputParameters
            {
                CSVSeparator = ";",
                ColumnLengths = null,
                InputHasHeaderRow = false,
                TrimOuputColumns = true
            };
            var result = XmlTasks.ConvertToXml(options, csvOptions, null, new CancellationToken());
            Assert.IsTrue(result.Result.StartsWith("<NewDataSet><Table1><Column1>asd</Column1>"));
        }

        [Test]
        public void TestConvertToXMLUsingJSON()
        {
            var options = new ConvertToXmlParameters { Input = "{\"field1\":\"value1\", \"field2\":\"value2\"}" };
            var jsonInputParameters = new ConvertToXmlJsonInputParameters { XMLRootElementName = "test" };
            var result = XmlTasks.ConvertToXml(options, null, jsonInputParameters, new CancellationToken());
            Assert.IsTrue(result.Result.StartsWith("<test><field1>value1</field1>"));
        }
        [Test]
        public void TestConvertToXMLWithNumericKeys()
        {

            var options = new ConvertToXmlParameters { Input = "{\"48\":\"value1\", \"2\":\"value2\"}" };
            var jsonInputParameters = new ConvertToXmlJsonInputParameters { XMLRootElementName = "test", AppendToFieldName = "foo" };
            var result = XmlTasks.ConvertToXml(options, null, jsonInputParameters, new CancellationToken());
            Assert.IsTrue(result.Result.StartsWith("<test><foo48>value1</foo48>"));
        }

        [Test]
        public void TestConvertToXMLWithNumericKeysWithoutAppend()
        {
            var options = new ConvertToXmlParameters { Input = "{\"1\":\"value1\", \"2\":\"value2\"}" };
            var jsonInputParameters = new ConvertToXmlJsonInputParameters { XMLRootElementName = "test", AppendToFieldName = null };

            var result = XmlTasks.ConvertToXml(options, null, jsonInputParameters, new CancellationToken());
            Assert.IsTrue(result.Result.StartsWith("<test><_x0031_>value1</_x0031_>"));

        }
    }

    [TestFixture]
    public class ConvertXmlToCsvTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Down()
        {
        }

        [Test]
        public void TestConvertXmlToCsv()
        {
            var indata = new ConvertXmlToCsvInput
            {
                InputXmlString = TestFiles.ConvertXmlToCsvTestData.TestXml,
                CsvSeparator = ",",
                IncludeHeaders = true
            };

            var result = XmlTasks.ConvertXmlToCsv(indata, new CancellationToken());
            Assert.AreEqual(TestFiles.ConvertXmlToCsvTestData.ExpectedCsvResult, result.Result);
        }

        [Test]

        public void TestConvertXmlToCsvWithSpecialCharacters()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var indata = new ConvertXmlToCsvInput
                {
                    InputXmlString = "<root><v1>foo1</v1><v2>bar2,bar2</v2><v3>baz3\nbaz3</v3><v4>\"fo\"o4\"</v4></root>",
                    CsvSeparator = ";",
                    IncludeHeaders = true
                };
                var result = XmlTasks.ConvertXmlToCsv(indata, new CancellationToken());
                Assert.AreEqual("v1;v2;v3;v4\nfoo1;bar2,bar2;\"baz3\nbaz3\";\"\"\"fo\"\"o4\"\"\"\n", result.Result);
            }
            else
            {
                var indata = new ConvertXmlToCsvInput
                {
                    InputXmlString = "<root><v1>foo1</v1><v2>bar2,bar2</v2><v3>baz3\r\nbaz3</v3><v4>\"fo\"o4\"</v4></root>",
                    CsvSeparator = ";",
                    IncludeHeaders = true
                };
                var result = XmlTasks.ConvertXmlToCsv(indata, new CancellationToken());
                Assert.AreEqual("v1;v2;v3;v4\r\nfoo1;bar2,bar2;\"baz3\nbaz3\";\"\"\"fo\"\"o4\"\"\"\r\n", result.Result);
            }
        }

        [Test]
        public void TestConvertToCsv_Xml_WithWrongFileType_ShouldThrowAnException()
        {
            var indata = new ConvertXmlToCsvInput
            {
                InputXmlString = TestFiles.ConvertXmlToCsvTestData.TestJson,
                CsvSeparator = ",",
                IncludeHeaders = true
            };

            Assert.Throws<XmlException>(() => XmlTasks.ConvertXmlToCsv(indata, new CancellationToken()));
        }
    }

    [TestFixture]
#if !_WINDOWS
    [Ignore("No .pfx file")] // Signature creation not working in Linux.
#endif
    public class SigningTaskTest
    {
        private readonly string _certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "certwithpk.pfx");
        private readonly string _privateKeyPassword = "password";

        [SetUp]
        public void Setup()
        {
            TestFiles.CreateSignatureFile.GenerateSignatureFile(_certificatePath, _privateKeyPassword);
        }

        [TearDown]
        public void Down()
        {
            File.Delete(_certificatePath);
        }

        [Test]
        public void SignXml_ShouldSignXmlStringWithPrivateKeyCertificate()
        {
            var input = new SignXmlInput
            {
                CertificatePath = _certificatePath,
                PrivateKeyPassword = _privateKeyPassword,
                SigningStrategy = SigningStrategyType.PrivateKeyCertificate,
                XmlInputType = XmlParamType.XmlString,
                XmlEnvelopingType = XmlEnvelopingType.XmlEnvelopedSignature,
                Xml = "<root><value>foo</value></root>"
            };
            var output = new SignXmlOutput
            {
                OutputType = XmlParamType.XmlString
            };
            var options = new SignXmlOptions
            {
                DigestMethod = DigestMethod.SHA256,
                TransformMethods = new[] { TransformMethod.DsigExcC14 },
                XmlSignatureMethod = XmlSignatureMethod.RSASHA256
            };

            SigningResult result = XmlTasks.SignXml(input, output, options, new CancellationToken());

            StringAssert.Contains("<Signature", result.Result);
        }

        [Test]
        public void SignXml_ShouldSignXmlFileWithPrivateKeyCertificate()
        {
            // create file
            string xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", Guid.NewGuid().ToString() + ".xml");
            File.WriteAllText(xmlFilePath, @"<root>
    <value>foo</value>
</root>");
            var input = new SignXmlInput
            {
                CertificatePath = _certificatePath,
                PrivateKeyPassword = _privateKeyPassword,
                SigningStrategy = SigningStrategyType.PrivateKeyCertificate,
                XmlEnvelopingType = XmlEnvelopingType.XmlEnvelopedSignature,
                XmlInputType = XmlParamType.File,
                XmlFilePath = xmlFilePath
            };
            var output = new SignXmlOutput
            {
                OutputType = XmlParamType.File,
                OutputFilePath = xmlFilePath.Replace(".xml", "_signed.xml"),
                OutputEncoding = "utf-8"
            };
            var options = new SignXmlOptions
            {
                DigestMethod = DigestMethod.SHA256,
                TransformMethods = new[] { TransformMethod.DsigExcC14 },
                XmlSignatureMethod = XmlSignatureMethod.RSASHA256,
                PreserveWhitespace = true
            };

            SigningResult result = XmlTasks.SignXml(input, output, options, new CancellationToken());
            var signedXml = File.ReadAllText(result.Result);

            StringAssert.Contains("<Signature", signedXml);
            StringAssert.DoesNotContain("<Signature", File.ReadAllText(xmlFilePath));

            // cleanup
            File.Delete(xmlFilePath);
            File.Delete(result.Result);
        }

        [Test]
        public void SignXml_ShouldAddSignatureToSourceFile()
        {
            // create file
            string xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", Guid.NewGuid().ToString() + ".xml");
            File.WriteAllText(xmlFilePath, @"<root>
    <value>foo</value>
</root>");
            var input = new SignXmlInput
            {
                CertificatePath = _certificatePath,
                PrivateKeyPassword = _privateKeyPassword,
                SigningStrategy = SigningStrategyType.PrivateKeyCertificate,
                XmlEnvelopingType = XmlEnvelopingType.XmlEnvelopedSignature,
                XmlInputType = XmlParamType.File,
                XmlFilePath = xmlFilePath
            };
            var output = new SignXmlOutput
            {
                OutputType = XmlParamType.File,
                AddSignatureToSourceFile = true
            };
            var options = new SignXmlOptions
            {
                DigestMethod = DigestMethod.SHA256,
                TransformMethods = new[] { TransformMethod.DsigExcC14 },
                XmlSignatureMethod = XmlSignatureMethod.RSASHA256,
                PreserveWhitespace = true
            };

            SigningResult result = XmlTasks.SignXml(input, output, options, new CancellationToken());
            var signedXml = File.ReadAllText(result.Result);

            StringAssert.Contains("<Signature", signedXml);

            // cleanup
            File.Delete(xmlFilePath);
            File.Delete(result.Result);
        }
    }

    [TestFixture]
#if !_WINDOWS
    [Ignore("No .pfx file")] // Signature creation not working in Linux.
#endif
    public class VerifyTaskTest
    {
        private readonly string _certificatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles", "certwithpk.pfx");
        private readonly string _privateKeyPassword = "password";

        [SetUp]
        public void Setup()
        {

            TestFiles.CreateSignatureFile.GenerateSignatureFile(_certificatePath, _privateKeyPassword);
        }

        [TearDown]
        public void Down()
        {
            File.Delete(_certificatePath);
        }

        [Test]
        public void VerifySignedXml_ShouldVerifySignedXmlString()
        {
            var input = new SignXmlInput
            {
                CertificatePath = _certificatePath,
                PrivateKeyPassword = _privateKeyPassword,
                SigningStrategy = SigningStrategyType.PrivateKeyCertificate,
                XmlInputType = XmlParamType.XmlString,
                XmlEnvelopingType = XmlEnvelopingType.XmlEnvelopedSignature,
                Xml = "<root><foo>bar</foo></root>"
            };
            var output = new SignXmlOutput
            {
                OutputType = XmlParamType.XmlString
            };
            var options = new SignXmlOptions
            {
                DigestMethod = DigestMethod.SHA256,
                TransformMethods = new[] { TransformMethod.DsigExcC14 },
                XmlSignatureMethod = XmlSignatureMethod.RSASHA256
            };
            string signedXml = XmlTasks.SignXml(input, output, options, new CancellationToken()).Result;
            var verifyInput = new VerifySignatureInput
            {
                XmlInputType = XmlParamType.XmlString,
                Xml = signedXml
            };

            var result = XmlTasks.VerifySignedXml(verifyInput, new VerifySignatureOptions(), new CancellationToken());

            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void VerifySignedXml_ShouldVerifySignedXmlDocument()
        {
            var input = new VerifySignatureInput
            {
                XmlInputType = XmlParamType.File,
                XmlFilePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..", "TestFiles", "signed.xml"))
            };
            var options = new VerifySignatureOptions
            {
                PreserveWhitespace = true
            };

            var result = XmlTasks.VerifySignedXml(input, options, new CancellationToken());

            Assert.IsTrue(result.IsValid);
        }
    }
}
