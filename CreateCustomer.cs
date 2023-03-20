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
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace BankAccount
{
    public static class CreateCustomer
    {
        [FunctionName("CreateCustomer")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process);
            
            log.LogInformation("C# HTTP trigger function processed a request.");


            Customer customer1 = new Customer()
            {
                Name = "Mike",
                SurName = "Specter",
                Email = "mike.specter123@outlook.com",
                Address = "London",
                CreatedAt = new DateTime(2021,5,17)
            };
            Customer customer2 = new Customer()
            {
                Name = "Derek",
                SurName = "Hale",
                Email = "derek.hale123@outlook.com",
                Address = "Beacon Hills",
                CreatedAt = new DateTime(2017,7,31)
            };
            Customer customer3 = new Customer()
            {
                Name = "Joey",
                SurName = "Tribiani",
                Email = "joey.tribiani123@outlook.com",
                Address = "New York City",
                PhoneNumber = "1234567890",
                CreatedAt = DateTime.Now
            };
            List<Customer> customers = new List<Customer> { customer1, customer2, customer3 };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Use a transaction to ensure that all inserts succeed or none of them do
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        
                        foreach (Customer customer in customers)
                        {
                            // Construct the SQL command to insert the customer record
                            SqlCommand command = new SqlCommand(
                                "INSERT INTO Customer (CustomerId, Name, SurName, Email, Address, PhoneNb, CreatedAt)" +
                                " VALUES (@CustomerId, @Name, @SurName, @Email, @Address, @PhoneNb, @CreatedAt)",
                                connection, transaction);

                            command.Parameters.AddWithValue("@CustomerId", customer.Id);
                            command.Parameters.AddWithValue("@Name", customer.Name);
                            command.Parameters.AddWithValue("@SurName", customer.SurName);
                            command.Parameters.AddWithValue("@Email", customer.Email);
                            command.Parameters.AddWithValue("@Address", customer.Address);
                            command.Parameters.AddWithValue("@PhoneNb", customer.PhoneNumber);
                            command.Parameters.AddWithValue("@CreatedAt", customer.CreatedAt);

                            // Execute the SQL command
                            await command.ExecuteNonQueryAsync();
                        }

                        // If all inserts succeeded, commit the transaction
                        transaction.Commit();

                        return new OkObjectResult($"Inserted {customers.Count} customers.");
                    }
                    catch (Exception ex)
                    {
                        // If any insert failed, roll back the transaction
                        transaction.Rollback();
                        log.LogError($"Error in inserting customers: {ex.Message}");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }
                }
            }
        }
    }
}
