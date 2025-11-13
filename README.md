# Contract Monthly Claim System (CMCS)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-purple?style=for-the-badge&logo=.net)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-07405E?style=for-the-badge&logo=sqlite&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)

##  Project Overview
The **Contract Monthly Claim System (CMCS)** is a comprehensive web application developed for PROG6212 Portfolio of Evidence. This system modernizes the claim management process for academic institutions, providing an efficient platform for lecturers to submit monthly claims and for administrators to review and approve them.

##  Key Features
- **Role-Based Access** - Separate interfaces for Lecturers, Programme Coordinators, and Academic Managers
- **Smart File Upload** - Drag & drop file upload with validation (PDF, DOCX, XLSX, images)
- **Professional UI** - Purple academic theme with dark/light mode toggle
- **Real-time Dashboard** - Live statistics and status tracking
- **Approval Workflow** - Streamlined claim review and approval process
- **Responsive Design** - Optimized for desktop and mobile devices

##  Technology Stack
- **Frontend**: ASP.NET Core MVC, Bootstrap 5, JavaScript
- **Backend**: ASP.NET Core 8.0, C#
- **Database**: Entity Framework Core, SQLite
- **Styling**: Custom CSS with CSS Variables, Font Awesome Icons

##  Getting Started
### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git

### Installation
1. **Clone the repository**
   git clone https://github.com/ST10345327/PROG6212-POE-Part2.git
   cd CMCS.WebApp
   
2. **Restore dependencies**
   dotnet restore
   
3. **Run the application**
   bash
   dotnet run
  
4. **Access the application**
   - Open: `https://localhost:5133`
   - Default port may vary

##  User Roles
###  Lecturers
- Submit monthly claims with supporting documents
- Track claim status in real-time
- View submission history

###  Programme Coordinators
- Review pending claims
- Approve or reject submissions
- Monitor claim statistics

###  Academic Managers
- Final approval authority
- Comprehensive oversight
- Analytics and reporting

##  Project Structure
CMCS.WebApp/
├── Controllers/          # MVC Controllers
├── Models/              # Data Models
├── Views/               # Razor Views
├── Data/                # Database Context
├── wwwroot/             # Static Assets
│   ├── css/             # Stylesheets
│   ├── js/              # JavaScript
│   └── uploads/         # File Storage
└── Program.cs           # Application Entry Point

##  UI Features
- **Purple Academic Theme** - Professional color scheme
- **Dark/Light Mode** - User preference persistence
- **Responsive Design** - Mobile-first approach
- **Interactive Elements** - Smooth transitions and feedback

##  Technical Features
- **Entity Framework Core** - Modern ORM with migrations
- **Dependency Injection** - Clean architecture
- **Model Validation** - Client and server-side validation
- **Error Handling** - Global exception handling
- **Security** - Anti-forgery tokens and input sanitization

##  Database Design
The system uses a relational database with:
- **Users** - Role-based access control
- **Claims** - Core claim entity with status tracking
- **SupportingDocuments** - File metadata and storage
- **Approvals** - Audit trail for workflow

##  Academic Context
This project was developed as part of **PROG6212 - GUI Development** Portfolio of Evidence, demonstrating proficiency in:
- C# and ASP.NET Core MVC development
- Database design with Entity Framework
- User interface design and implementation
- Software engineering best practices

##  Developer
- **Student**: ST10345327
- **Course**: PROG6212 - GUI Development
- **Institution**: IIE Rosebank College

##  License
This project is developed for academic purposes as part of PROG6212 Portfolio of Evidence.

**Built for PROG6212 Portfolio of Evidence**
