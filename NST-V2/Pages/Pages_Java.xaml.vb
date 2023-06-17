Imports System.IO
Imports System.Threading
Imports Downloader
Imports HandyControl.Controls
Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports Newtonsoft.Json.Linq
Imports NST_V2.Pages_Download
Imports Wpf.Ui.Dpi

Class Pages_Java
    Dim dlinfo As New dwinfo
    Private Sub Page_Initialized(sender As Object, e As EventArgs)
        '页面初始化
        Dim thr1 As New Thread(AddressOf initialize_thr)
        thr1.Start()
    End Sub

    Private Sub initialize_thr(obj As Object)
        log_record("DLJava", "开始执行主页初始化！")
        '遍历读取整合包
        mclist.Dispatcher.Invoke(New Action(Sub()
                                                mclist.Items.Clear()
                                            End Sub))
        For Each dirlist In Directory.GetDirectories(Path_Root() & "\MinecraftFiles\")
            If File.Exists(dirlist & "\MCVersionInfo.json") Then
                '检测到文件夹内为符合格式的整合包，加载
                log_record("DLJava", "检测到整合包！文件夹：" & dirlist)
                Dim info_reader As String = File.ReadAllText(dirlist & "\MCVersionInfo.json")
                Dim localinfo As JObject = read_localvinfo(info_reader)
                mclist.Dispatcher.Invoke(New Action(Sub()
                                                        mclist.Items.Add(localinfo("MCVersion").ToString & "-" & localinfo("ModLoader").ToString & "[" & localinfo("ShaderLoader").ToString & "]~" & localinfo("PackID").ToString)
                                                    End Sub))
            End If
        Next
        '没有整合包的处理
        If mclist.Items.Count = 0 Then
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    log_record("DLJava", "没有检测到任何整合包！")
                                                    mclist.IsEnabled = False
                                                    Growl.Warning("本地不存在整合包！" & vbCrLf & “请前往下载整合包！”)
                                                End Sub))
        Else
            mclist.Dispatcher.Invoke(New Action(Sub()
                                                    mclist.SelectedIndex = 0
                                                    Growl.Success("成功加载整合包" & mclist.Items.Count & "个！")
                                                End Sub））

        End If
    End Sub

    Private Sub mclist_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles mclist.SelectionChanged
        '选择改变时，启用线程read信息
        Dim thr1 As New Thread(AddressOf mclist_changed)
        thr1.Start()
    End Sub

    Private Sub mclist_changed(obj As Object)
        For Each dirlist In Directory.GetDirectories(Path_Root() & "\MinecraftFiles\")
            If File.Exists(dirlist & "\MCVersionInfo.json") Then
                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = File.ReadAllText(dirlist & "\MCVersionInfo.json")
                Dim localinfo As JObject = read_localvinfo(info_reader)
                java_info.Dispatcher.Invoke(New Action(Sub()
                                                           If mclist.SelectedItem.ToString = localinfo("MCVersion").ToString & "-" & localinfo("ModLoader").ToString & "[" & localinfo("ShaderLoader").ToString & "]~" & localinfo("PackID").ToString Then
                                                               java_info.Text = "Java " & localinfo("JavaVersion").ToString
                                                               log_record("DLJava", "读取整合包信息：" & dirlist)
                                                           End If
                                                       End Sub))
            End If
        Next
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim thr1 As New Thread(AddressOf dljava_thr)
        thr1.Start()
        bt_on.IsEnabled = True
    End Sub

    Private Async Sub dljava_thr(obj As Object)
        Dim isAccessible As Boolean = TestWebsiteAccessibility(get_server_domain_https() & "/test.txt")
        If isAccessible Then
            For Each dirlist In Directory.GetDirectories(Path_Root() & "\MinecraftFiles\")
                If File.Exists(dirlist & "\MCVersionInfo.json") Then
                    '检测到文件夹内为符合格式的整合包，加载
                    Dim info_reader As String = File.ReadAllText(dirlist & "\MCVersionInfo.json")
                    Dim localinfo As JObject = read_localvinfo(info_reader)
                    java_info.Dispatcher.Invoke(New Action(Sub()
                                                               If mclist.SelectedItem.ToString = localinfo("MCVersion").ToString & "-" & localinfo("ModLoader").ToString & "[" & localinfo("ShaderLoader").ToString & "]~" & localinfo("PackID").ToString Then
                                                                   dlinfo.url = json_java("Java" & localinfo("JavaVersion").ToString)
                                                                   dlinfo.rootpath = dirlist
                                                                   log_record("DLJava", "读取整合包信息：" & dirlist)
                                                               End If
                                                           End Sub))
                End If
            Next

            '开始下载
            Growl.Info("开始处理Java下载任务！")
            Try
                Directory.Delete(dlinfo.rootpath & "\jre-x64\", True)
                Directory.CreateDirectory(dlinfo.rootpath & "\jre-x64\")
            Catch ex As Exception

            End Try
            text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                         pg_dw.Maximum = 100
                                                     End Sub))

            Dim downloader = New DownloadService(downloader_config_read)
            AddHandler downloader.DownloadProgressChanged, AddressOf OnDownloadProgressChanged
            Await downloader.DownloadFileTaskAsync(dlinfo.url, dlinfo.rootpath & "\jre-x64\java.zip")

            '——————解压——————
            Text.Encoding.RegisterProvider(Text.CodePagesEncodingProvider.Instance)
            Dim encode As Text.Encoding = Text.Encoding.GetEncoding("GB2312")
            ZipStrings.CodePage = encode.CodePage
            Dim zf As ZipFile = Nothing
            Try
                Dim fs As FileStream = File.OpenRead(dlinfo.rootpath & "\jre-x64\java.zip")
                zf = New ZipFile(fs)
                pg_dw.Dispatcher.Invoke(New Action(Sub()
                                                       pg_dw.Maximum = zf.Count
                                                       pg_dw.Value = 0
                                                   End Sub))
                Dim ii = 0
                For Each zipEntry As ZipEntry In zf
                    ii = ii + 1
                    If Not zipEntry.IsFile Then     ' 忽略目录
                        Continue For
                    End If
                    Dim entryFileName As String = zipEntry.Name
                    ' 从条目中删除文件夹：- entryFileName = Path.GetFileName（entryFileName）;
                    ' （可选）将条目名称与此处的选择列表匹配，以便根据需要跳过。
                    ' 解包长度在 zipEntry.Size 属性中可用.

                    Dim buffer As Byte() = New Byte(4095) {}    ' 4K is optimum
                    Dim zipStream As Stream = zf.GetInputStream(zipEntry)

                    ' Manipulate the output filename here as desired.
                    Dim fullZipToPath As [String] = Path.Combine(dlinfo.rootpath & "\jre-x64\", entryFileName）
                    Dim directoryName As String = Path.GetDirectoryName(fullZipToPath)
                    If directoryName.Length > 0 Then
                        Directory.CreateDirectory(directoryName)
                    End If

                    ' Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    ' of the file, but does not waste memory.
                    ' The "Using" will close the stream even if an exception occurs.
                    Using streamWriter As FileStream = File.Create(fullZipToPath)
                        StreamUtils.Copy(zipStream, streamWriter, buffer)

                    End Using
                    pg_dw.Dispatcher.Invoke(New Action(Sub()
                                                           text_dwinfo.Text = "正在解压：" & ii & "/" & pg_dw.Maximum
                                                           pg_dw.Value = ii
                                                       End Sub))
                Next
            Finally
                If zf IsNot Nothing Then
                    zf.IsStreamOwner = True     ' Makes close also shut the underlying stream
                    ' Ensure we release resources
                    zf.Close()
                    log_record("DLJava", "文件解压缩完成。")
                    Try
                        File.Delete(dlinfo.rootpath & "\jre-x64\java.zip")

                    Catch ex As Exception

                    End Try

                    log_record("DLJava", "删除Java压缩文件完成。")
                End If
            End Try
            '——————解压——————
            Growl.Info("Java下载完成！")
            bt_on.Dispatcher.Invoke(New Action(Sub()
                                                   bt_on.IsEnabled = False
                                               End Sub))
        Else
            bt_on.Dispatcher.Invoke(New Action(Sub()
                                                   bt_on.IsEnabled = False
                                               End Sub))
        End If


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
