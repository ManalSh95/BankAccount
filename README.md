# Bank Account Azure Functions App

# Prerequisites
Visual Studio 2022

.NET Core

Azure Functions Core Tools

# Running the project
Clone the repository to your local machine


Run the SQL Server Query script file in your SQL Server Management Studio to create the databases


Open the solution file (BankAccount.sln) in Visual Studio


Create a 'local.settings.json' file


Open the 'local.settings.json file' and update the values for the keys that require local configuration, such as the AzureWebJobsStorage, the database connection string, the httpPort, and the Host CORS for the URL of the app that will use your Azure Functions services


Build the solution by pressing Ctrl+Shift+B or by selecting Build > Build Solution from the menu


Set the startup project to the Azure Functions project by right-clicking the project in Solution Explorer and selecting Set as Startup Project


Press F5 or select Debug > Start Debugging from the menu to run the project locally
