Imports Microsoft.VisualBasic

Imports System.Data
Imports System.Data.Common
Imports System.Configuration

Public Class DbHelper

    Private Shared dbProviderName As String = ConfigurationManager.ConnectionStrings("DbHelperProvider").ConnectionString
    Private Shared dbConnectionString As String = ConfigurationManager.ConnectionStrings("DbHelperConnectionString").ConnectionString

    Private connection As DbConnection
    Public Sub New()
        Me.connection = CreateConnection(DbHelper.dbConnectionString)
    End Sub
    Public Sub New(ByVal connectionString As String)
        Me.connection = CreateConnection(connectionString)
    End Sub
    Public Shared Function CreateConnection() As DbConnection
        Dim dbfactory As DbProviderFactory = DbProviderFactories.GetFactory(DbHelper.dbProviderName)
        Dim dbconn As DbConnection = dbfactory.CreateConnection()
        dbconn.ConnectionString = DbHelper.dbConnectionString
        Return dbconn
    End Function
    Public Shared Function CreateConnection(ByVal connectionString As String) As DbConnection
        Dim dbfactory As DbProviderFactory = DbProviderFactories.GetFactory(DbHelper.dbProviderName)
        Dim dbconn As DbConnection = dbfactory.CreateConnection()
        dbconn.ConnectionString = connectionString
        Return dbconn
    End Function

    Public Function GetStoredProcCommond(ByVal storedProcedure As String) As DbCommand
        Dim dbCommand As DbCommand = connection.CreateCommand()
        dbCommand.CommandText = storedProcedure
        dbCommand.CommandType = CommandType.StoredProcedure
        Return dbCommand
    End Function
    Public Function GetSqlStringCommond(ByVal sqlQuery As String) As DbCommand
        Dim dbCommand As DbCommand = connection.CreateCommand()
        dbCommand.CommandText = sqlQuery
        dbCommand.CommandType = CommandType.Text
        Return dbCommand
    End Function

    '增加参数
#Region "增加参数"
    Public Sub AddParameterCollection(ByVal cmd As DbCommand, ByVal dbParameterCollection As DbParameterCollection)
        For Each dbParameter As DbParameter In dbParameterCollection
            cmd.Parameters.Add(dbParameter)
        Next
    End Sub
    Public Sub AddOutParameter(ByVal cmd As DbCommand, ByVal parameterName As String, ByVal dbType As DbType, ByVal size As Integer)
        Dim dbParameter As DbParameter = cmd.CreateParameter()
        dbParameter.DbType = dbType
        dbParameter.ParameterName = parameterName
        dbParameter.Size = size
        dbParameter.Direction = ParameterDirection.Output
        cmd.Parameters.Add(dbParameter)
    End Sub
    Public Sub AddInParameter(ByVal cmd As DbCommand, ByVal parameterName As String, ByVal dbType As DbType, ByVal value As Object)
        Dim dbParameter As DbParameter = cmd.CreateParameter()
        dbParameter.DbType = dbType
        dbParameter.ParameterName = parameterName
        dbParameter.Value = value
        dbParameter.Direction = ParameterDirection.Input
        cmd.Parameters.Add(dbParameter)
    End Sub
    Public Sub AddReturnParameter(ByVal cmd As DbCommand, ByVal parameterName As String, ByVal dbType As DbType)
        Dim dbParameter As DbParameter = cmd.CreateParameter()
        dbParameter.DbType = dbType
        dbParameter.ParameterName = parameterName
        dbParameter.Direction = ParameterDirection.ReturnValue
        cmd.Parameters.Add(dbParameter)
    End Sub
    Public Function GetParameter(ByVal cmd As DbCommand, ByVal parameterName As String) As DbParameter
        Return cmd.Parameters(parameterName)
    End Function

#End Region

    ' 执行
#Region "执行"
    Public Function ExecuteDataSet(ByVal cmd As DbCommand) As DataSet
        Dim dbfactory As DbProviderFactory = DbProviderFactories.GetFactory(DbHelper.dbProviderName)
        Dim dbDataAdapter As DbDataAdapter = dbfactory.CreateDataAdapter()
        dbDataAdapter.SelectCommand = cmd
        Dim ds As New DataSet()
        dbDataAdapter.Fill(ds)
        Return ds
    End Function

    Public Function ExecuteDataTable(ByVal cmd As DbCommand) As DataTable
        Dim dbfactory As DbProviderFactory = DbProviderFactories.GetFactory(DbHelper.dbProviderName)
        Dim dbDataAdapter As DbDataAdapter = dbfactory.CreateDataAdapter()
        dbDataAdapter.SelectCommand = cmd
        Dim dataTable As New DataTable()
        dbDataAdapter.Fill(dataTable)
        Return dataTable
    End Function

    Public Function ExecuteArrayList(ByVal cmd As DbCommand) As ArrayList
        Dim dbfactory As DbProviderFactory = DbProviderFactories.GetFactory(DbHelper.dbProviderName)
        Dim dbDataAdapter As DbDataAdapter = dbfactory.CreateDataAdapter()
        dbDataAdapter.SelectCommand = cmd
        Dim dataTable As DataTable = New DataTable()
        dbDataAdapter.Fill(dataTable)
        If dataTable.Rows.Count = 0 Then
            Dim t As New ArrayList()
            t.Add("")
            Return t
        Else
            Return DataTable2ArrayList(dataTable)
        End If

    End Function


    Private Function DataTable2ArrayList(ByVal data As DataTable) As ArrayList
        Dim array As ArrayList = New ArrayList()
        For i As Integer = 0 To data.Rows.Count - 1
            Dim row As DataRow = data.Rows(i)
            Dim record As Hashtable = New Hashtable
            For j As Integer = 0 To data.Columns.Count - 1
                Dim cellValue As Object = row(j)
                record(data.Columns(j).ColumnName) = cellValue
            Next
            array.Add(record)
        Next
        Return array
    End Function


    Public Function ExecuteReader(ByVal cmd As DbCommand) As DbDataReader
        cmd.Connection.Open()
        Dim reader As DbDataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection)
        Return reader
    End Function
    Public Function ExecuteNonQuery(ByVal cmd As DbCommand) As Integer
        cmd.Connection.Open()
        Dim ret As Integer = cmd.ExecuteNonQuery()
        cmd.Connection.Close()
        Return ret
    End Function

    Public Function ExecuteScalar(ByVal cmd As DbCommand) As Object
        cmd.Connection.Open()
        Dim ret As Object = cmd.ExecuteScalar()
        cmd.Connection.Close()
        Return ret
    End Function
#End Region

    ' 执行事务
#Region "执行事务"
    Public Function ExecuteDataSet(ByVal cmd As DbCommand, ByVal t As Trans) As DataSet
        cmd.Connection = t.DbConnection
        cmd.Transaction = t.DbTrans
        Dim dbfactory As DbProviderFactory = DbProviderFactories.GetFactory(DbHelper.dbProviderName)
        Dim dbDataAdapter As DbDataAdapter = dbfactory.CreateDataAdapter()
        dbDataAdapter.SelectCommand = cmd
        Dim ds As New DataSet()
        dbDataAdapter.Fill(ds)
        Return ds
    End Function

    Public Function ExecuteDataTable(ByVal cmd As DbCommand, ByVal t As Trans) As DataTable
        cmd.Connection = t.DbConnection
        cmd.Transaction = t.DbTrans
        Dim dbfactory As DbProviderFactory = DbProviderFactories.GetFactory(DbHelper.dbProviderName)
        Dim dbDataAdapter As DbDataAdapter = dbfactory.CreateDataAdapter()
        dbDataAdapter.SelectCommand = cmd
        Dim dataTable As New DataTable()
        dbDataAdapter.Fill(dataTable)
        Return dataTable
    End Function

    Public Function ExecuteReader(ByVal cmd As DbCommand, ByVal t As Trans) As DbDataReader
        cmd.Connection.Close()
        cmd.Connection = t.DbConnection
        cmd.Transaction = t.DbTrans
        Dim reader As DbDataReader = cmd.ExecuteReader()
        Dim dt As New DataTable()
        Return reader
    End Function
    Public Function ExecuteNonQuery(ByVal cmd As DbCommand, ByVal t As Trans) As Integer
        cmd.Connection.Close()
        cmd.Connection = t.DbConnection
        cmd.Transaction = t.DbTrans
        Dim ret As Integer = cmd.ExecuteNonQuery()
        Return ret
    End Function

    Public Function ExecuteScalar(ByVal cmd As DbCommand, ByVal t As Trans) As Object
        cmd.Connection.Close()
        cmd.Connection = t.DbConnection
        cmd.Transaction = t.DbTrans
        Dim ret As Object = cmd.ExecuteScalar()
        Return ret
    End Function
#End Region
End Class

Public Class Trans
    Implements IDisposable
    Private conn As DbConnection
    Private m_dbTrans As DbTransaction
    Public ReadOnly Property DbConnection() As DbConnection
        Get
            Return Me.conn
        End Get
    End Property
    Public ReadOnly Property DbTrans() As DbTransaction
        Get
            Return Me.m_dbTrans
        End Get
    End Property

    Public Sub New()
        conn = DbHelper.CreateConnection()
        conn.Open()
        m_dbTrans = conn.BeginTransaction()
    End Sub
    Public Sub New(ByVal connectionString As String)
        conn = DbHelper.CreateConnection(connectionString)
        conn.Open()
        m_dbTrans = conn.BeginTransaction()
    End Sub
    Public Sub Commit()
        m_dbTrans.Commit()
        Me.Colse()
    End Sub

    Public Sub RollBack()
        m_dbTrans.Rollback()
        Me.Colse()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Me.Colse()
    End Sub

    Public Sub Colse()
        If conn.State = System.Data.ConnectionState.Open Then
            conn.Close()
        End If
    End Sub
End Class

