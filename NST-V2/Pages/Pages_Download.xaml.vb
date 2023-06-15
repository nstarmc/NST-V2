Imports System.Threading
Imports Newtonsoft.Json.Linq
Imports System.IO
Imports HandyControl.Controls

Class Pages_Download

    Private Sub Page_Initialized(sender As Object, e As EventArgs)
        Dim thr1 As New Thread(AddressOf init_thr)
        thr1.Start()
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
    End Sub

    Private Sub dwpack_thr(obj As Object)
        Dim isAccessible As Boolean = TestWebsiteAccessibility(get_server_domain_https() & "/test.txt")
        If isAccessible Then
            Dim dwi As New dwinfo
            dwi.allow = False
            Dim packs_json As JObject = json_packs_nstarmc()
            Dim temp_json As JObject
            Dim vertemp_json As JObject

            For Each category In packs_json("packs")

                dw_ver.Dispatcher.Invoke(New Action(Sub()
                                                        temp_json = lc_json(category)
                                                        If dw_ver.SelectedItem.ToString = temp_json("Category").ToString Then
                                                            For Each category_ver In temp_json("Version")
                                                                vertemp_json = lc_json(category_ver)
                                                                dw_ver.Dispatcher.Invoke(New Action(Sub()
                                                                                                        If dw_ver.SelectedItem.ToString = vertemp_json("VersionName").ToString Then
                                                                                                            dwi.url = vertemp_json("Url")
                                                                                                            dwi.packid = vertemp_json("PackID")
                                                                                                        End If
                                                                                                    End Sub))
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
                    Else
                        dwi.allow = True
                    End If
                End If
            Next

            '处理下载任务
            If dwi.allow = True Then
                Growl.Info("开始处理整合包下载任务！")
            End If
        Else
            Growl.Error("网络错误！")
        End If
    End Sub
End Class
