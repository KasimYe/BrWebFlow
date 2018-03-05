Imports System.Collections
Imports System.Collections.Generic
Imports System.Reflection
Imports System
Imports System.Data
Imports System.Data.Common

Partial Class Service
    Inherits System.Web.UI.Page

    Private pageSize As Integer
    Private connBakSys As String = "Data Source=192.168.102.10;database=bak_sys;uid=sa;pwd=abc123"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim methodName As String = Request.Params("method")
        Dim type As Type = Me.GetType
        Dim method As MethodInfo = type.GetMethod(methodName)
        If method Is Nothing Then
            Throw New Exception("method is null")
        End If
        Try
            BeforeInvoke(methodName)
            method.Invoke(Me, Nothing)
        Catch ex As Exception
            Dim result As New Hashtable()
            result("error") = -1
            result("message") = ex.InnerException
            result("stackTrace") = ex.StackTrace
            Dim json As String = MiniJson.JSON.Encode(result)
            Response.Clear()
            Response.Write(json)
        Finally
            AfterInvoke(methodName)
        End Try
    End Sub
#Region "初始化"
    '权限管理 
    Protected Sub BeforeInvoke(ByVal methodName As String)
        'Hashtable user = GetUser();  
        'if (user.role == "admin" && methodName == "remove") throw .       
    End Sub

    '日志管理 
    Protected Sub AfterInvoke(ByVal methodName As String)
    End Sub



    '解析字符串,Json格式:[{"key":"value","key":"value"}],返回Hashtable
    Public Shared Function GetHashTableByJosn(ByVal JsonStr As String) As Hashtable
        Dim sb As New StringBuilder(StringDispose(JsonStr.Trim()))
        sb.Remove(0, 2)
        sb.Remove(sb.Length - 2, 2)
        Dim strs As String = sb.ToString()
        Dim strArrA As String() = strs.Split(New Char() {","c})
        If strArrA Is Nothing OrElse strArrA.Length < 1 Then
            Return Nothing
        End If
        Dim hs As New Hashtable()
        For Each str As String In strArrA
            Dim strArrB As String() = str.Split(New Char() {":"c})
            hs.Add(strArrB(0), strArrB(1))
        Next
        Return hs
    End Function

    Public Shared Function StringDispose(ByVal str As String) As String
        If str Is Nothing OrElse str.Length < 1 Then
            Return ""
        End If
        Dim strs As String = str.Replace(" ", "")
        strs = strs.Replace("'", "")
        strs = strs.Replace("""", "")
        strs = strs.Replace(vbLf, "")
        strs = strs.Replace(vbTab, "")
        strs = strs.Replace(vbCr, "")
        strs = strs.Replace(vbCr & vbLf, "")
        strs = strs.Replace(Environment.NewLine.ToString(), "")
        Return strs
    End Function

    ''' <summary>
    ''' 根据key val返回Hashtable
    ''' </summary>    
    Public Shared Function getHtMsg(ByVal GStoreVal As String, ByVal GCodeVal As String, ByVal GMessageVal As String) As Hashtable
        Dim record As Hashtable = New Hashtable
        record("GStore") = GStoreVal
        record("GCode") = GCodeVal
        record("GMessage") = GMessageVal
        Return record
    End Function
#End Region

#Region "登录"
    Public Sub Login()
        Dim LoginID As String = Request("LoginID")
        Dim pwd As String = Request("UserPass")
        Dim ret As String
        Dim json As String = ""
        Try
            Dim strSQL As String = "SELECT user_Id,user_Name,user_Level,user_Email,user_Status FROM users WHERE loginId='" & LoginID & "' AND password='" & pwd & "'"
            Dim db As New DbHelper(connBakSys)
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)

            json = MiniJson.JSON.Encode(arr)

            If arr.Count = 1 Then
                If arr(0)("user_Id") = 1 Then
                    strSQL = "SELECT m.menu_Id,m.menu_Name,m.parent_Id,m.url FROM menu m WHERE m.bVisible=1 ORDER BY m.order_Id"
                Else
                    strSQL = "SELECT m.menu_Id,m.menu_Name,m.parent_Id,m.url FROM useraccess a,menu m WHERE a.menu_Id=m.menu_Id AND m.bVisible=1 AND a.user_Id=" & arr(0)("user_Id") & " ORDER BY m.order_Id"
                End If
                Dim cmd2 As DbCommand = db.GetSqlStringCommond(strSQL)
                Dim dt2 As DataTable = db.ExecuteDataTable(cmd2)
                If dt2.Rows.Count = 0 Then
                    ret = "You  have no permission"
                Else
                    System.Web.Security.FormsAuthentication.SetAuthCookie(LoginID, False)
                    Session("UserID") = arr(0)("user_Id")
                    Session("UserName") = arr(0)("user_Name")
                    Session("UserLevel") = arr(0)("user_Level")
                    Session("UserStatus") = arr(0)("user_Status")
                    ret = "ok"
                End If
            Else
                ret = "error"
            End If
        Catch ex As Exception
            ret = ex.Message
        End Try
        json = "{""loginMsg"":""" & ret & """,""list"":" & json & "}"
        Response.Write(json)

    End Sub

    Public Sub ConfirmStockDetail()
        Dim LocationID As String = Request("LocationID")
        Dim Qty As Decimal = Request("Quantity2")
        Dim DetailID As Integer = Request("DetailID")

        Dim ret As String
        Try
            Dim db As New DbHelper
            Dim cmd As DbCommand = db.GetStoredProcCommond("RF_In")

            Using t As Trans = New Trans()
                cmd.Connection = t.DbConnection
                cmd.Transaction = t.DbTrans
                Try
                    db.AddInParameter(cmd, "@DetailID", DbType.Int32, DetailID)
                    db.AddInParameter(cmd, "@Place", DbType.String, LocationID)
                    db.AddInParameter(cmd, "@Qty", DbType.Decimal, Qty)
                    db.AddInParameter(cmd, "@UserID", DbType.Int32, Session("UserID"))
                    cmd.ExecuteNonQuery()

                    t.Commit()
                    ret = "ok"
                Catch exp As Exception
                    t.RollBack()
                    ret = exp.Message
                End Try
                cmd.Connection.Close()

            End Using

        Catch ex As Exception
            ret = ex.Message
        End Try

        Response.Write(ret)

    End Sub

    Public Sub GetLoginInfo()
        Try
            Dim db As New DbHelper
            Dim strSQL As String = "SELECT UserName,StoreName  FROM Users u,Stores s WHERE u.UserID=" & Session("UserID") & " AND s.Storeid= " & Session("StoreID")
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)

            Dim json As String = MiniJson.JSON.Encode(arr)
            Response.Write(json)
        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetStore()
        Try
            Dim db As New DbHelper
            Dim strSQL As String = "SELECT StoreID,StoreName FROM Stores WHERE Enable=1 AND BottomLevel=1 "
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)

            Dim json As String = MiniJson.JSON.Encode(arr)
            Response.Write(json)
        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetMenu()
        Try
            Dim db As New DbHelper
            Dim strSQL As String
            If Session("UserID") = 1 Then
                strSQL = "SELECT m.MenuID,m.MenuText,m.URL FROM RFMenu m Where ParentID>0 AND Visible=1 order by m.MenuSort"
            Else
                strSQL = "SELECT f.MenuID,m.MenuText,m.URL FROM RFUserAccess f,RFMenu m WHERE f.MenuID=m.MenuID And m.ParentID>0 AND m.Visible=1 And StoreID=" & Session("StoreID") & " And UserID=" & Session("UserID") & " Order by m.MenuSort"
            End If
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)
            Dim json As String
            json = MiniJson.JSON.Encode(arr)

            Response.Write(json)
        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetStockDetail()
        Try
            Dim DetailID As Integer = Request("DetailID")
            Dim db As New DbHelper
            Dim strSQL As String = "SELECT l.*,d.DetailID,d.Batch,d.ExpiryDate,d.Quantity,i.Place FROM vStockDetail d LEFT JOIN Inventory i ON d.PID=i.PBID AND i.WarehouseID=" & Session("StoreID") & " AND i.Quantity>0,Products l WHERE d.PID=l.PID AND d.DetailID=" & DetailID
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)
            Dim json As String
            json = MiniJson.JSON.Encode(arr)

            Response.Write(json)
        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetInvByLocation()
        Try
            Dim LocationID As String = Request("OldLoc")
            Dim db As New DbHelper
            Dim strSQL As String = "SELECT InventoryID,PName,Model,FromPlace,Batch,ExpiryDate,i.PID,i.Quantity,i.Place FROM vInventory i,Products l WHERE i.PID=l.PID AND i.WarehouseID= " & Session("StoreID") & " AND Place='" & LocationID & "' "
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)
            Dim json As String
            json = MiniJson.JSON.Encode(arr)

            Response.Write(json)
        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetStock()
        Try
            Dim strSQL As String = "select f.StockID,f.StockNumber,CONVERT(NVARCHAR(10),f.SystemDate,120) AS SystemDate,DetailID,s.ClientName,s.OwnerName,PName,BarCode,pb.Batch,pb.ProductDate,pb.ExpiryDate,Quantity FROM Stock f,vStockDetail d,vSupplier s,Products p,ProductBatches pb " & _
                                "WHERE f.StockID=d.StockID AND f.ClientID=s.ClientID AND f.StoreID=" & Session("StoreID") & " AND d.PID=p.PID AND f.Status=0 AND d.SubStatus=0 order By f.StockID DESC"

            Dim db As New DbHelper
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)
            Dim json As String
            json = MiniJson.JSON.Encode(arr)

            Response.Write(json)
        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetClientByClientCode()
        Try
            Dim clientCode As Integer = Request("clientCode")
            Dim strSQL As String = "SELECT ClientID,ClientCode,ClientName FROM dbo.Clients WHERE ClientCode='" & clientCode & "'"
            Dim db As New DbHelper
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)
            Dim json As String
            json = MiniJson.JSON.Encode(arr)

            Response.Write(json)
        Catch ex As Exception

        End Try
    End Sub

    Public Sub GetProductByPCode()
        Try
            Dim pCode As Integer = Request("pCode")
            Dim strSQL As String = "SELECT PID,PCode5,PName,Model,FromPlace FROM dbo.Level5Products WHERE PCode5='" & pCode & "'"
            Dim db As New DbHelper
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)
            Dim json As String
            json = MiniJson.JSON.Encode(arr)

            Response.Write(json)
        Catch ex As Exception

        End Try
    End Sub

    Public Sub GetSaleBill()

        Try
            pageSize = ConfigurationManager.AppSettings("pageSize")
            Dim pageIndex As Integer = Request("pageIndex")
            Dim clientCode As String = Request("clientCode")
            Dim formNumber As String = Request("formNumber")
            Dim startDate As String = Request("startDate")
            Dim endDate As String = Request("endDate")
            Dim formType As String = Request("formType")
            Dim strSQL As String = "SELECT FID,FormNumber,CONVERT(NVARCHAR(10),f.SystemDate,120) AS SystemDate,c.ClientName,CONVERT(NVARCHAR(20),CONVERT(DECIMAL(16,2),TaxSum)) AS TaxSum " & _
                                "INTO #list " & _
                                "FROM dbo.Form f,dbo.Clients c WHERE f.ClientID=c.ClientID AND FormTypeID=" + formType + " AND SystemDate >='" & startDate & "' AND SystemDate <='" & endDate & "' "
            If clientCode <> "" Then
                strSQL &= " AND c.ClientCode='" & clientCode & "' "
            End If
            If formNumber <> "" Then
                strSQL &= " AND f.FormNumber='" & formNumber & "' "
            End If
            strSQL &= " ORDER BY FID DESC"
            strSQL &= ";DECLARE @count INT;SELECT @count=COUNT(FID) FROM #list;SELECT TOP " & pageSize & " *,@count AS rCount FROM #list t WHERE NOT EXISTS (SELECT 1 FROM (SELECT TOP ((" & pageIndex & "-1)*" & pageSize & ") FID FROM #list ORDER BY FID) a WHERE a.FID=t.FID) ORDER BY t.FID"
            Dim db As New DbHelper
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)
            Dim rowCount As Integer = arr(0)("rCount")
            If rowCount Mod pageSize <> 0 Then
                rowCount = rowCount / pageSize + 1
            Else
                rowCount = rowCount / pageSize
            End If

            Dim json As String
            json = MiniJson.JSON.Encode(arr)
            json = "{""pageCount"":" & rowCount & ",""CurrentPage"":" & pageIndex & ",""pageSize"":" & pageSize & ",""list"":" & json & "}"
            Response.Write(json)

        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetSaleBillDetail()
        Try
            Dim FID As Integer = Request("FID")
            Dim db As New DbHelper
            Dim strSQL As String = "SELECT FDetailID,PName,Model,FromPlace,BaseUnit,Batch,Quantity,CONVERT(NVARCHAR(20),CONVERT(DECIMAL(16,2),TaxPrice)) AS TaxPrice,CONVERT(NVARCHAR(20),CONVERT(DECIMAL(16,2),TaxTotal)) AS TaxTotal FROM dbo.FormDetail d,dbo.Level5Products p WHERE d.PID=p.PID AND p.PID<>3 AND FID=" & FID
            Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
            Dim arr As ArrayList = db.ExecuteArrayList(cmd)
            Dim json As String
            json = MiniJson.JSON.Encode(arr)

            Response.Write(json)
        Catch ex As Exception

        End Try

    End Sub

    Public Sub ReverseSaleBillDetail()
        Dim DetailID As Integer = Request("DetailID")
        Dim db As New DbHelper
        Dim strSQL As String = "EXEC ReverseDetail @FDetailID=" & DetailID
        Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
        Dim arr As Integer = db.ExecuteNonQuery(cmd)
        Dim json As String = "{""ret"":" & arr & "}"
        Response.Write(json)
    End Sub

    Public Sub InsertSaleBillDetail()
        Dim ClientID As Integer = Request("ClientID")
        Dim PID As Integer = Request("PID")
        Dim Batch As String = Request("Batch")
        Dim Quantity As Double = Request("Quantity")
        Dim TaxPrice As Double = Request("TaxPrice")

        Dim db As New DbHelper
        Dim strSQL As String = String.Format("EXEC InsertDetail @ClientID={0},@PID={1},@Batch='{2}',@Quantity={3},@TaxPrice={4}", ClientID, PID, Batch, Quantity, TaxPrice)
        Dim cmd As DbCommand = db.GetSqlStringCommond(strSQL)
        Dim arr As Integer = db.ExecuteNonQuery(cmd)
        Dim json As String = "{""ret"":" & arr & "}"
        Response.Write(json)
    End Sub

    Public Sub ChangeLocation()
        Dim NewLoc As String = Request("NewLoc")
        Dim InventoryID As Integer = Request("InventoryID")

        Dim ret As String
        Try
            Dim db As New DbHelper
            Dim cmd As DbCommand = db.GetStoredProcCommond("Bay_ChangeLocation")

            Try
                db.AddInParameter(cmd, "@InventoryID", DbType.Int32, InventoryID)
                db.AddInParameter(cmd, "@LocationID", DbType.String, NewLoc)

                db.ExecuteNonQuery(cmd)

                ret = "ok"
            Catch exp As Exception
                ret = exp.Message
            End Try


        Catch ex As Exception
            ret = ex.Message
        End Try

        Response.Write(ret)

    End Sub

    Public Sub ChkInv()
        Dim Qty As Decimal = Request("Qty")
        Dim Quantity As Decimal = Request("Quantity")
        Dim LocationID As String = Request("LocationID")
        Dim PID As Integer = Request("PID")
        Dim ret As String
        Try
            Dim db As New DbHelper
            Dim cmd As DbCommand = db.GetStoredProcCommond("Bay_ChkInv")

            Try
                db.AddInParameter(cmd, "@LocationID", DbType.String, LocationID)
                db.AddInParameter(cmd, "@WareHouseID", DbType.Int32, Session("StoreID"))
                db.AddInParameter(cmd, "@PID", DbType.Int32, PID)
                db.AddInParameter(cmd, "@OldQty", DbType.Decimal, Quantity)
                db.AddInParameter(cmd, "@NewQty", DbType.Decimal, Qty)
                db.AddInParameter(cmd, "@CreatorID", DbType.Int32, Session("UserID"))
                db.ExecuteNonQuery(cmd)

                ret = "ok"
            Catch exp As Exception
                ret = exp.Message
            End Try


        Catch ex As Exception
            ret = ex.Message
        End Try

        Response.Write(ret)

    End Sub
#End Region

#Region "集单"
    Public Sub AutoGatherOrders()

        Dim arr As New ArrayList
        Dim json As String


        Dim ret As String

        Dim strSql As String = "SELECT [Value],[Display] FROM dbo.[Types] WHERE [Type]='集单分公司'"
        Dim db As New DbHelper
        Dim cmd As DbCommand = db.GetSqlStringCommond(strSql)
        Dim dtStores As DataTable = db.ExecuteDataTable(cmd)
        Dim i As Integer
        For i = 0 To dtStores.Rows.Count
            Try
                ret = CheckGatherStatus(dtStores.Rows(i)("Value"))
                '判断集单状态
                If ret = "" Then
                    Dim dsOrder As DataSet = GetOrdersFromSup(dtStores.Rows(i)("Value"))
                    ret = InsertOrders(dsOrder)
                    '判断是否有需要集单的订单
                    If ret = "" Then
                        Dim dtOrder As DataTable = GetOrdersFromLocal(dtStores.Rows(i)("Value"))
                        If Not dtOrder Is Nothing Then

                            arr.Add(getHtMsg(dtStores.Rows(i)("Display"), "1003", "获取本地合并品种后的订单正常,准备获取批号进行采购!"))
                        Else
                            arr.Add(getHtMsg(dtStores.Rows(i)("Display"), "4003", "获取本地订单失败,正常情况下不可能出现"))
                        End If
                        arr.Add(getHtMsg(dtStores.Rows(i)("Display"), "1002", "获取上游订单正常,下载并回传状态完毕,准备获取本地订单!"))
                    Else
                        arr.Add(getHtMsg(dtStores.Rows(i)("Display"), "4002", ret))
                    End If
                    arr.Add(getHtMsg(dtStores.Rows(i)("Display"), "1001", "集单状态正常,准备下载订单!"))
                Else
                    arr.Add(getHtMsg(dtStores.Rows(i)("Display"), "4001", ret))
                End If
            Catch ex As Exception
                arr.Add(getHtMsg(dtStores.Rows(i)("Display"), "4444", ex.Message))
            End Try
        Next

        json = MiniJson.JSON.Encode(arr)
        Response.Write(json)
    End Sub

    ''' <summary>
    ''' 判断集单状态，今天是否已经集单
    ''' </summary>
    Private Function CheckGatherStatus(ByVal storeID As Integer) As String
        Return ""
    End Function

    ''' <summary>
    ''' 获取订单信息
    ''' </summary>
    Private Function GetOrdersFromSup(ByVal storeID As Integer) As DataSet

        Return Nothing
    End Function

    ''' <summary>
    ''' 将获取的订单写入本地数据库
    ''' </summary>
    Private Function InsertOrders(ByVal dsOrder As DataSet) As String
        If dsOrder.Tables.Count = 2 AndAlso dsOrder.Tables(0).Rows.Count > 0 Then
            Return ""
        Else
            Return "获取订单失败,无可用订单!"
        End If

    End Function

    ''' <summary>
    ''' 获取本地数据库合并后的订单供采购使用,取出供应价和毛利价,方便后面选批号
    ''' </summary>
    Private Function GetOrdersFromLocal(ByVal storeID As Integer) As DataTable
        Return Nothing
    End Function

    ''' <summary>
    ''' 为订单选择批号进行采购
    ''' </summary>
    Private Function ChangeBatchForOrders(ByVal dtOrder As DataTable) As String
        Dim i As Integer
        Dim dvBatch As DataView
        Dim bMeet As Boolean
        Dim retMsg As String
        For i = 0 To dtOrder.Rows.Count
            dvBatch = ChangeBatch(dtOrder.Rows(i)("PID"), dtOrder.Rows(i)("Quantity"), dtOrder.Rows(i)("TaxPrice"), dtOrder.Rows(i)("CostPrice"), bMeet, retMsg)
            If dvBatch Is Nothing OrElse dvBatch.Count = 0 Then

                Continue For
            End If
        Next
        Return ""
    End Function

    ''' <summary>
    ''' 根据规则为单个品种获取批号:
    ''' GSP规则:排除召回记录里召回的批号
    ''' 毛利规则:采购价小于等于供应价 或 采购价小于等于毛利价 (毛利价有可能高于供应价,即负毛利销售)
    ''' 效期规则:只采购有效期在11个月以上的批号
    ''' 批号规则:一个批号能满足优先采购一个批号，不够再选择多批号
    ''' 零整规则:整件优先,具体逻辑详见代码
    ''' </summary>
    Private Function ChangeBatch(ByVal pid As Integer, ByVal qty As Double, ByVal taxPrice As Double, ByVal costPrice As Double, ByRef bMeet As Boolean, ByRef retMsg As String) As DataView

        Return Nothing
    End Function

    Private Sub ReturnOrderDetailStatus()
        Dim strSql As String = ""
    End Sub
#End Region

End Class
