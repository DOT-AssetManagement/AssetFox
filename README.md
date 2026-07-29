# AssetFox

AssetFox is a web-based decision-support application for transportation agencies. It helps teams manage infrastructure assets, model deterioration, compare maintenance strategies, plan investments, and evaluate long-term outcomes.

## Features

- Manage asset networks, attributes, and condition data
- Define performance curves, treatments, budgets, and planning goals
- Run multi-year investment and maintenance scenarios
- Monitor simulations and report generation in real time
- Review recommended work, projected conditions, and funding needs
- Create reports and export analysis results

## Technology

AssetFox uses an ASP.NET Core Web API running on .NET 10, with Entity Framework Core and SQL Server for data access. The frontend is built with Vue 3, TypeScript, Vite, Vuetify, and PrimeVue. REST, GraphQL, and SignalR support communication between the frontend and backend.

## Project structure

- `AssetFoxCore` — backend API
- `VuejsApp` — web frontend
- `AssetFox.Core.*` — analysis, data, reporting, and shared libraries
- `AssetFoxCore.sln` — primary Visual Studio solution

## Development

Local development requires the .NET 10 SDK, Node.js with npm, and access to SQL Server. Environment-specific database, authentication, and encryption settings are excluded from source control and must be obtained from the project team.
