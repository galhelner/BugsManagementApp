using BugsManagementApp.Repositories;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BugsManagementApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnOpenBugsWindow_Click(object sender, RoutedEventArgs e)
        {
            BugsWindow bugsWindow = new BugsWindow();
            bugsWindow.Show();
        }

        private void BtnOpenCategoriesWindow_Click(object sender, RoutedEventArgs e)
        {
            CategoriesWindow categoriesWindow = new CategoriesWindow();
            categoriesWindow.Show();
        }

        private void BtnWriteCompositeToFile_Click(object sender, RoutedEventArgs e)
        {
            CategorySqlRepository categorySqlRepository = CategorySqlRepository.GetInstance();
            categorySqlRepository.WriteCompositeTreeToFile();
        }
    }

}