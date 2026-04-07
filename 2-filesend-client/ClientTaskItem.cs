namespace _2_filesend_client
{
    internal enum ClientTaskStatus
    {
        Queued,
        Transferring,
        Success,
        Failed
    }

    internal static class ClientTaskStatusExtensions
    {
        public static string ToDisplayText(this ClientTaskStatus status) =>
            status switch
            {
                ClientTaskStatus.Queued => "排队",
                ClientTaskStatus.Transferring => "传输中",
                ClientTaskStatus.Success => "成功",
                ClientTaskStatus.Failed => "失败",
                _ => "失败"
            };
    }

    internal sealed class ClientTaskItem
    {
        public ClientTaskItem(string fileName, string sourcePath, string relativePath)
        {
            FileName = fileName;
            SourcePath = sourcePath;
            RelativePath = relativePath;
        }

        public string FileName { get; }
        public string SourcePath { get; }
        public string RelativePath { get; }
        public string? ResumeKey { get; set; }
        public ClientTaskStatus Status { get; set; }
        public DataGridViewRow? Row { get; set; }
    }
}
