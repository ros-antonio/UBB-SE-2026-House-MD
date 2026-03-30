using ERManagementSystem.DataAccess;
using ERManagementSystem.Helpers;
using ERManagementSystem.Repositories;
using ERManagementSystem.Services;
using ERManagementSystem.ViewModels;
using ERManagementSystem.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace ERManagementSystem
{
    public partial class App : Application
    {
        public static ServiceProvider Services { get; private set; } = null!;

        // Expose the main window so ViewModels can access XamlRoot for dialogs
        public static Window? MainAppWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // ── Core infrastructure ──────────────────────────────────────────
            services.AddSingleton<DatabaseConnection>();
            services.AddSingleton<SqlHelper>();

            // ── Navigation ───────────────────────────────────────────────────
            services.AddSingleton<NavigationService>();
            services.AddSingleton<INavigationService>(sp =>
                sp.GetRequiredService<NavigationService>());

            // ── Repositories (Miruna's: Patient & Visit) ─────────────────────
            services.AddTransient<PatientRepository>();
            services.AddTransient<ERVisitRepository>();

            // ── Repositories (Triage) ─────────────────────────────────────────
            services.AddTransient<TriageRepository>();
            services.AddTransient<TriageParametersRepository>();

            // ── Services (Miruna's: Registration & State) ────────────────────
            services.AddTransient<RegistrationService>();
            services.AddTransient<StateManagementService>();

            // ── Services (Triage & Queue) ────────────────────────────────────
            services.AddSingleton<NurseService>();
            services.AddTransient<TriageService>();
            services.AddTransient<QueueService>();

            // ── ViewModels ───────────────────────────────────────────────────
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<PatientRegistrationViewModel>();
            services.AddTransient<TriageViewModel>();
            services.AddTransient<QueueViewModel>();

            // ── Views ────────────────────────────────────────────────────────
            services.AddTransient<PatientRegistrationView>();
            services.AddTransient<TriageView>();
            services.AddTransient<QueueView>();

            Services = services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }
    }
}