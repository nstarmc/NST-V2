Imports System.IO
Imports System.Security.Cryptography

Module SHA_256
    Public Function VerifyFileSHA256(filePath As String, expectedHash As String) As Boolean
        ' 检查文件是否存在
        If Not File.Exists(filePath) Then
            Throw New FileNotFoundException("指定的文件不存在。", filePath)
        End If

        ' 读取文件内容，并计算 SHA-256 哈希值
        Using stream As FileStream = File.OpenRead(filePath)
            Dim sha256 As New SHA256CryptoServiceProvider()
            Dim hashBytes As Byte() = sha256.ComputeHash(stream)

            ' 将字节数组转换为十六进制字符串
            Dim actualHash As String = BitConverter.ToString(hashBytes).Replace("-", "")

            ' 比较实际的哈希值和期望的哈希值
            Return String.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase)
        End Using
    End Function
End Module
