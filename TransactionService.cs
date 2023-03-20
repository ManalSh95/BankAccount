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
using System.Collections.Generic;

namespace BankAccount
{
    public static class TransactionService
    {
        [FunctionName("CreateDepositTransaction")]
        public static async Task<IActionResult> HandleCreateDepositTransaction(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateDepositTransaction/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string connectionString = Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process);

            // Read the request body as a string.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // Deserialize the request body into a dynamic object.
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Extract the account details from the request body.
            string transactionName = data?.transactionName ?? string.Empty;
            string transactionNote = data?.transactionNote ?? string.Empty;
            decimal transactionAmount = data?.transactionAmount;


            Transaction transaction = new Transaction()
            {
                Name = transactionName,
                Note = transactionNote,
                Amount = transactionAmount,
                TransactionDate = DateTime.UtcNow,
                AccountId = id
            };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(
                    "INSERT INTO [Transaction] (TransactionId, Name, Note, AccountId, Amount, Date, TransactionType)" +
                    " VALUES (@TransactionId, @Name, @Note, @AccountId, @Amount, @Date, @TransactionType)",
                                connection);

                command.Parameters.AddWithValue("@TransactionId", transaction.Id);
                command.Parameters.AddWithValue("@Name", transaction.Name);
                command.Parameters.AddWithValue("@Note", transaction.Note);
                command.Parameters.AddWithValue("@AccountId", transaction.AccountId);
                command.Parameters.AddWithValue("@Amount", transaction.Amount);
                command.Parameters.AddWithValue("@Date", transaction.TransactionDate);
                command.Parameters.AddWithValue("@TransactionType", 1);

                try
                {
                    // Open the SQL connection.
                    await connection.OpenAsync();

                    // Execute the SQL command to insert the new account into the database.
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    // Check that the account was inserted successfully.
                    if (rowsAffected > 0)
                    {
                        
                        return new OkObjectResult(new { message = "Created transaction successfully" });
                    }
                    else
                    {
                        return new BadRequestObjectResult(new { message = "Unable to create new transaction." });
                    }
                }
                catch (Exception e)
                {
                    // Log any errors that occur during the SQL operation.
                    log.LogError(e, "Error creating new transaction for account {accountId}.", id);

                    // Return an error response if an exception is thrown.
                    return new StatusCodeResult(500);
                }
            }
        }

        [FunctionName("GetAccountTransactions")]
        public static async Task<IActionResult> HandleGetAccountTransactions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAccountTransactions/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            string connectionString = Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process);

            var accountId = Guid.Parse(id);

            List<Transaction> transactions = new List<Transaction>();
            TransactionType transactionType = new TransactionType();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                   string query = "SELECT t.TransactionId, t.Amount, t.Date, t.TransactionType " +
                        "FROM [Transaction] t " +
                        "WHERE t.AccountId = @AccountId;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AccountId", accountId);

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
                            }
                        }
                    }

                return new OkObjectResult(transactions);
            }
        }
    }
}
