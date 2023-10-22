Imports System.IO

Module FileCopy
    '文件夹复制
    Public Function CopyFolder(ByVal sourceFolder As String, ByVal destFolder As String) As Object
        Try
            If Not Directory.Exists(destFolder) Then
                Directory.CreateDirectory(destFolder)
            End If

            Dim files As String() = Directory.GetFiles(sourceFolder)

            For Each file As String In files
                Dim name As String = Path.GetFileName(file)
                Dim dest As String = Path.Combine(destFolder, name)
                IO.File.Copy(file, dest, True)
            Next

            Dim folders As String() = Directory.GetDirectories(sourceFolder)

            For Each folder As String In folders
                Dim name As String = Path.GetFileName(folder)
                Dim dest As String = Path.Combine(destFolder, name)
                CopyFolder(folder, dest)
            Next

            Return True
        Catch ex As Exception
            log_record("文件夹复制", "出错：" & ex.Message)
        End Try
    End Function

    Public Sub CopyFile(sourcePath As String, destinationPath As String)
        ' 检查源文件是否存在
        If Not File.Exists(sourcePath) Then
            log_record("文件夹复制", "找不到指定的源文件。" & sourcePath)
        End If

        ' 复制文件
        Try
            File.Copy(sourcePath, destinationPath, True)
            log_record("文件夹复制", "文件复制成功。")
        Catch ex As Exception
            log_record("文件夹复制", "文件复制失败: " & ex.Message)
        End Try
    End Sub
End Module
