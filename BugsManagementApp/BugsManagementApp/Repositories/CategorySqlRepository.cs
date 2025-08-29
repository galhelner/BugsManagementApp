using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IO;
using BugsManagementApp.Models;


namespace BugsManagementApp.Repositories
{
    public class CategorySqlRepository : IRepository<Category>
    {
        private const string _sqlConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Database;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
        private static CategorySqlRepository? _instance;

        private CategorySqlRepository()
        {

        }

        public static CategorySqlRepository GetInstance()
        {
            if (_instance == null)
            {
                _instance = new CategorySqlRepository();
            }

            return _instance;
        }
        public async Task Add(Category item)
        {
            // insert item to DB
            string query = "INSERT INTO Categories (CategoryName, ParentCategoryId) VALUES (@CategoryName, @ParentCategoryId);";

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to avoid SQL injection
                    command.Parameters.AddWithValue("@CategoryName", item.Name);
                    command.Parameters.AddWithValue("@ParentCategoryId", item.ParentId);

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
            string query = "DELETE FROM Categories WHERE Id = @CategoryId";

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to avoid SQL injection
                    command.Parameters.AddWithValue("@CategoryId", itemID);

                    // Open the connection
                    await connection.OpenAsync();

                    // Execute the command
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<Category?> Get(int itemID)
        {
            string query = "SELECT * FROM Categories WHERE Id = @CategoryID";
            Category? category = null;

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to avoid SQL injection
                    command.Parameters.AddWithValue("@CategoryID", itemID);

                    // Open the connection
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            category = new Category
                            {
                                Id = await reader.GetFieldValueAsync<int>("Id"),
                                Name = await reader.GetFieldValueAsync<string>("CategoryName"),
                                ParentId = await reader.GetFieldValueAsync<int>("ParentCategoryId")
                            };
                        }

                        if (category != null)
                        {
                            category.ParentName = await GetParentName(category.ParentId);
                        }

                        return category;
                    }
                }
            }
        }

        public async Task<Category?> GetByName(string name)
        {
            string query = "SELECT * FROM Categories WHERE CategoryName = @CategoryName";
            Category? category = null;

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to avoid SQL injection
                    command.Parameters.AddWithValue("@CategoryName", name);

                    // Open the connection
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            category = new Category
                            {
                                Id = await reader.GetFieldValueAsync<int>("Id"),
                                Name = await reader.GetFieldValueAsync<string>("CategoryName"),
                                ParentId = await reader.GetFieldValueAsync<int>("ParentCategoryId")
                            };
                        }

                        if (category != null)
                        {
                            category.ParentName = await GetParentName(category.ParentId);
                        }

                        return category;
                    }
                }
            }
        }

        private async Task<string> GetParentName(int parentID)
        {
            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                // If a parent category exists, fetch its name
                if (parentID != -1)
                {
                    string parentQuery = "SELECT CategoryName FROM Categories WHERE Id = @ParentId";
                    using (SqlCommand parentCommand = new SqlCommand(parentQuery, connection))
                    {
                        parentCommand.Parameters.AddWithValue("@ParentId", parentID);
                        // Open the connection
                        await connection.OpenAsync();
                        using (SqlDataReader parentReader = await parentCommand.ExecuteReaderAsync())
                        {
                            if (await parentReader.ReadAsync())
                            {
                                return await parentReader.GetFieldValueAsync<string>(0);
                            }
                        }
                    }
                }
                return "No Parent";
            }
        }

        public async Task<List<Category>> GetAll()
        {
            string query = "SELECT * FROM Categories";
            List<Category> categories = new List<Category>();

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
                            Category category = new Category
                            {
                                Id = await reader.GetFieldValueAsync<int>("Id"),
                                Name = await reader.GetFieldValueAsync<string>("CategoryName"),
                                ParentId = await reader.GetFieldValueAsync<int>("ParentCategoryId"),
                                ParentName = await GetParentName(await reader.GetFieldValueAsync<int>("ParentCategoryId"))
                            };

                            if (category.ParentName == "No Parent")
                            {
                                category.ParentId = -1;
                                await Update(category.Id, category);
                            }

                            categories.Add(category);

                        }

                        return categories;
                    }
                }
            }
        }

        public async Task<List<Category>> GetAllTopLevel()
        {
            string query = "SELECT * FROM Categories WHERE ParentCategoryId = -1";
            List<Category> categories = new List<Category>();

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Open the connection
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                Id = await reader.GetFieldValueAsync<int>("Id"),
                                Name = await reader.GetFieldValueAsync<string>("CategoryName"),
                                ParentId = await reader.GetFieldValueAsync<int>("ParentCategoryId"),
                                ParentName = await GetParentName(await reader.GetFieldValueAsync<int>("ParentCategoryId"))
                            });
                        }

                        return categories;
                    }
                }
            }
        }

        public async Task<List<Category>> GetAllChilds(int parentId)
        {
            string query = "SELECT * FROM Categories WHERE ParentCategoryId = @ParentCategoryId";
            List<Category> categories = new List<Category>();

            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ParentCategoryId", parentId);
                    // Open the connection
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                Id = await reader.GetFieldValueAsync<int>("Id"),
                                Name = await reader.GetFieldValueAsync<string>("CategoryName"),
                                ParentId = await reader.GetFieldValueAsync<int>("ParentCategoryId"),
                                ParentName = await GetParentName(await reader.GetFieldValueAsync<int>("ParentCategoryId"))
                            });
                        }

                        return categories;
                    }
                }
            }
        }

        public async Task Update(int itemID, Category item)
        {
            SqlConnection? connection = null;
            try
            {
                string query = "UPDATE Categories SET CategoryName = @CategoryName, ParentCategoryId = @ParentCategoryId WHERE Id = @categoryID";

                using (connection = new SqlConnection(_sqlConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to avoid SQL injection
                        command.Parameters.AddWithValue("@categoryID", itemID);
                        command.Parameters.AddWithValue("@CategoryName", item.Name);
                        command.Parameters.AddWithValue("@ParentCategoryId", item.ParentId);

                        // Open the connection
                        await connection.OpenAsync();

                        // Execute the command
                        await command.ExecuteNonQueryAsync();
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

        public async Task<List<Category>> GetCompositeTree()
        {
            BugSqlRepository bugSqlRepository = BugSqlRepository.GetInstance();
            List<Category> categories = await GetAllTopLevel();
            foreach (Category category in categories)
            {
                category.Bugs = await bugSqlRepository.GetBugsByCategoryId(category.Id);
                await AddChilds(category);
            }
            return categories;
        }

        private async Task AddChilds(Category category)
        {
            BugSqlRepository bugSqlRepository = BugSqlRepository.GetInstance();
            List<Category> children = await GetAllChilds(category.Id);
            category.ChildCategories = children;
            foreach (Category child in children)
            {
                child.Bugs = await bugSqlRepository.GetBugsByCategoryId(child.Id);
                await AddChilds(child);
            }
        }

        public async Task<string> GetCompositeTreeAsString()
        {
            List<Category> composite = await GetCompositeTree();
            StringBuilder compositeStringBuilder = new StringBuilder();
            foreach (var category in composite)
            {
                AppendCategoryToStringBuilder(category, compositeStringBuilder, 0);
            }
            return compositeStringBuilder.ToString();
        }

        private void AppendCategoryToStringBuilder(Category category, StringBuilder builder, int depth)
        {
            // Start with the category name and indentation
            string categoryLine = new string(' ', depth * 2) + "- " + category.Name;

            // Retrieve and append bugs related to the current category
            var bugs = category.Bugs;
            if (bugs.Any())
            {
                // Append bugs in parentheses
                categoryLine += " (" + string.Join(", ", bugs.Select(b => b.Title)) + ")";
            }

            // Add the category line to the builder
            builder.AppendLine(categoryLine);

            // Recursively add child categories
            if (category.ChildCategories != null)
            {
                foreach (var child in category.ChildCategories)
                {
                    AppendCategoryToStringBuilder(child, builder, depth + 1);
                }
            }
        }

        public async void WriteCompositeTreeToFile()
        {
            string filePath = "composite.txt";
            string fullPath = Path.GetFullPath(filePath);
            string content = await GetCompositeTreeAsString();

            try
            {
                File.WriteAllText(filePath, content);
                MessageBox.Show("Composite pattern was written to " + fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
    }

}
