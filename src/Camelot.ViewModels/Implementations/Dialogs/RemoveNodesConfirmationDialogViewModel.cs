using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Camelot.Services.Interfaces;
using Camelot.ViewModels.Implementations.NavigationParameters;
using ReactiveUI;

namespace Camelot.ViewModels.Implementations.Dialogs
{
    public class RemoveNodesConfirmationDialogViewModel : ParameterizedDialogViewModelBase<bool, NodesRemovingNavigationParameter>
    {
        private readonly IPathService _pathService;
        private const int ShowedFilesLimit = 4;
        
        private IEnumerable<string> _files;
        private bool _isRemovingToTrash;
        
        public IEnumerable<string> Files
        {
            get => _files;
            set
            {
                this.RaiseAndSetIfChanged(ref _files, value);
                this.RaisePropertyChanged(nameof(FilesCount));
                this.RaisePropertyChanged(nameof(ShouldShowFilesList));
            }
        }

        public int FilesCount => Files.Count();

        public bool ShouldShowFilesList => FilesCount <= ShowedFilesLimit;

        public bool IsRemovingToTrash
        {
            get => _isRemovingToTrash;
            set => this.RaiseAndSetIfChanged(ref _isRemovingToTrash, value);
        }
        
        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }
        
        public RemoveNodesConfirmationDialogViewModel(
            IPathService pathService)
        {
            _pathService = pathService;
            
            OkCommand = ReactiveCommand.Create(() => Close(true));
            CancelCommand = ReactiveCommand.Create(() => Close());
        }
        
        public override void Activate(NodesRemovingNavigationParameter parameter)
        {
            Files = parameter.Files.Select(_pathService.GetFileName);
            IsRemovingToTrash = parameter.IsRemovingToTrash;
        }
    }
}