namespace Camelot.Services.EventArgs
{
    public class FileRenamedEventArgs : FileEventArgsBase
    {
        public string NewName { get; }

        public FileRenamedEventArgs(string node, string newName)
            : base(node)
        {
            NewName = newName;
        }
    }
}