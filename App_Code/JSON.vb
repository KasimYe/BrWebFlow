Imports Microsoft.VisualBasic

Imports System.Collections.Generic
Imports System.Text
Imports System.Collections
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json.Converters

Namespace MiniJson
    Public Class JSON
        Public Shared DateTimeFormat As String = "yyyy'-'MM'-'dd'T'HH':'mm':'ss"

        Public Shared Function Encode(ByVal o As Object) As String
            If o Is Nothing OrElse o.ToString() = "null" Then
                Return Nothing
            End If
            If o IsNot Nothing AndAlso (o.GetType Is GetType(String) OrElse o.GetType Is GetType(String)) Then
                Return o.ToString()
            End If
            Dim dt As New IsoDateTimeConverter()
            dt.DateTimeFormat = DateTimeFormat
            Return JsonConvert.SerializeObject(o, dt)
        End Function

        Public Shared Function Decode(ByVal json As String) As Object
            If String.IsNullOrEmpty(json) Then
                Return ""
            End If
            Dim o As Object = JsonConvert.DeserializeObject(json)
            If o.GetType Is GetType(String) Then
                o = JsonConvert.DeserializeObject(o.ToString())
            End If
            Dim v As Object = toObject(o)
            Return v
        End Function

        Public Shared Function Decode(ByVal json As String, ByVal type As Type) As Object
            Return JsonConvert.DeserializeObject(json, type)
        End Function

        Private Shared Function toObject(ByVal o As Object) As Object
            If o Is Nothing Then
                Return Nothing
            End If
            If o.GetType Is GetType(String) Then
                '判断是否符合2010-09-02T10:00:00的格式    
                Dim s As String = o.ToString()
                If s.Length = 19 AndAlso s(10) = "T"c AndAlso s(4) = "-"c AndAlso s(13) = ":"c Then
                    o = System.Convert.ToDateTime(o)
                End If
            ElseIf TypeOf o Is JObject Then
                Dim jo As JObject = TryCast(o, JObject)
                Dim h As New Hashtable()
                For Each entry As KeyValuePair(Of String, JToken) In jo
                    h(entry.Key) = toObject(entry.Value)
                Next
                o = h
            ElseIf TypeOf o Is IList Then
                Dim list As New ArrayList()
                list.AddRange(TryCast(o, IList))
                Dim i As Integer = 0, l As Integer = list.Count
                While i < l
                    list(i) = toObject(list(i))
                    i += 1
                End While
                o = list
            ElseIf GetType(JValue) Is o.GetType Then
                Dim v As JValue = DirectCast(o, JValue)
                o = toObject(v.Value)
            Else
            End If
            Return o
        End Function
    End Class
End Namespace

