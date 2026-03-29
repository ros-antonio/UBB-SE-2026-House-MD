using ERManagementSystem.ViewModels;
using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class TransferLogView : Page
    {
        public TransferLogViewModel? ViewModel { get; private set; }

        public TransferLogView()
        {
            InitializeComponent();
        }

        public TransferLogView(TransferLogViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ViewModel == null)
            {
                var dbConnection = new DataAccess.DatabaseConnection();
                var sqlHelper = new Helpers.SqlHelper(dbConnection);
                var transferDir = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.MyDocuments), "ERTransfers");

                var erVisitRepo = new ERVisitRepository(sqlHelper);
                var stateSvc = new StateManagementService(erVisitRepo);

                var transferService = new TransferService(sqlHelper, transferDir, stateSvc);
                ViewModel = new TransferLogViewModel(transferService, sqlHelper);
            }

            ViewModel.XamlRoot = this.XamlRoot;
            ViewModel.LoadData();
        }
    }
}