Imports System.IO
Imports System.Threading
Imports HandyControl.Controls
Imports Newtonsoft.Json.Linq
Imports Wpf.Ui.Controls

Class MainWindow
    Dim checkupd As Boolean
    Private Sub Window_Initialized(sender As Object, e As EventArgs)
        '初始化日志模块
        log_initialize()
        '执行启动初始化线程
        Dim Initialize_thread As New Thread(AddressOf Initialize)
        Initialize_thread.Start()
        checkupd = True
        bqinfo.Text = "NSTARMC-Tools V2 Beta " & Application.ResourceAssembly.GetName().Version.ToString()
    End Sub

    Private Sub Initialize(obj As Object)
        '检查关键文件存在情况
        If Directory.Exists(Path_Root() & "\NST\") = False Then
            Try
                Directory.CreateDirectory(Path_Root() & "\NST\")
                log_record("Mainwindow", “NST文件夹创建成功！”)
            Catch ex As Exception
                log_record("Mainwindow", “必要文件检查和创建出错” & vbCrLf & ex.ToString)
            End Try
        End If
        If Directory.Exists(Path_Root() & "\MinecraftFiles\") = False Then
            Try
                Directory.CreateDirectory(Path_Root() & "\MinecraftFiles\")
                log_record("Mainwindow", “MinecraftFiles文件夹创建成功！”)
            Catch ex As Exception
                log_record("Mainwindow", “必要文件检查和创建出错” & vbCrLf & ex.ToString)
            End Try
        End If
        log_record("Mainwindow", “必要文件检查完成！”)



    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        ' 分配修改控件的方法给委托变量


    End Sub

    Public Event MyEvent As EventHandler
    Public Sub updEvent()
        RootNavigation.Navigate(6)
        RootNavigation.Visibility = Visibility.Collapsed
    End Sub
End Class
