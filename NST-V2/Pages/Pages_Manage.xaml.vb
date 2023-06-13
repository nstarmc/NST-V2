Imports System.IO
Imports System.Threading
Imports HandyControl.Controls
Imports Newtonsoft.Json.Linq

Class Pages_Manage
    Private Sub Page_Initialized(sender As Object, e As EventArgs)
        '页面初始化
        Dim thr1 As New Thread(AddressOf initialize_thr)
        thr1.Start()
    End Sub

    Private Sub initialize_thr(obj As Object)
        log_record("Manage", "开始执行主页初始化！")
        '遍历读取整合包
        mclist.Dispatcher.Invoke(New Action(Sub()
                                                mclist.Items.Clear()
                                            End Sub))
        For Each dirlist In Directory.GetDirectories(Path_Root() & "\MinecraftFiles\")
            If File.Exists(dirlist & "\MCVersionInfo.json") Then
                '检测到文件夹内为符合格式的整合包，加载
                log_record("Manage", "检测到整合包！文件夹：" & dirlist)
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
                                                    log_record("Manage", "没有检测到任何整合包！")
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
End Class
