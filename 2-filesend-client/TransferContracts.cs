namespace _2_filesend_client
{
    internal sealed class TransferStartRequest
    {
        public string FileName { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ResumeKey { get; set; } = string.Empty;
    }

    internal sealed class TransferStartResponse
    {
        public bool Accepted { get; set; }
        public long ResumeOffset { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    internal sealed class TransferCompleteResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
