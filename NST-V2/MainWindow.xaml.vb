Imports System.IO
Imports System.Threading

Class MainWindow
    Private Sub Window_Initialized(sender As Object, e As EventArgs)
        '初始化日志模块
        log_initialize()
        '执行启动初始化线程
        Dim Initialize_thread As New Thread(AddressOf Initialize)
        Initialize_thread.Start()
    End Sub

    Private Sub Initialize(obj As Object)
        '检查关键文件存在情况
        If Directory.Exists(Path_Root() & "\NST\") = False Then
            Try
                Directory.CreateDirectory(Path_Root() & "\NST\")
            Catch ex As Exception

            End Try
        End If
        log_record("Mainwindow", “检查ok”)
    End Sub
End Class
