# Bliss Recruitment Candidate Solution

## Prerequirements
* Jetbrains Rider or Visual Studio 2019 (or higher)
* .NET Core SDK (version 6.0 or higher)
* PostgreSQL

## Setup
* `PostgreSQL`: Download postgresSQL [here](https://www.postgresql.org/download/) if not 
installed on your pc and follow instructions [here](https://www.postgresql.org/docs/current/tutorial-install.html)
to set it up.
* `.NET 6`: Download .NET 6 [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) and proceed to your OS link for installation guide
  * Windows Installation Guide: https://learn.microsoft.com/en-us/dotnet/core/install/windows?tabs=net60
  * Linux Installation Guide: https://learn.microsoft.com/en-us/dotnet/core/install/linux
  * macOS Installation Guide: https://learn.microsoft.com/en-us/dotnet/core/install/macos
* `IDEs`: Click [here](https://www.jetbrains.com/rider/download/) to download Jetbrains Rider and 
[here](https://visualstudio.microsoft.com/downloads/) for Visual Studio 2022
* `SMTP` with Gmail: Follow this [link](https://support.google.com/mail/answer/185833?hl=en-GB) to create an 
app password from your gmail account. Update the appsettings with the credentials in order to send emails.

## How To Run
* Open solution in Jetbrains Rider or Visual Studio
* Build the solution.
* Run the <b>BlissRecruitment.Api</b> project.

### NB
* When the database connection is setup and configured successfully in appsettings, the database and tables will be
created automatically on the first start of the application
* There is a Postman collection included in the <b>docs</b> folder to test the endpoints; also a swagger page will be loaded when the api runs
