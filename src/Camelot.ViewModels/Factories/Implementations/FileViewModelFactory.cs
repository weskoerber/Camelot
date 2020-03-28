using System.Globalization;
using Camelot.Services.Behaviors.Interfaces;
using Camelot.Services.Interfaces;
using Camelot.Services.Models;
using Camelot.ViewModels.Factories.Interfaces;
using Camelot.ViewModels.Implementations.MainWindow;
using Camelot.ViewModels.Interfaces.MainWindow;

namespace Camelot.ViewModels.Factories.Implementations
{
    public class FileSystemNodeViewModelFactory : IFileSystemNodeViewModelFactory
    {
        private readonly IFileSystemNodeOpeningBehavior _fileSystemNodeOpeningBehavior;
        private readonly IFileSystemNodeOpeningBehavior _directorySystemNodeOpeningBehavior;
        private readonly IFileSizeFormatter _fileSizeFormatter;
        private readonly IPathService _pathService;

        public FileSystemNodeViewModelFactory(
            IFileSystemNodeOpeningBehavior fileSystemNodeOpeningBehavior,
            IFileSystemNodeOpeningBehavior directorySystemNodeOpeningBehavior,
            IFileSizeFormatter fileSizeFormatter,
            IPathService pathService)
        {
            _fileSystemNodeOpeningBehavior = fileSystemNodeOpeningBehavior;
            _directorySystemNodeOpeningBehavior = directorySystemNodeOpeningBehavior;
            _fileSizeFormatter = fileSizeFormatter;
            _pathService = pathService;
        }

        public IFileSystemNodeViewModel Create(FileModel fileModel)
        {
            var fileViewModel = new FileViewModel(_fileSystemNodeOpeningBehavior)
            {
                FullPath = fileModel.FullPath,
                Size = _fileSizeFormatter.GetFormattedSize(fileModel.SizeBytes),
                LastModifiedDateTime = fileModel.LastWriteTime.ToString(CultureInfo.CurrentCulture),
                FileName = _pathService.GetFileNameWithoutExtension(fileModel.Name),
                Extension = _pathService.GetExtension(fileModel.Name)
            };

            return fileViewModel;
        }

        public IFileSystemNodeViewModel Create(DirectoryModel directoryModel)
        {
            var fileViewModel = new DirectoryViewModel(_directorySystemNodeOpeningBehavior)
            {
                FullPath = directoryModel.FullPath,
                LastModifiedDateTime = directoryModel.LastModifiedDateTime.ToString(CultureInfo.CurrentCulture),
            };

            return fileViewModel;
        }
    }
}