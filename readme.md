# Salesforce to Azure Service Bus Function App

## Overview

This Azure Function queries Salesforce data every 30 minutes and sends the JSON result to an Azure Service Bus queue named `DevTest`.

---

## Features

- Timer-triggered Azure Function (every 30 minutes)
- Salesforce REST API Integration using SOQL
- OAuth2-based Salesforce Authentication
- Azure Service Bus Queue Integration
- Structured and logged processing

---

## Requirements

- .NET 8 SDK or latest
- Azure Function Core Tools
- Azure Subscription (for Service Bus)
- Salesforce Developer Account and App Registration

---

## Configuration

Update `local.settings.json` with:


"ClientId": "",
"ClientSecret": "",
"Username": "",
"Password": "",
"SecurityToken": "",
"LoginUrl": "https://login.salesforce.com/services/oauth2/callback",
"InstanceUrl": "https://orgfarm-33d88c2b67-dev-ed.develop.lightning.force.com/lightning/page/home",
"ServiceBusConnectionString": ""
