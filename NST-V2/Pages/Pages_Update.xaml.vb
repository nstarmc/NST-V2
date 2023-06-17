Imports System.ComponentModel
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Threading
Imports Downloader
Imports Newtonsoft.Json.Linq
Imports NST_V2.Pages_Download

Class Pages_Update
    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub Page_Initialized(sender As Object, e As EventArgs)
        Dim thr1 As New Thread(AddressOf getinfo_thr)
        thr1.Start()
    End Sub

    Private Sub getinfo_thr(obj As Object)
        Dim isAccessible As Boolean = TestWebsiteAccessibility(get_server_domain_https() & "/test.txt")
        If isAccessible Then
            Dim json As JObject = json_main()
            vinfo.Dispatcher.Invoke(New Action(Sub()
                                                   vinfo.Text = "最新版本：" & json("Version").ToString & vbCrLf & "当前版本：" & Application.ResourceAssembly.GetName().Version.ToString()
                                                   his_text.Text = json("History").ToString
                                               End Sub))
        Else
            HandyControl.Controls.MessageBox.Show("网络错误！无法连接到服务器！", "无法更新！", MessageBoxButton.OK, MessageBoxImage.Error)
        End If
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim thr1 As New Thread(AddressOf upd_thr)
        thr1.Start()
        bt_on.IsEnabled = False
    End Sub

    Private Async Sub upd_thr(obj As Object)
        '执行更新
        text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                     pg_dw.Maximum = 100
                                                     text_dwinfo.Text = "开始处理下载任务···"
                                                 End Sub))
        Try
            File.Delete(Path_Root() & "\Main_New.exe")
            File.Delete(Path_Root() & "\upd.bat")
        Catch ex As Exception

        End Try
        Dim dlurl = (json_main("DL").ToString)
        Dim downloader = New DownloadService(downloader_config_read)
        AddHandler downloader.DownloadProgressChanged, AddressOf OnDownloadProgressChanged
        'AddHandler downloader.DownloadFileCompleted, AddressOf Ondlcm
        Await downloader.DownloadFileTaskAsync(json_main("DL").ToString, Path_Root() & "\Main_New.exe")
        '写入bat替换脚本
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
        Dim file1 As System.IO.StreamWriter = New StreamWriter(Path_Root() & "\upd.bat", False, Encoding.GetEncoding("GB2312"))
        'file = My.Computer.FileSystem.OpenTextFileWriter(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\upd.bat", False, Encoding.GetEncoding("GB2312"))
        file1.WriteLine("@echo off")
        file1.WriteLine("title NSTARMC-Tools V2更新中······")
        file1.WriteLine("color 9")
        file1.WriteLine("echo [NSTARMC-Tools V2更新脚本-V" & Assembly.GetExecutingAssembly().GetName().Version.ToString() & "]")
        file1.WriteLine("echo 正在更新NSTARMC-Tools V2到最新版本：V" & json_main("Version").ToString)
        file1.WriteLine("TIMEOUT /T 1 >nul")
        file1.WriteLine("taskkill /F /IM " & Assembly.GetEntryAssembly().GetName().Name & ".exe 2>nul")
        file1.WriteLine("TIMEOUT /T 1 >nul")
        file1.WriteLine("copy """ & Path_Root() & "\main_new.exe"" """ & Path.Combine(AppContext.BaseDirectory, Assembly.GetEntryAssembly().GetName().Name & ".exe") & """ >nul")
        file1.WriteLine("start """" """ & Path.Combine(AppContext.BaseDirectory, Assembly.GetEntryAssembly().GetName().Name & ".exe") & """ >nul")
        file1.WriteLine("del """ & Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) & "\main_new.exe" & """ >nul")
        file1.WriteLine("title NSTARMC-Tools V2更新完成！")
        file1.WriteLine("echo 更新完成！请按任意键关闭本窗口~")
        file1.WriteLine("pause >nul")
        file1.Close()

        '替换
        HandyControl.Controls.MessageBox.Show("更新包下载完成！即将重启更新！"， "更新", MessageBoxButton.OK, MessageBoxImage.Information)
        text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                     Dim p As New Process()
                                                     p.StartInfo.FileName = "cmd.exe"
                                                     p.StartInfo.UseShellExecute = False
                                                     p.StartInfo.WorkingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                                                     p.StartInfo.RedirectStandardInput = True
                                                     p.StartInfo.RedirectStandardOutput = True
                                                     p.StartInfo.RedirectStandardError = True
                                                     p.StartInfo.CreateNoWindow = True
                                                     p.Start()
                                                     p.StandardInput.WriteLine("start "" "" upd.bat") '这个Data就是cmd命令
                                                 End Sub))
    End Sub



    Private Sub OnDownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs)
        text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                     text_dwinfo.Text = "文件总大小：" & Math.Round(e.TotalBytesToReceive / 1024 / 1024, 2) & "MB" & vbCrLf &
                                                 "文件已下载：" & Math.Round(e.ReceivedBytesSize / 1024 / 1024, 2) & "MB" & vbCrLf &
                                                 "当前速度：" & Math.Round(e.BytesPerSecondSpeed / 1024 / 1024, 2) & "MB/S" & vbCrLf &
                                                 "平均速度：" & Math.Round(e.AverageBytesPerSecondSpeed / 1024 / 1024, 2) & "MB/S"
                                                     pg_dw.Value = Math.Round(e.ProgressPercentage, 1)
                                                 End Sub))
    End Sub
End Class
