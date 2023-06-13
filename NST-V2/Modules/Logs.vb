Imports System.IO
Imports System.Reflection
Imports System.Windows.Threading

Module Moudule_Logs
    Dim logwrite_timer As DispatcherTimer = New DispatcherTimer()
    Dim log_data

    '日志系统初始化
    Function log_initialize()
        Try
            '检查log文件夹是否存在，不存在则创建
            If Not Directory.Exists(Path_Root() & "\NST\Logs") Then
                Directory.CreateDirectory(Path_Root() & "\NST\Logs")
                log_record("日志Module", "log文件夹成功创建！")
            End If
            log_record("日志Module", "log文件夹检查完成！")

            '检查有无旧的log文件，对应处理
            If File.Exists(Path_Root() & "\NST\Logs\Loglog-last.log") Then
                File.Delete(Path_Root() & "\NST\Logs\Loglog-last.log")
                log_record("日志Module", "过期日志删除成功！")
            End If
            If File.Exists(Path_Root() & "\NST\Logs\Loglog-latest.log") Then
                Rename(Path_Root() & "\NST\Logs\Loglog-latest.log", Path_Root() & "\NST\Logs\Loglog-last.log")
                log_record("日志Module", "上一次的日志重命名成功！")
            End If
            File.Create(Path_Root() & "\NST\Logs\Loglog-latest.log")
            log_record("日志Module", "日志文件创建完成！")


            '启动写日志timer
            AddHandler logwrite_timer.Tick, AddressOf logwrite_timer_Tick
            logwrite_timer.Interval = New TimeSpan(0, 0, 1)
            logwrite_timer.Start()
            log_record("日志Module", "日志系统初始化成功！")
        Catch ex As Exception
            HandyControl.Controls.MessageBox.Show("日志系统初始化失败，程序无法启动！请尝试删除Log文件夹！" & vbCrLf & ex.Message， "错误-无法启动"， MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Function

    '日志输出timer
    Private Sub logwrite_timer_Tick(sender As Object, e As EventArgs)
        Try
            If Not log_data = "" Then
                File.AppendAllText(Path_Root() & "\NST\Logs\Loglog-latest.log", log_data)
                log_data = ""
            End If
        Catch ex As Exception

        End Try
    End Sub

    '日志记录，记录到临时变量
    Function log_record(logheader As String, logtext As String)
        '写日志
        'Dim log_writer As New StreamWriter(Path_Root & "\NST\Loglog-latest.log", True)
        'log_writer.WriteLine("「" & Now & "」" & logtext)
        'log_writer.Close()
        'File.AppendAllText(Path_Root & "\NST\Loglog-latest.log", "「" & Now & "」" & logtext)
        log_data = log_data & "「" & Now & "」[" & logheader & "]" & logtext & vbCrLf
    End Function
End Module
