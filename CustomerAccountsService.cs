using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using BankAccount.DataModels;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;

namespace BankAccount
{
    public static class CustomerAccountsService
    {
        [FunctionName("GetCustomerAccounts")]
        public static async Task<IActionResult> HandleGetCustomerAccounts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetCustomerAccounts/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            string connectionString = Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process);

            var customerId = Guid.Parse(id);
            
         //   TransactionType transactionType = new TransactionType();

            List<Account> accounts = new List<Account>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT a.AccountId, a.Name, a.Balance " +
                                "FROM Account a " +
                                "WHERE a.CustomerId = @CustomerId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Account account = new Account()
                            {
                                Id = reader["AccountId"].ToString(),
                                Name = reader.GetString(1),
                                Balance = reader.GetDecimal(2)
                            };
                            accounts.Add(account);
                        }
                    }
                }
/*
                foreach (Account account in accounts)
                {
                    decimal totalBalance = 0;
                    List<Transaction> transactions = new List<Transaction>();

                    query = "SELECT t.TransactionId, t.Amount, t.Date, t.TransactionType " +
                        "FROM [Transaction] t " +
                        "WHERE t.AccountId = @AccountId;";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AccountId", account.Id);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {

                            while (await reader.ReadAsync())
                            {
                                string transactionId = reader["TransactionId"].ToString();
                                decimal amount = reader.GetDecimal(1);
                                DateTime dateTime = reader.GetDateTime(2);
                                int type = (int)reader["TransactionType"];

                                if (type == 1)
                                {
                                    transactionType = TransactionType.Deposit;
                                }
                                else if (type == 2)
                                {
                                    transactionType = TransactionType.Withdrawal;
                                }
                                // TransactionType transactionType = Enum.Parse<TransactionType>(reader.GetString(2));
                                Transaction transaction = new Transaction()
                                {
                                    Id = transactionId,
                                    Amount = amount,
                                    TransactionDate = dateTime,
                                    TransactionType = transactionType
                                };
                                transactions.Add(transaction);
                                totalBalance += amount;
                            }
                        }
                    }

                    account.Transactions = transactions;
                    account.Balance = totalBalance;
                }*/

                return new OkObjectResult(accounts);
            }
        }

        [FunctionName("CreateCustomerAccount")]
        public static async Task<IActionResult> HandleCreateCustomerAccount(
               [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "CreateCustomerAccount/{id}")] HttpRequest req,
               ILogger log, string id)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string connectionString = Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process);

            // Read the request body as a string.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize the request body into a dynamic object.
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Extract the account details from the request body.
            string accountName = data?.accountName ?? string.Empty;

            Account account = new Account()
            {
                CustomerId = id,
                Name = accountName,
                CreatedAt = DateTime.UtcNow
            };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(
                    "INSERT INTO Account (AccountId, Name, CustomerId, CreatedAt, Balance)" +
                    " VALUES (@AccountId, @Name, @CustomerId, @CreatedAt, @Balance)",
                                connection);

                command.Parameters.AddWithValue("@AccountId", account.Id);
                command.Parameters.AddWithValue("@Name", account.Name);
                command.Parameters.AddWithValue("@CustomerId", account.CustomerId);
                command.Parameters.AddWithValue("@CreatedAt", account.CreatedAt);
                command.Parameters.AddWithValue("@Balance", account.Balance);

                try
                {
                    // Open the SQL connection.
                    await connection.OpenAsync();

                    // Execute the SQL command to insert the new account into the database.
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    // Check that the account was inserted successfully.
                    if (rowsAffected > 0)
                    {
                        return new OkObjectResult(account);
                    }
                    else
                    {
                        return new BadRequestObjectResult(new { message = "Unable to create new account." });
                    }
                }
                catch (Exception e)
                {
                    // Log any errors that occur during the SQL operation.
                    log.LogError(e, "Error creating new account for customer {customerId}.", id);

                    // Return an error response if an exception is thrown.
                    return new StatusCodeResult(500);
                }
            }
        }

    }
}
