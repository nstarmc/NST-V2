Imports Downloader
Imports System.Net

Module Downloader
    Function downloader_config_read() As DownloadConfiguration
        Dim downloadOpt = New DownloadConfiguration()
        downloadOpt.BufferBlockSize = 10240 '文件缓冲区大小
        downloadOpt.ChunkCount = 8 '下载线程数量
        downloadOpt.MinimumSizeOfChunking = 16
        'downloadOpt.ChunkCount = Data("download")("thr") '下载线程数量
        downloadOpt.MaximumBytesPerSecond = 0 '下载限速
        downloadOpt.Timeout = 1000 '超时
        downloadOpt.MaxTryAgainOnFailover = Integer.MaxValue
        If downloadOpt.ChunkCount = 1 Then
            downloadOpt.ParallelDownload = False
        Else
            downloadOpt.ParallelDownload = True
        End If

        downloadOpt.RequestConfiguration.Accept = "*/*"
        downloadOpt.RequestConfiguration.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        downloadOpt.RequestConfiguration.CookieContainer = New CookieContainer()
        downloadOpt.RequestConfiguration.Headers = New WebHeaderCollection()
        downloadOpt.RequestConfiguration.KeepAlive = False
        downloadOpt.RequestConfiguration.ProtocolVersion = HttpVersion.Version11
        downloadOpt.RequestConfiguration.UseDefaultCredentials = False
        downloadOpt.RequestConfiguration.UserAgent = "nstarmc_tools_v2"
        log_record("Downloader-Config", "成功读取下载配置。")
        Return downloadOpt
    End Function
End Module
