Imports System
Module TimeSpanConvert



    Function ConvertStringToDateTime(ByVal timestamp As String) As Date
        Dim epoch As New Date(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        Dim seconds As Long
        If Long.TryParse(timestamp, seconds) Then
            Dim dateTime As Date = epoch.AddSeconds(seconds).AddHours(8)
            Return dateTime
        Else
            Throw New ArgumentException("Invalid timestamp format.")
        End If
    End Function
End Module
