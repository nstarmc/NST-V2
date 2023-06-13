Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Module JsonReader
    Function read_localvinfo(text1 As String)
        Dim json As JObject = CType(JsonConvert.DeserializeObject(text1), JObject)
        log_record("JsonReader Module", "获取本地整合包信息成功！" & vbCrLf & text1)
        Return json
    End Function
End Module
