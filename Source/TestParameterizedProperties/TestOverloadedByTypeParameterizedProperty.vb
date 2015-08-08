
Public Class TestOverloadedByTypeParameterizedProperty

    Private _fileNames As List(Of String)

    Public Sub New()
        Me.New(Enumerable.Empty(Of String)())
    End Sub

    Public Sub New(ByVal fileNames As IEnumerable(Of String))
        _fileNames = fileNames.ToList()
    End Sub

    Public Property FileNames(index As Integer) As String
        Get
            Return _fileNames(index)
        End Get
        Set(value As String)
            _fileNames(index) = value
        End Set
    End Property

    Public Property FileNames(fileName As String) As String
        Get
            Return _fileNames.IndexOf(fileName)
        End Get
        Set(value As String)
            _fileNames.Add(value)
        End Set
    End Property
End Class

