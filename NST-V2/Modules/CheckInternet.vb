Imports System.Net
Imports System.Net.NetworkInformation

Module CheckInternet
    'ping
    Function CheckDomainConnection(domainName As String) As Boolean
        If Not String.IsNullOrWhiteSpace(domainName) Then
            Dim pingSender As New Ping()
            Dim reply As PingReply = pingSender.Send(domainName)

            If reply.Status = IPStatus.Success Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If
    End Function

    'access
    Public Function TestWebsiteAccessibility(ByVal url As String) As Boolean
        Try
            Dim request As HttpWebRequest = DirectCast(WebRequest.Create(url), HttpWebRequest)
            request.Timeout = 5000 '设置请求超时时间为5秒

            Using response As HttpWebResponse = DirectCast(request.GetResponse(), HttpWebResponse)
                '检查返回的状态码
                If response.StatusCode = HttpStatusCode.OK Then
                    '网站可访问
                    Return True
                Else
                    '网站不可访问
                    Return False
                End If
            End Using
        Catch ex As Exception
            '发生异常，网站不可访问
            Return False
        End Try
    End Function
End Module
