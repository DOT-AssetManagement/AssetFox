Welcome to AssetFox!
AssetFox is developed and maintained by PennDOT Asset Management to help transportation owners make smarter, more strategic infrastructure decisions. Built for State DOTs, agencies, and municipalities, the platform delivers powerful tools to optimize investment, evaluate treatment strategies, and forecast asset conditions far into the future.

Originally engineered for bridge and pavement networks, AssetFox is fully asset‑agnostic. Its flexible architecture allows any organization to model and manage any asset class — from trucks and pipes to buildings and facilities — using the same modern, data‑driven framework.

AssetFox gives owners the ability to:

Understand long‑term performance trends
Compare investment scenarios
Optimize treatment timing and funding
Support TAM‑aligned planning and reporting

Welcome aboard — your assets have a smarter future ahead.

---

## Table of contents

- [Overview](#overview)
- [Features](#features)
- [Technology](#technology)
- [Project structure](#project-structure)
- [Development](#development)
- [Releases](#releases)
- [Contributing](#contributing)
- [License](#license)
- [Contacts](#contacts)

---

## Overview

AssetFox is a web-based decision-support application for transportation agencies. It helps teams manage infrastructure assets, model deterioration, compare maintenance strategies, plan investments, and evaluate long-term outcomes.

## Features

- Manage asset networks, attributes, and condition data
- Define performance curves, treatments, budgets, and planning goals
- Run multi-year investment and maintenance scenarios
- Monitor simulations and report generation in real time
- Review recommended work, projected conditions, and funding needs
- Create reports and export analysis results
- Extensible to non-transportation assets

> Note: Capabilities may evolve during development.

## Technology

AssetFox uses an ASP.NET Core Web API running on .NET 10, with Entity Framework Core and SQL Server for data access. The frontend is built with Vue 3, TypeScript, Vite, Vuetify, and PrimeVue. REST, GraphQL, and SignalR support communication between the frontend and backend.

## Project structure

- `AssetFoxCore` — backend API
- `VuejsApp` — web frontend
- `AssetFox.Core.*` — analysis, data, reporting, and shared libraries
- `AssetFoxCore.sln` — primary Visual Studio solution

## Development

Local development requires the .NET 10 SDK, Node.js with npm, and access to SQL Server. Environment-specific database, authentication, and encryption settings are excluded from source control and must be obtained from the project team.

## Releases
To find the latest tagged release, check the Releases section on GitHub.

## Contributing
We welcome issues and pull requests! To contribute, follow these steps:

- Fork the repository.
- Create a feature branch for your changes.
- Commit your changes with clear, descriptive messages.
- Open a pull request and describe the changes you've made and the reasoning behind them.

## License
This repository includes a LICENSE file. Please refer to this file for the specific terms and conditions.

## Contacts
For program information and collaboration inquiries, you can reach out to:

PennDOT Asset Management
Justin Bruner, P.E. — jbruner@pa.gov
