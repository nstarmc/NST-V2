Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports HandyControl.Controls
Imports Newtonsoft.Json.Linq
Imports Wpf.Ui.Controls

Class Pages_Home
    Private Sub Page_Initialized(sender As Object, e As EventArgs)
        '页面初始化
        Dim thr1 As New Thread(AddressOf initialize_thr)
        thr1.Start()
    End Sub

    Private Sub initialize_thr(obj As Object)
        log_record("Home", "开始执行主页初始化！")
        '遍历读取整合包
        mclist.Dispatcher.Invoke(New Action(Sub()
                                                mclist.Items.Clear()
                                            End Sub))
        For Each dirlist In Directory.GetDirectories(Path_Root() & "\MinecraftFiles\")
            If File.Exists(dirlist & "\MCVersionInfo.json") Then
                '检测到文件夹内为符合格式的整合包，加载
                log_record("Home", "检测到整合包！文件夹：" & dirlist)
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
                                                    log_record("Home", "没有检测到任何整合包！")
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
                mcinfo.Dispatcher.Invoke(New Action(Sub()
                                                        If mclist.SelectedItem.ToString = localinfo("MCVersion").ToString & "-" & localinfo("ModLoader").ToString & "[" & localinfo("ShaderLoader").ToString & "]~" & localinfo("PackID").ToString Then
                                                            mcinfo.Text = "Minecraft版本：" & localinfo("MCVersion").ToString & vbCrLf & "模组加载器：" & localinfo("ModLoader").ToString & vbCrLf & "光影加载器：" & localinfo("ShaderLoader").ToString & vbCrLf & "整合包唯一ID：" & localinfo("PackID").ToString & vbCrLf & "更新时间：" & ConvertStringToDateTime(localinfo("ReleaseTimespan").ToString).ToString
                                                            log_record("Home", "读取整合包信息：" & dirlist)
                                                            '读取启动器list
                                                            mclacuncher.Items.Clear()

                                                            For Each file As String In Directory.GetFiles(dirlist, "*.exe")
                                                                mclacuncher.Items.Add(Replace(Replace(file, dirlist & "\", ""), ".exe", ""))
                                                            Next
                                                            mclacuncher.SelectedIndex = 0
                                                        End If
                                                    End Sub))
            End If
        Next
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub
End Class
