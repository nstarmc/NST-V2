Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Downloader
Imports HandyControl.Controls
Imports Microsoft.WindowsAPICodePack.Dialogs
Imports Newtonsoft.Json
Imports ICSharpCode.SharpZipLib.Zip
Imports Newtonsoft.Json.Linq

Class Pages_Pack
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Dim dialog = New CommonOpenFileDialog()
        dialog.Title = "请选择你要打包的整合包的所在文件夹"
        dialog.IsFolderPicker = True
        If dialog.ShowDialog() = CommonFileDialogResult.Ok Then
            pathGame.Text = dialog.FileName
        End If
    End Sub

    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        Dim dialog = New CommonOpenFileDialog()
        dialog.Title = "请选择压缩包输出的文件夹"
        dialog.IsFolderPicker = True
        If dialog.ShowDialog() = CommonFileDialogResult.Ok Then
            pathOutput.Text = dialog.FileName
        End If
    End Sub
    Class PackInfo
        Public verMain As Integer
        Public verMedium As Integer
        Public verLast As Integer
        Public pathGame As String
        Public pathOutput As String
        Public modName As String
        Public modID As Integer
        Public shaderName As String
        Public shaderID As Integer
        Public clearAll As Boolean
        Public clearConfig As Boolean
        Public clearModConfig As Boolean
        Public clearSaves As Boolean
        Public idMode As Boolean
        Public customID As Integer
    End Class
    Dim packinfo_use As New PackInfo
    Private Sub Button_Click_2(sender As Object, e As RoutedEventArgs)
        logBox.Items.Clear()
        '定义线程
        Dim thr As New Thread(AddressOf packzip)
        '执行参数检查
        If Not Directory.Exists(pathGame.Text) Then
            Growl.Error("整合包目录不存在！")
            logBox.Items.Add("出错：整合包目录不存在！")
        Else
            '执行一次创建文件夹，防止输出目录不存在
            Try
                Directory.CreateDirectory(pathOutput.Text)
            Catch ex As Exception
            End Try
            If Directory.Exists(pathOutput.Text) Then
                If Not pathOutput.Text = pathGame.Text Then
                    If packidCustom.IsChecked = True Then '自定义id时检查范围
                        If customID.Text = "" OrElse verMedium.Text = "" OrElse verLast.Text = "" Then
                            Growl.Error("有参数是空值，请填写！")
                            logBox.Items.Add("出错：有参数是空值，请填写！")
                        Else
                            If Int(customID.Text) >= 2000000000 And Int(customID.Text) <= 9999999999 Then
                                packinfo_use.verMain = verMain.Text
                                packinfo_use.verMedium = verMedium.Text
                                packinfo_use.verLast = verLast.Text
                                packinfo_use.pathGame = pathGame.Text
                                packinfo_use.pathOutput = pathOutput.Text
                                packinfo_use.modName = selMod.Text
                                packinfo_use.modID = selMod.SelectedIndex
                                packinfo_use.shaderName = selShader.Text
                                packinfo_use.shaderID = selShader.SelectedIndex
                                packinfo_use.clearAll = checkAll.IsChecked
                                packinfo_use.clearConfig = checkGameConfig.IsChecked
                                packinfo_use.clearModConfig = checkModConfig.IsChecked
                                packinfo_use.clearSaves = checkSaves.IsChecked
                                packinfo_use.idMode = packidNSTARMC.IsChecked
                                packinfo_use.customID = customID.Text
                                thr.Start()
                            Else
                                Growl.Error("自定义整合包ID范围错误！")
                                logBox.Items.Add("出错：自定义整合包ID范围错误！")
                            End If
                        End If

                    Else 'nstarmc包id
                        If verMedium.Text = "" OrElse verLast.Text = "" Then
                            Growl.Error("有参数是空值，请填写！")
                            logBox.Items.Add("出错：有参数是空值，请填写！")
                        Else

                            packinfo_use.verMain = verMain.Text
                            packinfo_use.verMedium = verMedium.Text
                            packinfo_use.verLast = verLast.Text
                            packinfo_use.pathGame = pathGame.Text
                            packinfo_use.pathOutput = pathOutput.Text
                            packinfo_use.modName = selMod.Text
                            packinfo_use.modID = selMod.SelectedIndex
                            packinfo_use.shaderName = selShader.Text
                            packinfo_use.shaderID = selShader.SelectedIndex
                            packinfo_use.clearAll = checkAll.IsChecked
                            packinfo_use.clearConfig = checkGameConfig.IsChecked
                            packinfo_use.clearModConfig = checkModConfig.IsChecked
                            packinfo_use.clearSaves = checkSaves.IsChecked
                            packinfo_use.idMode = packidNSTARMC.IsChecked
                            thr.Start()
                        End If

                    End If
                Else
                    Growl.Error("输出文件夹不可以和整合包所在位置相同！")
                    logBox.Items.Add("出错：输出文件夹不可以和整合包所在位置相同！")
                End If
            Else
                Growl.Error("无法创建输出文件夹！")
                logBox.Items.Add("出错：无法创建输出文件夹！")
            End If
        End If
    End Sub

    Private Sub addlog(log As String)
        logBox.Dispatcher.Invoke(New Action(Sub()
                                                logBox.Items.Add(log)
                                                text_dwinfo.Text = log
                                            End Sub))
    End Sub
    Private Async Sub packzip(obj As Object)
        pgr.Dispatcher.Invoke(New Action(Sub()
                                             pgr.Visibility = Visibility.Visible
                                         End Sub))
        Try
            If Directory.Exists(Path_Root() & "\Working For Packing") Then
                Directory.Delete(Path_Root() & "\Working For Packing", True)
            End If
            '创建工作文件夹
            Directory.CreateDirectory(Path_Root() & "\Working For Packing")
            addlog("创建工作文件夹成功")
            '定义json
            Dim json_m As JObject = json_main()
            '检测本地有无NST程序
            If File.Exists(Path_Root() & "\NST\NST-V2.exe") Then
                '有的话，校验是不是最新的
                If VerifyFileSHA256(Path_Root() & "\NST\NST-V2.exe", json_m("SHA-256").ToString) = False Then
                    '不是最新就下载
                    addlog("NST-V2非最新，开始下载NST-V2")
                    Dim downloader = New DownloadService(downloader_config_read)
                    AddHandler downloader.DownloadProgressChanged, AddressOf OnDownloadProgressChanged
                    Await downloader.DownloadFileTaskAsync(json_m("DL").ToString, Path_Root() & "\NST\NST-V2.exe")
                    text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                                 text_dwinfo.Text = "NST-V2下载完成！"
                                                             End Sub))
                End If
            Else
                '没有就下载
                addlog("开始下载NST-V2")
                Dim downloader = New DownloadService(downloader_config_read)
                AddHandler downloader.DownloadProgressChanged, AddressOf OnDownloadProgressChanged
                Await downloader.DownloadFileTaskAsync(json_m("DL").ToString, Path_Root() & "\NST\NST-V2.exe")
                text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                             text_dwinfo.Text = "NST-V2下载完成！"
                                                         End Sub))
            End If
            '复制nst到工作目录
            addlog("开始复制NST")
            File.Copy(Path_Root() & "\NST\NST-V2.exe", Path_Root() & "\Working For Packing\NST-V2.exe")
            '创建提示的txt
            addlog("创建提示文本")
            Using writer As New StreamWriter(Path_Root() & "\Working For Packing\" & json_m("Txt_Title").ToString & ".txt")
                writer.Write(json_m("Txt_Content").ToString)
            End Using
            '创建游戏版本目录
            Directory.CreateDirectory(Path_Root() & "\Working For Packing\MinecraftFiles\")
            '复制文件夹
            addlog("开始复制游戏包")
            CopyFolder(packinfo_use.pathGame, Path_Root() & "\Working For Packing\MinecraftFiles\" &
                                      packinfo_use.verMain & "." & packinfo_use.verMedium & "." & packinfo_use.verLast & "-" &
                                      packinfo_use.modName & "-" & packinfo_use.shaderName)
            '输出json
            addlog("输出游戏信息Json")
            ' 创建一个GameData对象，并设置相关属性
            Dim gameData As New GameData()
            gameData.MCVersion = packinfo_use.verMain & "." & packinfo_use.verMedium & "." & packinfo_use.verLast
            gameData.ModLoader = packinfo_use.modName
            gameData.ShaderLoader = packinfo_use.shaderName
            If packinfo_use.idMode = True Then
                gameData.PackID = 10 & packinfo_use.verMain & packinfo_use.verMedium & packinfo_use.verLast & packinfo_use.modID & packinfo_use.shaderID
            Else
                gameData.PackID = packinfo_use.customID
            End If
            gameData.ReleaseTimespan = DateTime.UtcNow.AddHours(0).Subtract(New DateTime(1970, 1, 1)).TotalSeconds
            If packinfo_use.verMedium <= 16 Then
                gameData.JavaVersion = 8
            Else
                gameData.JavaVersion = 17
            End If
            ' 将对象转换为JSON格式的字符串
            Dim jsonString As String = JsonConvert.SerializeObject(gameData)
            ' 将JSON字符串写入文件
            File.WriteAllText(Path_Root() & "\Working For Packing\MinecraftFiles\" &
                                      packinfo_use.verMain & "." & packinfo_use.verMedium & "." & packinfo_use.verLast & "-" &
                                      packinfo_use.modName & "-" & packinfo_use.shaderName & "\MCVersionInfo.json", jsonString)
            '清理无用文件
            Dim gamepath = Path_Root() & "\Working For Packing\MinecraftFiles\" &
                                      packinfo_use.verMain & "." & packinfo_use.verMedium & "." & packinfo_use.verLast & "-" &
                                      packinfo_use.modName & "-" & packinfo_use.shaderName & "\.minecraft"

            addlog("开始清理无用文件")
            '干净模式
            For Each file_temp In Directory.GetFiles(gamepath)

                If Replace(file_temp, gamepath & "\", "") = "options.txt" Then
                    If packinfo_use.clearConfig = False Then
                    Else
                        File.Delete(file_temp)
                    End If
                Else
                    File.Delete(file_temp)
                End If

            Next
            For Each dir_temp In Directory.GetDirectories(gamepath)
                Dim temppath = Replace(dir_temp, gamepath & "\", "")
                If temppath = "versions" OrElse temppath = "assets" OrElse temppath = "shaderpacks" OrElse temppath = "mods" OrElse temppath = "libraries" OrElse temppath = "resourcepacks" Then
                Else

                    If temppath = "config" Then
                        If packinfo_use.clearModConfig = False Then
                        Else
                            Directory.Delete(dir_temp, True)
                        End If
                    ElseIf temppath = "saves" Then
                        If packinfo_use.clearSaves = False Then
                        Else
                            Directory.Delete(dir_temp, True)
                        End If
                    Else
                        Directory.Delete(dir_temp, True)
                    End If

                End If
            Next
            If Directory.Exists(Path_Root() & "\Working For Packing\MinecraftFiles\" &
                                      packinfo_use.verMain & "." & packinfo_use.verMedium & "." & packinfo_use.verLast & "-" &
                                      packinfo_use.modName & "-" & packinfo_use.shaderName & "\PCL\") = True Then
                For i = 1 To 5
                    Try
                        File.Delete(Path_Root() & "\Working For Packing\MinecraftFiles\" &
                                      packinfo_use.verMain & "." & packinfo_use.verMedium & "." & packinfo_use.verLast & "-" &
                                      packinfo_use.modName & "-" & packinfo_use.shaderName & "\PCL\Log" & i & ".txt")
                    Catch ex As Exception

                    End Try
                Next
                Try
                    File.Delete(Path_Root() & "\Working For Packing\MinecraftFiles\" &
                                  packinfo_use.verMain & "." & packinfo_use.verMedium & "." & packinfo_use.verLast & "-" &
                                  packinfo_use.modName & "-" & packinfo_use.shaderName & "\PCL\LatestLaunch.bat")
                Catch ex As Exception

                End Try
            End If

            '——————打包zip
            addlog("开始压缩")
            ' 发布压缩信息
            text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                         text_dwinfo.Text = ("正在压缩···")
                                                     End Sub))
            Dim directoryPath As String = Path_Root() & "\Working For Packing\"
            Dim archivePath As String = packinfo_use.pathOutput & "\" &
                                      packinfo_use.verMain & "." & packinfo_use.verMedium & "." & packinfo_use.verLast & "-" &
                                      packinfo_use.modName & "-" & packinfo_use.shaderName & "-" & Now.Year & Now.Month & Now.Day & ".zip"
            ' 创建一个新的文件流用于写入压缩包文件
            Using zipStream As New ZipOutputStream(File.Create(archivePath))
                zipStream.SetLevel(9) ' 设置压缩级别，0-9，9表示最高压缩率
                Dim i = 0
                ' 遍历源目录下的所有文件和子目录
                For Each filePath As String In Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                    i = i + 1
                    Dim entryName As String = Path.GetRelativePath(directoryPath, filePath)

                    ' 创建一个ZipEntry对象表示要添加到压缩包的文件或目录
                    Dim newEntry As New ZipEntry(entryName)
                    newEntry.DateTime = DateTime.Now
                    newEntry.Size = New FileInfo(filePath).Length

                    ' 将ZipEntry对象添加到压缩流中
                    zipStream.PutNextEntry(newEntry)

                    ' 读取文件内容并写入压缩流
                    Using fileStream As FileStream = File.OpenRead(filePath)
                        Dim buffer(4096) As Byte
                        Dim bytesRead As Integer = fileStream.Read(buffer, 0, buffer.Length)
                        While bytesRead > 0
                            zipStream.Write(buffer, 0, bytesRead)
                            bytesRead = fileStream.Read(buffer, 0, buffer.Length)
                        End While
                    End Using


                Next

                zipStream.Finish()
            End Using

            addlog("打包完成！")
            Growl.Info("整合包打包完成！")
            text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                         text_dwinfo.Text = ("Done!")
                                                     End Sub))
            pgr.Dispatcher.Invoke(New Action(Sub()
                                                 pgr.Visibility = Visibility.Collapsed
                                             End Sub))
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

    End Sub

    Public Class GameData
        Public Property MCVersion As String
        Public Property ModLoader As String
        Public Property ShaderLoader As String
        Public Property PackID As Integer
        Public Property ReleaseTimespan As Long
        Public Property JavaVersion As Integer
    End Class
    Private Sub OnDownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs)
        text_dwinfo.Dispatcher.Invoke(New Action(Sub()
                                                     text_dwinfo.Text = "文件总大小：" & Math.Round(e.TotalBytesToReceive / 1024 / 1024, 2) & "MB" & vbCrLf &
                                                 "文件已下载：" & Math.Round(e.ReceivedBytesSize / 1024 / 1024, 2) & "MB" & vbCrLf &
                                                 "当前速度：" & Math.Round(e.BytesPerSecondSpeed / 1024 / 1024, 2) & "MB/S" & vbCrLf &
                                                 "平均速度：" & Math.Round(e.AverageBytesPerSecondSpeed / 1024 / 1024, 2) & "MB/S"
                                                 End Sub))
    End Sub
    Private Sub customID_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles customID.PreviewTextInput
        e.Handled = New Regex("[^0-9]+").IsMatch(e.Text)
    End Sub
End Class
