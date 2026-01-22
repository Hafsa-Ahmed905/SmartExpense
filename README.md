# 💰 Smart Expense Management System

Full-stack web application for personal finance management built with ASP.NET Core 6.0.

## Features

- **Transaction Management** - Track income and expenses with categories, filters, and export options
- **Budget Tracking** - Set monthly budgets with automated alerts at 75%, 90%, and 100% thresholds
- **Real-time Notifications** - Live updates using SignalR for transactions and budget warnings
- **Analytics Dashboard** - Interactive charts showing spending patterns, trends, and insights
- **User Authentication** - Secure login with session management and persistent cookies
- **Customization** - Dark mode, notification preferences, and personalized settings

## Tech Stack

- ASP.NET Core 6.0
- Entity Framework Core
- SignalR
- Chart.js
- Bootstrap 5
- SQL Server

## Quick Start

```bash
git clone https://github.com/Hafsa-Ahmed905/SmartExpense.git
cd smart-expense-management
dotnet restore
dotnet ef database update
dotnet run
```

Navigate to `https://localhost:5001`
