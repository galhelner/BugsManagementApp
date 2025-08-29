using BugsManagementApp.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace BugsManagementApp.Repositories
{
    public class BugSqlRepository : IRepository<Bug>
    {
        private const string _sqlConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Database;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        private static BugSqlRepository? _instance;

        private BugSqlRepository()
        {

        }

        public static BugSqlRepository GetInstance()
        {
            if (_instance == null)
            {
                _instance = new BugSqlRepository();
            }
            return _instance;
        }


        public async Task Add(Bug item)
        {
            // insert item to DB
            string query = "INSERT INTO bugs (title, description, status, categoryID) VALUES (@title, @description, @status, @categoryID);";

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to avoid SQL injection
                    command.Parameters.AddWithValue("@title", item.Title);
                    command.Parameters.AddWithValue("@description", item.Description);
                    command.Parameters.AddWithValue("@status", item.Status);
                    command.Parameters.AddWithValue("@categoryID", item.CategoryId);

                    // Open the connection
                    await connection.OpenAsync();

                    // Execute the command
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task Delete(int itemID)
        {
            // delete item from DB
            string query = "DELETE FROM bugs WHERE Id = @bugID";

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to avoid SQL injection
                    command.Parameters.AddWithValue("@bugID", itemID);

                    // Open the connection
                    await connection.OpenAsync();

                    // Execute the command
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<Bug?> Get(int itemID)
        {
            CategorySqlRepository categorySqlRepository = CategorySqlRepository.GetInstance();
            string query = "SELECT * FROM bugs WHERE Id = @bugID";
            Bug? bug = null;

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to avoid SQL injection
                    command.Parameters.AddWithValue("@bugID", itemID);

                    // Open the connection
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int categoryID = await reader.GetFieldValueAsync<int>("categoryID");
                            Category? category = await categorySqlRepository.Get(categoryID);
                            bug = new Bug()
                            {
                                BugID = await reader.GetFieldValueAsync<int>("Id"),
                                Description = await reader.GetFieldValueAsync<string>("description"),
                                Status = await reader.GetFieldValueAsync<string>("status"),
                                CategoryId = await reader.GetFieldValueAsync<int>("categoryID"),
                                CategoryName = category?.Name
                            };
                        }
                        return bug;
                    }
                }
            }
        }

        public async Task<Bug?> GetByTitle(string title)
        {
            CategorySqlRepository categorySqlRepository = CategorySqlRepository.GetInstance();
            string query = "SELECT * FROM bugs WHERE title = @title";
            Bug? bug = null;

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to avoid SQL injection
                    command.Parameters.AddWithValue("@title", title);

                    // Open the connection
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            int categoryID = await reader.GetFieldValueAsync<int>("categoryID");
                            Category? category = await categorySqlRepository.Get(categoryID);
                            bug = new Bug()
                            {
                                BugID = await reader.GetFieldValueAsync<int>("Id"),
                                Description = await reader.GetFieldValueAsync<string>("description"),
                                Status = await reader.GetFieldValueAsync<string>("status"),
                                CategoryId = await reader.GetFieldValueAsync<int>("categoryID"),
                                CategoryName = category?.Name
                            };
                        }
                        return bug;
                    }
                }
            }
        }

        public async Task<List<Bug>> GetAll()
        {
            CategorySqlRepository categorySqlRepository = CategorySqlRepository.GetInstance();
            string query = "SELECT * FROM bugs";
            List<Bug> bugs = new List<Bug>();

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Open the connection
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            int categoryID = await reader.GetFieldValueAsync<int>("categoryID");
                            Category? category = await categorySqlRepository.Get(categoryID);
                            bugs.Add(new Bug()
                            {

                                BugID = await reader.GetFieldValueAsync<int>("Id"),
                                Title = await reader.GetFieldValueAsync<string>("title"),
                                Description = await reader.GetFieldValueAsync<string>("description"),
                                Status = await reader.GetFieldValueAsync<string>("status"),
                                CategoryId = await reader.GetFieldValueAsync<int>("categoryID"),
                                CategoryName = category?.Name
                            });
                        }

                        return bugs;
                    }
                }
            }
        }

        public async Task Update(int itemID, Bug item)
        {
            SqlConnection? connection = null;
            try
            {
                string query = "UPDATE bugs SET title = @title, description = @description, status = @status, categoryID = @categoryID WHERE Id = @bugID";

                using (connection = new SqlConnection(_sqlConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to avoid SQL injection
                        command.Parameters.AddWithValue("@bugID", itemID);
                        command.Parameters.AddWithValue("@title", item.Title);
                        command.Parameters.AddWithValue("@description", item.Description);
                        command.Parameters.AddWithValue("@status", item.Status);
                        command.Parameters.AddWithValue("@categoryID", item.CategoryId);

                        // Open the connection
                        await connection.OpenAsync();

                        // Execute the command
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(new Exception("Failed to update", ex));
            }
            finally
            {
                connection?.Close();
            }
        }

        public async Task<List<Bug>> GetBugsByCategoryId(int categoryID)
        {
            CategorySqlRepository categorySqlRepository = CategorySqlRepository.GetInstance();
            string query = "SELECT * FROM bugs WHERE categoryID = @categoryID";
            List<Bug> bugs = new List<Bug>();

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@categoryID", categoryID);

                    // Open the connection
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Category? category = await categorySqlRepository.Get(categoryID);
                            bugs.Add(new Bug()
                            {

                                BugID = await reader.GetFieldValueAsync<int>("Id"),
                                Title = await reader.GetFieldValueAsync<string>("title"),
                                Description = await reader.GetFieldValueAsync<string>("description"),
                                Status = await reader.GetFieldValueAsync<string>("status"),
                                CategoryId = await reader.GetFieldValueAsync<int>("categoryID"),
                                CategoryName = category?.Name
                            });
                        }

                        return bugs;
                    }
                }
            }
        }
    }

}
