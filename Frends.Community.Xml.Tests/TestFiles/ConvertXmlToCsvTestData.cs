using System;

namespace Frends.Community.Xml.Tests.TestFiles
{
    public static class ConvertXmlToCsvTestData
    {
        private static string _testXml;
        private static string _expectedCsvResult;
        private static string _testJSON;

        public static string TestXml
        {
            get
            {
                if (string.IsNullOrEmpty(_testXml))
                {
                    _testXml = @"<DocumentElement>
                        <dbo.CDInformation>
                            <Title>asd</Title>
                            <Artist>asd</Artist>
                            <Country>asd</Country>
                            <Company>asd</Company>
                            <Price>asd</Price>
                            <Year>asd</Year>
                          </dbo.CDInformation>
                          <dbo.CDInformation>
                            <Title>1234132</Title>
                            <Artist>123123</Artist>
                            <Country>123123</Country>
                            <Company>12312</Company>
                            <Price>312312312</Price>
                            <Year>314234</Year>
                          </dbo.CDInformation>
                        </DocumentElement>";
                }
                return _testXml;
            }
        }

        public static string ExpectedCsvResult
        {
            get
            {
                if (string.IsNullOrEmpty(_expectedCsvResult))
                {
                    _expectedCsvResult = "Title,Artist,Country,Company,Price,Year" +
                        Environment.NewLine +
                        "asd,asd,asd,asd,asd,asd" +
                        Environment.NewLine +
                        "1234132,123123,123123,12312,312312312,314234" +
                        Environment.NewLine;
                }
                return _expectedCsvResult;
            }
        }

        public static string TestJson
        {
            get
            {
                if (string.IsNullOrEmpty(_testJSON))
                {
                    _testJSON = @"{
                        'dbo.CDInformation': [
                          {
                            'Title': 'asd',
                            'Artist': 'asd',
                            'Country': 'asd',
                            'Company': 'asd',
                            'Price': 'asd',
                            'Year': 'asd'
                          },
                          {
                            'Title': '1234132',
                            'Artist': '123123',
                            'Country': '123123',
                            'Company': '12312',
                            'Price': '312312312',
                            'Year': '314234'
                          }
                        ]
                    }";
                }
                return _testJSON;
            }
        }
    }
}
