Imports System.Reflection
Imports System.IO

Module Moudule_Paths
    Function Path_Root() As String
        Dim rootpath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
        Return rootpath
    End Function
End Module
