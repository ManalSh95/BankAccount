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
using System.Data;

namespace BankAccount
{
    public static class CustomerService
    {
        [FunctionName("GetAllCustomers")]
        public static async Task<IActionResult> HandleGetAllCustomers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process);

            List<Customer> customers = new List<Customer>();

            log.LogInformation("C# HTTP trigger function processed a request.");
            try { 
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                string query = "SELECT * FROM Customer";
                using (SqlCommand command = new SqlCommand(query, connection))
                {

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    { 
                        while (await reader.ReadAsync())
                        {
                            Customer customer = new Customer()
                            {
                                Id = reader["CustomerId"].ToString(),
                                Name = (string)reader["Name"],
                                SurName = (string)reader["SurName"],
                                Email = (string)reader["Email"],
                                PhoneNumber = (string)reader["PhoneNb"],
                                Address = (string)reader["Address"],
                                CreatedAt = (DateTime)reader["CreatedAt"]
                            };
                            customers.Add( customer );
                        }
                    }

                }
            }
                return new OkObjectResult(customers);
            }
            catch (Exception ex)
            {
                log.LogError($"Error in getting customers: {ex.Message}");
                return new StatusCodeResult(500);
            }
            
        }

        [FunctionName("GetCustomer")]
        public static async Task<IActionResult> HandleGetCustomer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetCustomer/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var customerId = Guid.Parse(id);
            // log.LogInformation($"{customerId}");
            string connectionString = Environment.GetEnvironmentVariable("MyConnectionString", EnvironmentVariableTarget.Process);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT * FROM Customer WHERE CustomerId = @customerId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@customerId", customerId);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();

                            Customer customer = new Customer()
                            {
                                Id = reader["CustomerId"].ToString(),
                                Name = (string)reader["Name"],
                                SurName = (string)reader["SurName"],
                                Email = (string)reader["Email"],
                                Address = (string)reader["Address"],
                                PhoneNumber = (string)reader["PhoneNb"],
                                CreatedAt = (DateTime)reader["CreatedAt"]
                            };

                            return new OkObjectResult(customer);
                        }
                        else
                        {
                            return new NotFoundResult();
                        }
                    }
                }
            }
        }
    }
}
