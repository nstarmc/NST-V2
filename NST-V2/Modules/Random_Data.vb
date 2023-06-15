Imports System.Text

Module Random_Data
    Function GenerateRandomString(length As Integer) As String
        Const chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
        Dim sb As New StringBuilder()
        Dim random As New Random()

        For i As Integer = 0 To length - 1
            Dim index As Integer = random.Next(0, chars.Length)
            sb.Append(chars(index))
        Next

        Return sb.ToString()
    End Function
End Module
