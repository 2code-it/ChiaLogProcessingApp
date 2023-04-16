using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ChiaLogProcessingApp.Core.Services;
using ChiaLogProcessingApp.Core.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace ChiaLogProcessingApp.Wpf
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			Services = ConfigureServices();
			_logProcessingService = ActivatorUtilities.CreateInstance<Core.Services.LogProcessingService>(Services);
		}

		public IServiceProvider Services { get; private set; }

		private readonly Core.Services.LogProcessingService _logProcessingService;
		private readonly static object _locker = new object();

		protected override void OnStartup(StartupEventArgs e)
		{
			Views.MainWindow mainWindow = new Views.MainWindow();
			mainWindow.DataContext = GetViewModel<Core.ViewModels.MainWindowViewModel>(); 
			mainWindow.Show();

			base.OnStartup(e);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			
			Core.Models.UserSettings userSettings = Services.GetRequiredService<Core.Models.UserSettings>();
			Core.Services.IJsonSettingsService jsonSettingsService = Services.GetRequiredService<Core.Services.IJsonSettingsService>();

			jsonSettingsService.Save(userSettings);
			_logProcessingService.Dispose();

			base.OnExit(e);
		}

		private static IServiceProvider ConfigureServices()
		{
			ServiceCollection serviceCollection = new ServiceCollection();

			Core.Services.IFileSystemService fileSystemService = new Core.Services.FileSystemService();
			Core.Services.IJsonSettingsService jsonSettingsService = new Core.Services.JsonSettingsService(fileSystemService);
			Core.Models.UserSettings userSettings = jsonSettingsService.Load<Core.Models.UserSettings>();

			serviceCollection.AddSingleton(fileSystemService);
			serviceCollection.AddSingleton(jsonSettingsService);
			serviceCollection.AddSingleton<Core.Services.IFileWatcherService, Core.Services.FileWatcherService>();
			serviceCollection.AddSingleton<Core.Services.IDialogService, Services.WindowsDialogService>();
			serviceCollection.AddSingleton<Core.Services.IHttpFileService, Core.Services.HttpFileService>();
			serviceCollection.AddSingleton<IMessenger>(StrongReferenceMessenger.Default);
			serviceCollection.AddSingleton(userSettings);
			serviceCollection.AddSingleton<Core.ViewModels.MainWindowViewModel>();

			return serviceCollection.BuildServiceProvider();
		}

		private T GetViewModel<T>() where T : class
		{
			T viewModel = Services.GetRequiredService<T>();

			IEnumerable collections = typeof(T).GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
				.Select(x => x.GetValue(viewModel))
				.Where(x => x!= null)
				.Cast<IEnumerable>()
				.ToArray();

			this.Dispatcher.BeginInvoke(() =>
			{
				foreach (IEnumerable collection in collections)
				{
					BindingOperations.EnableCollectionSynchronization(collection, _locker);
				}
			});
			return viewModel;
		}
	}
}
