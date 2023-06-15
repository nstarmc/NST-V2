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

        '获取公告文本
        Dim isAccessible As Boolean = TestWebsiteAccessibility(get_server_domain_https() & "/test.txt")
        If isAccessible Then
            Dim json As JObject = json_main()
            Growl.Success("您与NST-V2服务器连接正常！")

            text_notice.Dispatcher.Invoke(New Action(Sub()
                                                         text_notice.Text = json("Notice").ToString
                                                     End Sub))
            '一言
            '一言获取
            Try
                Dim json_onesay As JObject = json
                Dim onesay_list(888)
                Dim i = 0
                For Each x3 In json_onesay("onesay")
                    onesay_list(i) = x3
                    i = i + 1
                Next
                text_onesay.Dispatcher.Invoke(New Action(Sub()
                                                             Dim MyValue As Integer
                                                             Randomize()
                                                             MyValue = CInt(Int((i * Rnd()) + 0))
                                                             text_onesay.Text = onesay_list(MyValue)
                                                             log_record("Home", "一言加载成功：" & onesay_list(MyValue))
                                                         End Sub))
            Catch ex As Exception
                text_onesay.Dispatcher.Invoke(New Action(Sub()
                                                             text_onesay.Text = "草木扎根于无垠之中，叶梢朝向繁星生长。"
                                                             log_record("text_onesay", "一言加载失败，加载默认内容。" & vbCrLf & ex.ToString)
                                                         End Sub))
            End Try
        Else
            Growl.Error("无法连接到NST-V2服务器，联网功能将无法使用！")
            text_notice.Dispatcher.Invoke(New Action(Sub()
                                                         text_notice.Text = "欢迎使用NST-V2！"
                                                     End Sub))
        End If





        '获取MC版本信息
        Dim isAccessible2 As Boolean = TestWebsiteAccessibility("http://launchermeta.mojang.com/mc/game/version_manifest_v2.json")
        If isAccessible2 Then
            Dim json As JObject = json_mcv()
            text_mcv.Dispatcher.Invoke(New Action(Sub()
                                                      text_mcv.Text = "最新版本：" & json("latest")("release").ToString & vbCrLf & "最新快照版本：" & json("latest")("snapshot").ToString
                                                  End Sub))
        Else
            text_notice.Dispatcher.Invoke(New Action(Sub()
                                                         text_mcv.Text = "我不知道啊！" & vbCrLf & "无法连接到Launchermeta服务器······"
                                                     End Sub))
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

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        '启动启动器，启用线程来处理
        Dim thr1 As New Thread(AddressOf launch)
        thr1.Start()

    End Sub

    Private Sub launch(obj As Object)
        For Each dirlist In Directory.GetDirectories(Path_Root() & "\MinecraftFiles\")
            If File.Exists(dirlist & "\MCVersionInfo.json") Then
                '检测到文件夹内为符合格式的整合包，加载
                Dim info_reader As String = File.ReadAllText(dirlist & "\MCVersionInfo.json")
                Dim localinfo As JObject = read_localvinfo(info_reader)
                mcinfo.Dispatcher.Invoke(New Action(Sub()
                                                        If mclist.SelectedItem.ToString = localinfo("MCVersion").ToString & "-" & localinfo("ModLoader").ToString & "[" & localinfo("ShaderLoader").ToString & "]~" & localinfo("PackID").ToString Then
                                                            '启动启动器
                                                            Dim process As New Process()
                                                            process.StartInfo.FileName = dirlist & "\" & mclacuncher.SelectedItem.ToString & ".exe"
                                                            process.StartInfo.UseShellExecute = False
                                                            process.StartInfo.RedirectStandardOutput = True
                                                            process.Start()
                                                            process.Close()
                                                            log_record("Launch", "启动启动器：" & dirlist & "\" & mclacuncher.SelectedItem.ToString & ".exe")
                                                            Growl.Success("为您启动启动" & mclist.SelectedItem.ToString & “整合包成功！”)
                                                        End If
                                                    End Sub))
            End If
        Next

    End Sub
End Class
