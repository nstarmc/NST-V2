Imports System.Threading
Imports Newtonsoft.Json.Linq
Imports System.IO
Imports HandyControl.Controls
Imports Downloader
Imports System.Reflection
Imports System.IO.Compression
Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile

Class Pages_Download
    Dim dwi As New dwinfo

    Private Sub Page_Initialized(sender As Object, e As EventArgs)
        Dim thr1 As New Thread(AddressOf init_thr)
        thr1.Start()
        bt_dl.IsEnabled = False
        bt_dl.Content = "数据加载中···"
    End Sub

    Private Sub init_thr(obj As Object)
        Dim isAccessible As Boolean = TestWebsiteAccessibility(get_server_domain_https() & "/test.txt")
        If isAccessible Then
            Dim packs_json As JObject = json_packs_nstarmc()
            Dim temp_json As JObject
            dw_category.Dispatcher.Invoke(New Action(Sub()
                                                         dw_category.Items.Clear()
                                                     End Sub))
            For Each category In packs_json("packs")
                dw_category.Dispatcher.Invoke(New Action(Sub()
                                                             temp_json = lc_json(category)
                                                             dw_category.Items.Add(temp_json("Category").ToString)
                                                         End Sub))
            Next
            dw_category.Dispatcher.Invoke(New Action(Sub()
                                                         If Not dw_category.Items.Count = 0 Then
                                                             dw_category.SelectedIndex = 0
                                                         End If
                                                     End Sub))
        Else
            Growl.Error("网络错误！")
        End If
    End Sub

    Private Sub dw_category_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles dw_category.SelectionChanged
        Dim thr1 As New Thread(AddressOf verlistupd_thr)
        thr1.Start()
        bt_dl.Dispatcher.Invoke(New Action(Sub()
                                               bt_dl.IsEnabled = False
                                               bt_dl.Content = "数据加载中···"
                                           End Sub))
    End Sub

    Private Sub verlistupd_thr(obj As Object)
        Dim isAccessible As Boolean = TestWebsiteAccessibility(get_server_domain_https() & "/test.txt")
        If isAccessible Then
            Dim packs_json As JObject = json_packs_nstarmc()
            Dim temp_json As JObject
            Dim vertemp_json As JObject
            dw_ver.Dispatcher.Invoke(New Action(Sub()
                                                    dw_ver.Items.Clear()
                                                End Sub))
            For Each category In packs_json("packs")
                dw_category.Dispatcher.Invoke(New Action(Sub()
                                                             temp_json = lc_json(category)
                                                             If dw_category.SelectedItem.ToString = temp_json("Category").ToString Then
                                                                 For Each category_ver In temp_json("Version")
                                                                     vertemp_json = lc_json(category_ver)
                                                                     dw_ver.Dispatcher.Invoke(New Action(Sub()
                                                                                                             dw_ver.Items.Add(vertemp_json("VersionName").ToString)
                                                                                                         End Sub))
                                                                 Next
                                                             End If
                                                         End Sub))
            Next
            dw_ver.Dispatcher.Invoke(New Action(Sub()
                                                    If Not dw_ver.Items.Count = 0 Then
                                                        dw_ver.SelectedIndex = 0
                                                    End If
                                                End Sub))
            bt_dl.Dispatcher.Invoke(New Action(Sub()
                                                   bt_dl.IsEnabled = True
                                                   bt_dl.Content = "开始下载"
                                               End Sub))
        Else
            Growl.Error("网络错误！")
        End If
    End Sub
    Class dwinfo
        Public packid As String
        Public url As String
        Public allow As Boolean
    End Class


    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim thr1 As New Thread(AddressOf dwpack_thr)
        thr1.Start()
        bt_dl.IsEnabled = False
    End Sub

    Private Sub dwpack_thr(obj As Object)
        dwi.allow = True
        Dim isAccessible As Boolean = TestWebsiteAccessibility(get_server_domain_https() & "/test.txt")
        If isAccessible Then
            Dim packs_json As JObject = json_packs_nstarmc()
            Dim temp_json As JObject
            Dim vertemp_json As JObject

            For Each category In packs_json("packs")

                dw_ver.Dispatcher.Invoke(New Action(Sub()
                                                        temp_json = lc_json(category)
                                                        If dw_category.SelectedItem.ToString = temp_json("Category").ToString Then
                                                            For Each category_ver In temp_json("Version")
                                                                vertemp_json = lc_json(category_ver)
                                                                If dw_ver.SelectedItem.ToString = vertemp_json("VersionName").ToString Then
                                                                    dwi.url = vertemp_json("Url")
                                                                    dwi.packid = vertemp_json("PackID")
                                                                End If
                                                            Next
                                                        Else
                                                        End If
                                                    End Sub))
            Next

            '检查有无和本地重复ID
            For Each dirlist In Directory.GetDirectories(Path_Root() & "\MinecraftFiles\")
                If File.Exists(dirlist & "\MCVersionInfo.json") Then
                    '检测到文件夹内为符合格式的整合包，加载
                    log_record("JavaDW", "检测到整合包！文件夹：" & dirlist)
                    Dim info_reader As String = File.ReadAllText(dirlist & "\MCVersionInfo.json")
                    Dim localinfo As JObject = read_localvinfo(info_reader)
                    If localinfo("PackID").ToString = dwi.packid Then
                        dwi.allow = False
                        Growl.Error("本地有重复整合包，" & vbCrLf & "下载已取消！" & vbCrLf & "若需要更新整合包，" & vbCrLf & "请前往更新整合包选项卡！")
                        bt_dl.Dispatcher.Invoke(New Action(Sub()
                                                               bt_dl.IsEnabled = True
                                                           End Sub))
                    Else

                    End If
                End If
            Next
            Dim thr2 As New Thread(AddressOf dw_on_thr)
            thr2.Start()
        Else
            bt_dl.Dispatcher.Invoke(New Action(Sub()
                                                   bt_dl.IsEnabled = True
                                               End Sub))
            Growl.Error("无法连接到下载资源服务器！")
        End If
    End Sub

    Private Async Sub dw_on_thr(obj As Object)
        '处理下载任务
        If dwi.allow = True Then
            Growl.Info("开始处理整合包下载任务！")
            Try
                File.Delete(Path_Root() & "\MinecraftFiles\mcpack_nstv2dw.zip")
                Directory.Delete(Path_Root() & "\MinecraftFiles\unzip_temp_nstarmc_tools_v2\", True)
            Catch ex As Exception

            End Try
            '开始下载
            text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                         pg_dw.Maximum = 100
                                                     End Sub))

            Dim downloader = New DownloadService(downloader_config_read)
            AddHandler downloader.DownloadProgressChanged, AddressOf OnDownloadProgressChanged
            Await downloader.DownloadFileTaskAsync(dwi.url, Path_Root() & "\MinecraftFiles\mcpack_nstv2dw.zip")

            '——————解压——————
            Text.Encoding.RegisterProvider(Text.CodePagesEncodingProvider.Instance)
            Dim encode As Text.Encoding = Text.Encoding.GetEncoding("GB2312")
            ZipStrings.CodePage = encode.CodePage
            Dim zf As ZipFile = Nothing
            Try
                Dim fs As FileStream = File.OpenRead(Path_Root() & "\MinecraftFiles\mcpack_nstv2dw.zip")
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
                    Directory.CreateDirectory(Path_Root() & "\MinecraftFiles\unzip_temp_nstarmc_tools_v2\")
                    Dim fullZipToPath As [String] = Path.Combine(Path_Root() & "\MinecraftFiles\unzip_temp_nstarmc_tools_v2\", entryFileName）
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
                    log_record("页面-游戏更新线程", "文件解压缩完成。")
                    Try
                        File.Delete(Path_Root() & "\MinecraftFiles\mcpack_nstv2dw.zip")

                    Catch ex As Exception

                    End Try

                    log_record("页面-游戏更新线程", "删除更新包完成。")
                End If
            End Try
            '——————解压——————
            '移动文件夹
            Dim i = 0
            Dim new_name
            For Each x In Directory.GetDirectories(Path_Root() & "\MinecraftFiles\unzip_temp_nstarmc_tools_v2\MinecraftFiles\")
                i = i + 1
                new_name = Path_Root() & "\MinecraftFiles\" & dwi.packid & "_" & Now.Year & Now.Month & Now.Day & "_" & i & "_" & GenerateRandomString(8)
                Directory.Move(x, new_name)
            Next
            Directory.Delete(Path_Root() & "\MinecraftFiles\unzip_temp_nstarmc_tools_v2\", True)
            Growl.Success("done!")
            bt_dl.Dispatcher.Invoke(New Action(Sub()
                                                   bt_dl.IsEnabled = True
                                               End Sub))
        Else
            bt_dl.Dispatcher.Invoke(New Action(Sub()
                                                   bt_dl.IsEnabled = True
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
