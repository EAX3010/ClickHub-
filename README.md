# ClickHub: High-Performance Click Tracking System

[![Made with .NET](https://img.shields.io/badge/Made%20with-.NET-512BD4?style=flat-square&logo=.net)](https://dotnet.microsoft.com/)
[![Powered by Blazor](https://img.shields.io/badge/Powered%20by-Blazor-512BD4?style=flat-square&logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![Runs on MySQL](https://img.shields.io/badge/Runs%20on-MySQL-4479A1?style=flat-square&logo=mysql)](https://www.mysql.com/)

ClickHub is a high-performance click tracking and domain management system built with Blazor .NET Core 8. Designed for efficiency and scalability, it's ideal for ad tracking, analytics, and domain management scenarios.

## Key Features

- Fast, non-blocking redirects
- Asynchronous click data processing
- Domain-based configuration management
- Background processing of click data
- MySQL database integration
- DevExpress Blazor components for the admin interface
- Flexible data export options (XLSX, XLS, CSV)

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- MySQL Server
- DevExpress Blazor (for the admin interface)

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/clickhub.git
   cd clickhub
   ```

2. Configure the database:
   - Create a new MySQL database for ClickHub
   - Update the connection string in `appsettings.json`:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Server=your_server;Database=clickhub;Uid=your_username;Pwd=your_password;"
       }
     }
     ```

3. Build and run the project:
   ```
   dotnet build
   dotnet run
   ```

The application will be available at `https://localhost:5001`.

## Usage

### Tracking Endpoint

The main tracking endpoint is available at `/track`. It accepts the following query parameters:

- `id`: Domain ID (required)
- `ccpturl`: Click-through URL (required)
- `adpos`: Ad position
- `locphisical`: Physical location
- `locinterest`: Interest-based location
- `adgrp`: Ad group
- `kw`: Keyword
- `nw`: Network
- `url`: Landing page URL
- `cpn`: Campaign
- `device`: Device type
- `pl`: Placement

Example tracking URL:
```
https://yourdomain.com/track?id=1&ccpturl=https://example.com&adpos=top&kw=example
```

### Admin Interface

The admin interface is accessible at the root URL (`/`). It provides functionality to:

- Manage domains (add, update, delete)
- View tracking statistics
- Analyze click data

## Data Export Capabilities

ClickHub provides robust data export functionality, allowing users to easily extract and analyze click data. The following export options are available:

### Export to Excel (XLSX)

```csharp
async Task ExportXlsx_Click()
{
    await Grid.ExportToXlsxAsync("ExportResult", new GridXlExportOptions()
    {
        ExportSelectedRowsOnly = ExportSelectedRowsOnly,
        CustomizeCell = OnCustomizeCell
    });
}
```

### Export to Excel 97-2003 (XLS)

```csharp
async Task ExportXls_Click()
{
    await Grid.ExportToXlsAsync("ExportResult", new GridXlExportOptions()
    {
        ExportSelectedRowsOnly = ExportSelectedRowsOnly,
        CustomizeCell = OnCustomizeCell
    });
}
```

### Export to CSV

```csharp
async Task ExportCsv_Click()
{
    await Grid.ExportToCsvAsync("ExportResult", new GridCsvExportOptions()
    {
        ExportSelectedRowsOnly = ExportSelectedRowsOnly
    });
}
```

These export functions allow users to:
- Export data in XLSX, XLS, and CSV formats
- Choose to export all data or only selected rows
- Customize cell formatting and content (for Excel exports)

The export functionality enhances data analysis capabilities, enabling users to easily integrate ClickHub data with other tools and systems.

## Performance Optimization

ClickHub is optimized for high-performance scenarios:

- Uses channels for efficient communication between web and background services
- Implements fast, non-blocking redirects
- Utilizes background processing for data handling
- Employs caching for domain configurations
- Optimized database queries with proper indexing

## Roadmap

We have an exciting development plan for ClickHub! Here's a quick overview of our plans:

- Short-term: User authentication, enhanced data visualization, performance optimizations
- Medium-term: Public API, real-time analytics, multi-database support
- Long-term: Microservices architecture, multi-tenancy, advanced fraud detection

For a more detailed look at our plans and progress, check out our [full roadmap](ROADMAP.md) and our [project board](https://github.com/yourusername/clickhub/projects).

We welcome community input and contributions to help shape the future of ClickHub!

## Contributing

We welcome contributions to ClickHub! If you're interested in helping improve the project, here are some ways you can contribute:

- Report bugs or suggest features by opening issues
- Submit pull requests for bug fixes or new features
- Improve documentation
- Share your experience using ClickHub

Please read our [Contributing Guidelines](CONTRIBUTING.md) for more information on how to get started.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- DevExpress for their Blazor components
- Dapper for efficient database access
- MySqlConnector for MySQL integration

---

For more detailed information, please refer to the [Documentation](docs/index.md).