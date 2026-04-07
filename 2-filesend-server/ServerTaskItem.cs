namespace _2_filesend_server
{
    internal enum ServerTaskStatus
    {
        Transferring,
        Success,
        Failed
    }

    internal static class ServerTaskStatusExtensions
    {
        public static string ToDisplayText(this ServerTaskStatus status) =>
            status switch
            {
                ServerTaskStatus.Transferring => "传输中",
                ServerTaskStatus.Success => "成功",
                ServerTaskStatus.Failed => "失败",
                _ => "失败"
            };
    }

    internal sealed class ServerTaskItem
    {
        public string ResumeKey { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public ServerTaskStatus Status { get; set; }
        public DataGridViewRow? Row { get; set; }
    }

    internal sealed class ResumeRecord
    {
        public string ResumeKey { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string TempPath { get; set; } = string.Empty;
    }
}
