# DICOMAnonymizer

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) <!-- Optional: Add a license badge -->

A C# command-line application for anonymizing DICOM (Digital Imaging and Communications in Medicine) files. Designed with Clean Architecture principles for maintainability, testability, and extensibility.

## Table of Contents

-   [Introduction](#introduction)
-   [Features](#features)
-   [Architecture](#architecture)
-   [Getting Started](#getting-started)
    -   [Prerequisites](#prerequisites)
    -   [Building the Application](#building-the-application)
-   [Usage](#usage)
-   [Anonymization Rules](#anonymization-rules)
-   [Testing](#testing)
-   [Dependencies](#dependencies)
-   [Contributing](#contributing)
-   [License](#license)

## Introduction

This application anonymizes DICOM images by removing or modifying sensitive patient information, making them suitable for research, development, and training purposes while protecting patient privacy.

## Features

-   Anonymizes DICOM files in a directory.
-   Uses configuration-based anonymization rules.
-   Applies default anonymization for unconfigured tags.
-   Built with Clean Architecture for maintainability.
-   Includes unit tests for core components.
-   Uses `fo-dicom` library for DICOM file parsing and manipulation.

## Architecture

The application is structured using Clean Architecture, which promotes separation of concerns:

-   **Core:** Contains the core business logic and domain models (DICOM tags, anonymization rules).
-   **Application:** Orchestrates the core logic and defines use cases (e.g., "Anonymize a Folder").
-   **Infrastructure:** Implements the interfaces defined in the Application layer, dealing with specific technologies (e.g., `fo-dicom`, file system).
-   **CommandLine:** The command-line interface for interacting with the application.

## Getting Started

### Prerequisites

-   [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download) or later
-   Visual Studio 2022 or later (or another compatible IDE)

### Building the Application

1.  Clone the repository:
    ```bash
    git clone [your-repository-url]
    ```
2.  Navigate to the solution directory:
    ```bash
    cd DICOMAnonymizer
    ```
3.  Build the solution:
    ```bash
    dotnet build
    ```
    or open the `DICOMAnonymizer.sln` in Visual Studio and build the solution.
4.  Publish the application:
    ```bash
    dotnet publish DICOMAnonymizer.CommandLine -c Release -o publish
    ```

## Usage

The application is a command-line tool. To run it:

1.  Navigate to the publish directory (e.g., `DICOMAnonymizer/DICOMAnonymizer.CommandLine/bin/Release/net8.0/publish`).
2.  Run the executable:

    ```bash
    DICOMAnonymizer.CommandLine.exe --InputFolder <input-folder> --OutputFolder <output-folder>
    ```

    *   `--InputFolder`: The path to the directory containing the DICOM files to anonymize.
    *   `--OutputFolder`: The path to the directory where the anonymized DICOM files will be saved.

## Anonymization Rules

The anonymization rules are defined in the `AnonymizationConfiguration` class. The default configuration includes rules for:

-   Patient Name (set to "DEFAULT NAME")
-   Patient Birth Date (subtract a random number of days)
-   [Add more rules here as you implement them]

## Testing

To run the unit tests:

1.  Navigate to the `DICOMAnonymizer.Tests` directory.
2.  Run the tests:

    ```bash
    dotnet test
    ```

    or use Visual Studio's Test Explorer.

## Dependencies

-   [fo-dicom](https://github.com/fo-dicom/fo-dicom): A DICOM toolkit for .NET.
-   Microsoft.Extensions.Hosting
-   Microsoft.Extensions.Logging.Console

## Contributing

Contributions are welcome! Please feel free to submit pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
