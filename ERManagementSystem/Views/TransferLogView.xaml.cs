using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;

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
                var transferDir = Path.Combine(AppContext.BaseDirectory, "transfers");

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