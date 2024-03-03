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
using VantageLibrary;
using VantageLibrary.Types;
using static VantageLibrary.Types.VantageCategoryCreation;

namespace VantageWPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<VantageCategory> _categories;
        DomainClient domainClient;
        public MainWindow()
        {
            InitializeComponent();
            
        }

        public void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("You clicked the settings button");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateServerResponseTextBlock(null);
        }

        private void ConnectServerButton_Click(object sender, RoutedEventArgs e)
        {
            domainClient = new DomainClient(new Uri(ServerAddressInputTextBox.Text));

            //bool connected = domainClient.Functions.DomainOnline();

            bool connected = Task.Run(() => domainClient.Functions.DomainOnline()).Result;


            if (connected)
            {
                ConnectedStatusValue.Content = "Connected";
                ConnectedStatusValue.Background = System.Windows.Media.Brushes.Chartreuse;
                UpdateServerResponseTextBlock("Connected To Server!");
            }
            else {
                UpdateServerResponseTextBlock("Failed Connection to Server");
                MessageBox.Show("Server Failed Connection.\nCheck server address.","Server Error",MessageBoxButton.OKCancel);
                domainClient.Dispose();
            }
        }

        public void UpdateServerResponseTextBlock(string message, bool isJson = false)
        {
            ServerResponseTextBlock.Text = string.Empty;
            if (message != null)
            {
                if (isJson)
                {
                    // Pretty print the json to screen
                }
                ServerResponseTextBlock.Text = message;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (domainClient != null)
            {
                domainClient.Dispose();
            }
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (domainClient != null)
            {
                domainClient.Dispose();
            }
        }

        private void GetWorkflowsButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the workflows and populate the 'CategorySelectionBox' entry

            //List<VantageWorkflow> workflows = domainClient.Functions.GetWorkflows();

            if(_categories != null)
            {
                _categories.Clear();
            }
            _categories = domainClient.Functions.GetCategories();

            string textToOutput = string.Empty;


            foreach (var category in _categories)
            {
                VantageComboBoxItem item = new VantageComboBoxItem();
                item.DisplayText = category.Name;
                item.Value = category.Identifier;
                CategorySelectionBox.Items.Add(new ComboBoxItem{ Content = item.DisplayText, Tag = item.Value });
                textToOutput += "Category Name: " + category.Name + "\n";
                foreach(var workflow in category.Workflows)
                {
                    textToOutput += "Workflow Name: " + workflow.Name + "\n";
                }
            }
            UpdateServerResponseTextBlock(textToOutput, false);
        }

        private void CategorySelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Objective: Use the category selection to populate the workflow list. 'WorkflowSelectionBox'
            // First clear the 'WorkflowSelectionBox' of items.
            WorkflowSelectionBox.Items.Clear();
            
            // What is the selected item in 'CategorySelectionBox'?
            Guid selectedItemId = (Guid)((ComboBoxItem)CategorySelectionBox.SelectedItem).Tag;
            
            // Next, get the category that matches the Value of the comboboxitem
            VantageCategory selectedCategory = _categories.Where(x => x.Identifier == selectedItemId).FirstOrDefault();
            if (selectedCategory != null)
            {
                foreach (var workflowItem in selectedCategory.Workflows)
                {
                    // is the workflow active? Need to query for the workflow from Vantage.
                    VantageLibrary.Types.ResponseTypes.VantageWorkflow thisWorkflow = domainClient.Functions.GetWorkflow(workflowItem.Identifier);
                    bool active = thisWorkflow.State == VantageLibrary.Enums.WorkflowStateEnum.Active;
                    if (active)
                    {
                        VantageComboBoxItem item = new VantageComboBoxItem();
                        item.DisplayText = thisWorkflow.Name;
                        item.Value = thisWorkflow.Identifier;
                        WorkflowSelectionBox.Items.Add(new ComboBoxItem { Content = item.DisplayText, Tag = item.Value });
                    }
                }
            }
            else
            {
                MessageBox.Show("Big problemo!");
            }
        }

        public class VantageComboBoxItem : ComboBoxItem
        {
            public string DisplayText { get; set; }
            public Guid Value { get; set; }

            public override string ToString()
            {
                return DisplayText;
            }
        }
    }
}