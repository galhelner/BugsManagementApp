using BugsManagementApp.Models;
using BugsManagementApp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BugsManagementApp
{
    /// <summary>
    /// Interaction logic for CategoriesWindow.xaml
    /// </summary>
    public partial class CategoriesWindow : Window
    {
        private CategorySqlRepository _repository;
        private Category? _selectedCategory;
        public CategoriesWindow()
        {
            InitializeComponent();
            _repository = CategorySqlRepository.GetInstance();
            Loaded += async (s, e) => await LoadCategories();
        }

        private async Task LoadCategories()
        {
            try
            {
                var categories = await _repository.GetAll();
                CategoryDataGrid.ItemsSource = categories;
                TreeViewCategories.ItemsSource = null;
                TreeViewCategories.ItemsSource = await _repository.GetCompositeTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            string name = CategoryNameTextBox.Text;
            int parentCategoryID;

            if (name == String.Empty)
            {
                MessageBox.Show("Please enter category name!");
                return;
            }

            Category? sameNameCategory = await _repository.GetByName(name);
            if (sameNameCategory != null)
            {
                MessageBox.Show("Category named: " + name + " is already exist!");
                return;
            }

            if (_selectedCategory != null)
            {
                parentCategoryID = _selectedCategory.Id;
            }
            else
            {
                MessageBox.Show("Please choose category!");
                return;
            }

            Category category = new Category()
            {
                Name = name,
                ParentId = parentCategoryID
            };

            await _repository.Add(category);

            // re-render the data grid
            await LoadCategories();

            // clear input fields
            CategoryNameTextBox.Text = String.Empty;
            SelectCategoryButton.Content = "Select Category";
        }

        private async void BtnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CategoryDataGrid.SelectedItem is Category categoryItem)
                {
                    BugSqlRepository bugSqlRepository = BugSqlRepository.GetInstance();
                    List<Bug> bugs = await bugSqlRepository.GetBugsByCategoryId(categoryItem.Id);
                    if (bugs.Count > 0)
                    {
                        MessageBox.Show("Category cannot be deleted, there is bugs with this category!");
                        return;
                    }

                    await _repository.Delete(categoryItem.Id);

                    // re-render the data grid
                    await LoadCategories();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void CategoryDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                int columnIndex = e.Column.DisplayIndex;
                var editedItem = e.Row.DataContext;
                var editedCellValue = (e.EditingElement as TextBox)?.Text;
                var categoryItem = editedItem as Category;

                if (categoryItem != null)
                {
                    switch (columnIndex)
                    {
                        case 0:
                            MessageBox.Show("Category ID cannot be changed!");
                            // re-render the data grid
                            await LoadCategories();
                            return;
                        case 1:
                            if (editedCellValue != null && editedCellValue != string.Empty)
                            {
                                Category? sameNameCategory = await _repository.GetByName(editedCellValue);
                                if (sameNameCategory != null)
                                {
                                    MessageBox.Show("Category named: " + editedCellValue + " is already exist!");
                                    // re-render the data grid
                                    await LoadCategories();
                                    return;
                                }
                                categoryItem.Name = editedCellValue;
                            }
                            else
                            {
                                MessageBox.Show("Please enter category name!");
                                // re-render the data grid
                                await LoadCategories();
                                return;
                            }
                            break;
                        case 2:
                            string? parentName = editedCellValue;
                            categoryItem.ParentId = -1;
                            if (parentName != null && parentName != string.Empty)
                            {
                                Category? parent = await _repository.GetByName(parentName);
                                if (parent != null)
                                {
                                    categoryItem.ParentId = parent.Id;
                                }
                                else
                                {
                                    MessageBox.Show("There is no category named: " + parentName);
                                    // re-render the data grid
                                    await LoadCategories();
                                    return;
                                }
                            }
                            break;
                    }

                    // update category in DB
                    await _repository.Update(categoryItem.Id, categoryItem);

                    // re-render the data grid
                    await LoadCategories();
                }
            }
        }

        private async void TogglePopup_Click(object sender, RoutedEventArgs e)
        {
            // loading categories from DB
            await LoadCategories();

            if (TreeViewCategories.ItemsSource == null || !TreeViewCategories.ItemsSource.Cast<object>().Any())
            {
                MessageBox.Show("There is no categories!\nYou have to add category before adding a bug!");
                SelectCategoryButton.Content = "Select Category";
                return;
            }

            TreeViewPopup.IsOpen = !TreeViewPopup.IsOpen;
        }

        private void TreeViewCategories_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Store the selected category
            _selectedCategory = e.NewValue as Category;

            // Update the button content with the selected category's name
            if (_selectedCategory != null)
            {
                SelectCategoryButton.Content = _selectedCategory.Name;
                TreeViewPopup.IsOpen = false; // Close the popup after selection
            }
        }

    }
}
