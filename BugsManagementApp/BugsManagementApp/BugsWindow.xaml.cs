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
    /// Interaction logic for BugsWindow.xaml
    /// </summary>
    public partial class BugsWindow : Window
    {
        private BugSqlRepository? _repository;
        private CategorySqlRepository _categoryRepository;
        private Category? _selectedCategory;
        public BugsWindow()
        {
            InitializeComponent();
            _repository = BugSqlRepository.GetInstance();
            _categoryRepository = CategorySqlRepository.GetInstance();
            LoadBugs();
        }

        public async Task LoadCategories()
        {
            TreeViewCategories.ItemsSource = null;
            TreeViewCategories.ItemsSource = await _categoryRepository.GetCompositeTree();
        }

        private async void LoadBugs()
        {
            try
            {
                if (_repository == null)
                {
                    return;
                }
                var bugs = await _repository.GetAll();
                BugDataGrid.ItemsSource = bugs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void BtnAddBug_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string description = DescriptionTextBox.Text;
            int statusIndex = StatusComboBox.SelectedIndex;
            string? status;
            int categoryID;
            if (statusIndex != -1)
            {
                status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            }
            else
            {
                MessageBox.Show("Please choose status!");
                return;
            }

            if (title == String.Empty)
            {
                MessageBox.Show("Please enter bug title!");
                return;
            }


            if (_selectedCategory != null)
            {
                categoryID = _selectedCategory.Id;
            }
            else
            {
                MessageBox.Show("Please choose category!");
                return;
            }

            if (_repository == null)
            {
                return;
            }

            Bug? sameTitleBug = await _repository.GetByTitle(title);
            if (sameTitleBug != null)
            {
                MessageBox.Show("Bug with title named: " + title + " is already exists!");
                return;
            }

            Bug bug = new Bug
            {
                Title = title,
                Description = description,
                Status = status,
                CategoryId = categoryID
            };

            await _repository.Add(bug);

            // clear input fields
            TitleTextBox.Text = "";
            DescriptionTextBox.Text = "";
            StatusComboBox.SelectedIndex = -1;
            SelectCategoryButton.Content = "Select Category";
            TreeViewPopup.IsOpen = false; // Close the popup after selection

            // re-render the UI
            LoadBugs();
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

        private async void BtnDeleteBug_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BugDataGrid.SelectedItem is Bug bugItem)
                {
                    if (_repository == null)
                    {
                        return;
                    }

                    await _repository.Delete(bugItem.BugID);

                    // re-render the data grid
                    LoadBugs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void BugDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                int columnIndex = e.Column.DisplayIndex;
                var editedItem = e.Row.DataContext;
                var editedCellValue = (e.EditingElement as TextBox)?.Text;
                var bugItem = editedItem as Bug;

                if (_repository == null)
                {
                    return;
                }

                if (bugItem != null)
                {
                    switch (columnIndex)
                    {
                        case 0:
                            MessageBox.Show("Bug ID cannot be changed!");
                            // re-render the data grid
                            LoadBugs();
                            return;
                        case 1:
                            if (editedCellValue != null && editedCellValue != string.Empty)
                            {
                                Bug? sameTitleBug = await _repository.GetByTitle(editedCellValue);
                                if (sameTitleBug != null)
                                {
                                    MessageBox.Show("Bug named: " + editedCellValue + " is already exist!");
                                    // re-render the data grid
                                    LoadBugs();
                                    return;
                                }
                                bugItem.Title = editedCellValue;
                            }
                            else
                            {
                                MessageBox.Show("Please enter Bug Title!");
                                // re-render the data grid
                                await LoadCategories();
                                return;
                            }
                            break;
                        case 2:
                            if (editedCellValue != null)
                            {
                                string description = editedCellValue;
                                bugItem.Description = description;
                            }
                            break;
                        case 3:
                            if (editedCellValue != null)
                            {
                                string status = editedCellValue;
                                string[] possibleStatus = { "Open", "In Progress", "Closed" };
                                if (Array.Exists(possibleStatus, s => s == status))
                                {
                                    bugItem.Status = status;
                                }
                                else
                                {
                                    MessageBox.Show("Status: " + status + " is invalid!");
                                    // re-render the data grid
                                    LoadBugs();
                                    return;
                                }
                            }
                            break;
                        case 4:
                            if (editedCellValue != null)
                            {
                                string categoryName = editedCellValue;
                                Category? validCategory = await _categoryRepository.GetByName(categoryName);
                                if (validCategory != null)
                                {
                                    bugItem.CategoryId = validCategory.Id;
                                }
                                else
                                {
                                    MessageBox.Show("Category named: " + categoryName + " is invalid!");
                                    // re-render the data grid
                                    LoadBugs();
                                    return;
                                }
                            }

                            break;
                    }

                    // update bug in DB
                    await _repository.Update(bugItem.BugID, bugItem);

                    // re-render the data grid
                    LoadBugs();
                }
            }
        }

    }
}
