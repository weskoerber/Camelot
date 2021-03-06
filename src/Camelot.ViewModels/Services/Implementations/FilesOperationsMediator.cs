using System;
using Camelot.Services.EventArgs;
using Camelot.Services.Interfaces;
using Camelot.ViewModels.Interfaces.MainWindow.FilePanels;
using Camelot.ViewModels.Services.Interfaces;

namespace Camelot.ViewModels.Services.Implementations
{
    public class FilesOperationsMediator : IFilesOperationsMediator
    {
        private readonly IDirectoryService _directoryService;
        
        public string OutputDirectory => InactiveFilesPanelViewModel.CurrentDirectory;

        public IFilesPanelViewModel ActiveFilesPanelViewModel { get; private set; }
        
        public IFilesPanelViewModel InactiveFilesPanelViewModel { get; private set; }

        public FilesOperationsMediator(
            IDirectoryService directoryService)
        {
            _directoryService = directoryService;

            directoryService.SelectedDirectoryChanged += DirectoryServiceOnSelectedDirectoryChanged;
        }

        public void Register(IFilesPanelViewModel activeFilesPanelViewModel, IFilesPanelViewModel inactiveFilesPanelViewModel)
        {
            (ActiveFilesPanelViewModel, InactiveFilesPanelViewModel) = (activeFilesPanelViewModel, inactiveFilesPanelViewModel);

            SubscribeToEvents(ActiveFilesPanelViewModel);
            SubscribeToEvents(InactiveFilesPanelViewModel);

            UpdateCurrentDirectory();
            
            ActiveFilesPanelViewModel.Activate();
            InactiveFilesPanelViewModel.Deactivate();
        }

        private void SubscribeToEvents(IFilesPanelViewModel filesPanelViewModel) =>
            filesPanelViewModel.ActivatedEvent += FilesPanelViewModelOnActivatedEvent;

        private void FilesPanelViewModelOnActivatedEvent(object sender, EventArgs e)
        {
            var filesPanelViewModel = (IFilesPanelViewModel) sender;
            if (filesPanelViewModel == ActiveFilesPanelViewModel)
            {
                return;
            }

            SwapViewModels();
            UpdateCurrentDirectory();
            DeactivateInactiveViewModel();
        }

        private void DirectoryServiceOnSelectedDirectoryChanged(object sender, SelectedDirectoryChangedEventArgs e) =>
            ActiveFilesPanelViewModel.CurrentDirectory = e.NewDirectory;

        private void SwapViewModels() =>
            (InactiveFilesPanelViewModel, ActiveFilesPanelViewModel) =
            (ActiveFilesPanelViewModel, InactiveFilesPanelViewModel);

        private void UpdateCurrentDirectory() =>
            _directoryService.SelectedDirectory = ActiveFilesPanelViewModel.CurrentDirectory;

        private void DeactivateInactiveViewModel() => InactiveFilesPanelViewModel.Deactivate();
    }
}