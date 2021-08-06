# Frends.Community.Xml

Frends Community Task library for XML operations

[![Actions Status](https://github.com/CommunityHiQ/Frends.Community.Xml/workflows/PackAndPushAfterMerge/badge.svg)](https://github.com/CommunityHiQ/Frends.Community.Xml/actions) ![MyGet](https://img.shields.io/myget/frends-community/v/Frends.Community.Xml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) 

- [Installing](#installing)
- [Tasks](#tasks)
     - [CombineXML](#CombineXML)
     - [ConvertToXML](#ConvertToXML)
     - [ConvertXmlToCsv](#ConvertXmlToCsv)
     - [SplitXMLFile](#SplitXMLFile)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing

You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed
https://www.myget.org/F/frends-community/api/v3/index.json and in Gallery view in MyGet https://www.myget.org/feed/frends-community/package/nuget/Frends.Community.Xml

# Tasks

## CombineXML
Combines two or more xml strings or xml documents to one xml string

### Properties

#### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| InputXmls | ``array[InputXml]`` | Xml strings or xml documents that will be merged | n/a |
| XmlRootElementName| ``string`` | Root element of xml | `Root` |

#### InputXml

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Xml| ``object`` | Xml input as string or xml document | `<note><body>Hello!</body></note>` |
| ChildElementName| ``string`` | Child element name where the xml document will be written in| `ChildElement1` |


### Returns

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| Xml | ``string`` | Combined xml as string | ``<root><child1><note1><body1>Hello!</body1></note1></child1><child2><note2<body2>Hello2!</body2></note2></child2></root>``

## ConvertToXML

### Parameters

| Property				|  Type   | Description								| Example                     |
|-----------------------|---------|-----------------------------------------|-----------------------------|
| Input					| ``string`` | Supported formats JSON, CSV and fixed length | `first;second;third` |

### CsvInputParameters

| Property				|  Type   | Description								| Example                     |
|-----------------------|---------|-----------------------------------------|-----------------------------|
| CSVSeparator			| ``string`` | CSV separator	| `;` |
| ColumnLengths			| ``array<int>`` | Column lengths of fixed lenght input. These are used if CSV separator is not defined.	|  |
| InputHasHeaderRow		| ``bool`` | Input has header row	| `false` |
| TrimOuputColumns		| ``bool`` | Trim ouput columns of CVS input	| `true` |

### JsonInputParameters

| Property				|  Type   | Description								| Example                     |
|-----------------------|---------|-----------------------------------------|-----------------------------|
| XMLRootElementName	| ``string`` | Root name for when parsing JSON| `Root`	|
| AppendWith			| ``string`` | Append numeric JSON fields with prefix	| `foo_` |

### Result

| Property      | Type     | Description                      | Example                     |
|---------------|----------|----------------------------------|-----------------------------|
| Result        | ``string`` | Result as XML	| `<NewDataSet><Table1><Column1>first</Column1><Column2>second</Column2><Column3>third</Column3></Table1></NewDataSet>` |

## ConvertXmlToCsv

### Input

| Property				|  Type   | Description								| Example                     |
|-----------------------|---------|-----------------------------------------|-----------------------------|
| InputData				| ``string`` | XML string to be converted into csv. | See below. |
| CsvSeparator			| ``string`` | Separator for the output columns.	| `;` |
| IncludeHeaders		| ``bool`` | True if the column headers should be included into the output	| `true` |

Example input:
```
<root>
	<row id='1'>
		<name>Google</name>
		<url>https://en.wikipedia.org/wiki/Google</url>
		<fancy_characters>comma (,) inside field</fancy_characters>
	</row>
	<row id='2'>
		<name>Apple</name>
		<url>https://en.wikipedia.org/wiki/Apple_Inc.</url>
		<fancy_characters>Kanji </fancy_characters>
	</row>
	<row id='3'>
		<name>Missing columns</name>
	</row>
</root>
```

### Result

| Property      | Type     | Description                      | Example                     |
|---------------|----------|----------------------------------|-----------------------------|
| Result        | ``string``   | Result as CSV	| See below. |

Example output, for input given above, with comma as a delimeter and headers included:
```
name,url,fancy_characters,id
Google,https://en.wikipedia.org/wiki/Google,"comma (,) inside field",1
Apple,https://en.wikipedia.org/wiki/Apple_Inc.,Kanji ,2
Missing columns,,,3
```

If input XML string contains multiple fields with same name, they are omited. Also rows must be in element with same name. If id is not given for row as a attribute filed named rowname_Id is added, with row number. 

For example, following XML:
```
<table>
	<foo>
		<bar>700</bar>
		<foobar>12</foobar>
	</foo>
	<foo>
		<bar>800</bar>
		<bar>800</bar>
		<foobar>5</foobar>
	</foo>
	<invalid>
		<bar>200</bar>
		<foobar>7</foobar>
	</invalid>
</table>
```

is thus converted to, using comma as a delimeter and headers included:

```
foo_Id,foobar
0,12
1,5
```

## SplitXMLFile

Splits XML file into smaller XML files. This allows processing bigger (>2GB) XML files that otherwise could cause performance issues.

Example input XML file: 
```xml
<root>
    <Product>
        <id>1</id>
    </Product>
    <Product>
        <id>2</id>
    </Product>
    <Product>
        <id>3</id>
    </Product>
</root>
```

Example output files when value of ElementCountInEachFile is 2:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<root>
  <Product>
        <id>1</id>
    </Product>
  <Product>
        <id>2</id>
    </Product>
</root>
```

```xml
<?xml version="1.0" encoding="UTF-8"?>
<root>
  <Product>
        <id>3</id>
    </Product>
</root>
```

### Input

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| InputFilePath | `string` | Path of input file. | `F:\myfile.xml` |
| SplitAtElementName | `string` |  Name of the XML elements which are copied to output files. | `Product` |
| OutputFilesDirectory | `string` | Output directory for new files. | `F:\output` |

### Options

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| ElementCountInEachFile | `int` | Maximum number of chosen elements to be written in each file. | `5000` |
| OutputFileRootNodeName | `string` | Root element of output file. | `Root` |

### Returns

A result object with parameters.

| Property | Type | Description | Example |
| -------- | -------- | -------- | -------- |
| FilePaths | `List<string>` | List of new files. | `"F:\output\myfile.xml.0.part","F:\output\myfile.xml.1.part"` |

# Building

Clone a copy of the repository

`git clone https://github.com/CommunityHiQ/Frends.Community.Xml.git`

Rebuild the project

`dotnet build`

Run tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`

# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repository on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

# Change Log

| Version | Changes |
| ------- | ------- |
| 1.0.0   | First version. Includes task SplitXMLFile |
| 2.0.0   | CombineXML and ConvertToXML tasks included. |
