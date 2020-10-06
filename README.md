# Frends.Community.Xml

Frends Community Task library for XML operations

[![Actions Status](https://github.com/CommunityHiQ/Frends.Community.Xml/workflows/PackAndPushAfterMerge/badge.svg)](https://github.com/CommunityHiQ/Frends.Community.Xml/actions) ![MyGet](https://img.shields.io/myget/frends-community/v/Frends.Community.Xml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) 

- [Installing](#installing)
- [Tasks](#tasks)
     - [SplitXMLFile](#SplitXMLFile)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing

You can install the Task via frends UI Task View or you can find the NuGet package from the following NuGet feed
https://www.myget.org/F/frends-community/api/v3/index.json and in Gallery view in MyGet https://www.myget.org/feed/frends-community/package/nuget/Frends.Community.Xml

# Tasks

## SplitXMLFile

Splits XML file into smaller XML files. This allows processing bigger (>2GB) XML files that otherwise could cause performance issues.

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

### Returns SplitXMLFileResult

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
