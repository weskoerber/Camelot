using System.IO;
using System.Reflection;
using ApplicationDispatcher.Implementations;
using ApplicationDispatcher.Interfaces;
using Camelot.DataAccess.Configuration;
using Camelot.DataAccess.LiteDb;
using Camelot.DataAccess.UnitOfWork;
using Camelot.FileSystemWatcherWrapper.Implementations;
using Camelot.FileSystemWatcherWrapper.Interfaces;
using Camelot.Services.Behaviors.Implementations;
using Camelot.Services.Environment.Implementations;
using Camelot.Services.Environment.Interfaces;
using Camelot.Services.Implementations;
using Camelot.Services.Interfaces;
using Camelot.Services.Operations.Implementations;
using Camelot.Services.Operations.Interfaces;
using Camelot.TaskPool.Interfaces;
using Camelot.ViewModels.Configuration;
using Camelot.ViewModels.Factories.Implementations;
using Camelot.ViewModels.Factories.Interfaces;
using Camelot.ViewModels.Implementations;
using Camelot.ViewModels.Implementations.Dialogs;
using Camelot.ViewModels.Implementations.MainWindow;
using Camelot.ViewModels.Implementations.MainWindow.FilePanels;
using Camelot.ViewModels.Implementations.Menu;
using Camelot.ViewModels.Interfaces.MainWindow;
using Camelot.ViewModels.Interfaces.MainWindow.FilePanels;
using Camelot.ViewModels.Services.Implementations;
using Camelot.ViewModels.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Splat;

namespace Camelot
{
    public static class Bootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            RegisterConfiguration(services);
            RegisterEnvironmentServices(services);
            RegisterAvaloniaServices(services);
            RegisterFileSystemWatcherServices(services);
            RegisterTaskPool(services, resolver);
            RegisterDataAccess(services, resolver);
            RegisterServices(services, resolver);
            RegisterViewModels(services, resolver);
        }

        private static void RegisterConfiguration(IMutableDependencyResolver services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            var aboutDialogConfiguration = new AboutDialogConfiguration();
            configuration.GetSection("About").Bind(aboutDialogConfiguration);
            
            services.RegisterConstant(aboutDialogConfiguration);

            var databaseName = configuration["DataAccess:DatabaseName"];
            var assemblyLocation = Assembly.GetEntryAssembly()?.Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            var databaseConfiguration = new DatabaseConfiguration
            {
                ConnectionString = Path.Combine(assemblyDirectory, databaseName)
            };
            
            services.RegisterConstant(databaseConfiguration);
        }

        private static void RegisterEnvironmentServices(IMutableDependencyResolver services)
        {
            services.RegisterLazySingleton<IEnvironmentService>(() => new EnvironmentService());
            services.RegisterLazySingleton<IProcessService>(() => new ProcessService());
            services.RegisterLazySingleton<IPlatformService>(() => new PlatformService());
        }
        
        private static void RegisterAvaloniaServices(IMutableDependencyResolver services)
        {
            services.RegisterLazySingleton<IApplicationCloser>(() => new ApplicationCloser());
            services.RegisterLazySingleton<IApplicationDispatcher>(() => new AvaloniaDispatcher());
            services.RegisterLazySingleton<IApplicationVersionProvider>(() => new ApplicationVersionProvider());
        }
        
        private static void RegisterFileSystemWatcherServices(IMutableDependencyResolver services)
        {
            services.RegisterLazySingleton<IFileSystemWatcherWrapperFactory>(() => new FileSystemWatcherWrapperFactory());
        }
        
        private static void RegisterTaskPool(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton<ITaskPool>(() => new TaskPool.Implementations.TaskPool(
                resolver.GetService<IEnvironmentService>()
            ));
        }

        private static void RegisterDataAccess(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton<IUnitOfWorkFactory>(() => new LiteDbUnitOfWorkFactory(
                resolver.GetService<DatabaseConfiguration>()
            ));
            services.RegisterLazySingleton<IClipboardService>(() => new ClipboardService());
            services.RegisterLazySingleton<IMainWindowProvider>(() => new MainWindowProvider());
        }

        private static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton<IFileService>(() => new FileService(
                resolver.GetService<IPathService>()
            ));
            services.RegisterLazySingleton<IDriveService>(() => new DriveService());
            services.RegisterLazySingleton<ITrashCanServiceFactory>(() => new TrashCanServiceFactory(
                resolver.GetService<IPlatformService>(),
                resolver.GetService<IDriveService>(),
                resolver.GetService<IOperationsService>(),
                resolver.GetService<IEnvironmentService>(),
                resolver.GetService<IPathService>(),
                resolver.GetService<IFileService>(),
                resolver.GetService<IProcessService>(),
                resolver.GetService<IDirectoryService>()
            ));
            services.Register<IOperationsFactory>(() => new OperationsFactory(
                resolver.GetService<ITaskPool>(),
                resolver.GetService<IDirectoryService>(),
                resolver.GetService<IFileService>(),
                resolver.GetService<IPathService>()
            ));
            services.RegisterLazySingleton<IFileSystemWatchingService>(() => new FileSystemWatchingService(
                resolver.GetService<IFileSystemWatcherWrapperFactory>()
            ));
            services.RegisterLazySingleton<IFilesSelectionService>(() => new FilesSelectionService());
            services.RegisterLazySingleton<IOperationsService>(() => new OperationsService(
                resolver.GetService<IOperationsFactory>(),
                resolver.GetService<IDirectoryService>(),
                resolver.GetService<IResourceOpeningService>(),
                resolver.GetService<IFileService>(),
                resolver.GetService<IPathService>()
            ));
            services.RegisterLazySingleton<IDirectoryService>(() => new DirectoryService(
                resolver.GetService<IPathService>()
            ));
            services.RegisterLazySingleton<IResourceOpeningService>(() => new ResourceOpeningService(
                resolver.GetService<IProcessService>(),
                resolver.GetService<IPlatformService>()
            ));
            services.RegisterLazySingleton<IFileSystemWatcherWrapperFactory>(() => new FileSystemWatcherWrapperFactory());
            services.Register<IFileSystemWatchingService>(() => new FileSystemWatchingService(
                resolver.GetService<IFileSystemWatcherWrapperFactory>()
            ));
            services.RegisterLazySingleton(() => new FileOpeningBehavior(
                resolver.GetService<IResourceOpeningService>()
            ));
            services.RegisterLazySingleton(() => new DirectoryOpeningBehavior(
                resolver.GetService<IDirectoryService>()
            ));
            services.RegisterLazySingleton<IFileSizeFormatter>(() => new FileSizeFormatter());
            services.RegisterLazySingleton<IPathService>(() => new PathService());
            services.RegisterLazySingleton<IDialogService>(() => new DialogService(
                resolver.GetService<IMainWindowProvider>()
            ));
            services.RegisterLazySingleton<IClipboardOperationsService>(() => new ClipboardOperationsService(
                resolver.GetService<IClipboardService>(),
                resolver.GetService<IOperationsService>(),
                resolver.GetService<IEnvironmentService>()
            ));
        }

        private static void RegisterViewModels(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            services.RegisterLazySingleton<IFilesOperationsMediator>(() => new FilesOperationsMediator(
                resolver.GetService<IDirectoryService>()
            ));
            services.RegisterLazySingleton<ITabViewModelFactory>(() => new TabViewModelFactory(
                resolver.GetService<IPathService>()
            ));
            services.RegisterLazySingleton<IFileSystemNodeViewModelFactory>(() => new FileSystemNodeViewModelFactory(
                resolver.GetService<FileOpeningBehavior>(),
                resolver.GetService<DirectoryOpeningBehavior>(),
                resolver.GetService<IFileSizeFormatter>(),
                resolver.GetService<IPathService>(),
                resolver.GetService<IOperationsService>(),
                resolver.GetService<IClipboardOperationsService>(),
                resolver.GetService<IFilesOperationsMediator>()

            ));
            services.Register(() => new AboutDialogViewModel(
                resolver.GetService<IApplicationVersionProvider>(),
                resolver.GetService<IResourceOpeningService>(),
                resolver.GetService<AboutDialogConfiguration>()
            ));
            services.Register(() => new CreateDirectoryDialogViewModel());
            services.Register(() => new RemoveNodesConfirmationDialogViewModel(
                resolver.GetService<IPathService>()
            ));
            services.Register<IOperationsViewModel>(() => new OperationsViewModel(
                resolver.GetService<IFilesOperationsMediator>(),
                resolver.GetService<IOperationsService>(),
                resolver.GetService<IFilesSelectionService>(),
                resolver.GetService<IDialogService>(),
                resolver.GetService<IDirectoryService>(),
                resolver.GetService<ITrashCanServiceFactory>()
            ));
            services.Register(() => new MenuViewModel(
                resolver.GetService<IApplicationCloser>(),
                resolver.GetService<IDialogService>()
            ));
            services.RegisterLazySingleton(() => new MainWindowViewModel(
                resolver.GetService<IFilesOperationsMediator>(),
                resolver.GetService<IOperationsViewModel>(),
                CreateFilesPanelViewModel(resolver, "Left"),
                CreateFilesPanelViewModel(resolver, "Right"),
                resolver.GetService<MenuViewModel>()
            ));
        }

        private static IFilesPanelViewModel CreateFilesPanelViewModel(
            IReadonlyDependencyResolver resolver,
            string panelKey)
        {
            var filesPanelStateService = new FilesPanelStateService(
                resolver.GetService<IUnitOfWorkFactory>(),
                panelKey
            );
            var filesPanelViewModel = new FilesPanelViewModel(
                resolver.GetService<IFileService>(),
                resolver.GetService<IDirectoryService>(),
                resolver.GetService<IFilesSelectionService>(),
                resolver.GetService<IFileSystemNodeViewModelFactory>(),
                resolver.GetService<IFileSystemWatchingService>(),
                resolver.GetService<IApplicationDispatcher>(),
                filesPanelStateService,
                resolver.GetService<ITabViewModelFactory>(),
                resolver.GetService<IFileSizeFormatter>(),
                resolver.GetService<IClipboardOperationsService>()
            );

            return filesPanelViewModel;
        }
    }
}