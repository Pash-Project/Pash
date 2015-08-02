
Public Class TestReadOnlyParameterizedProperty

    Private _fileNames As List(Of String)

    Public Sub New()
        Me.New(Enumerable.Empty(Of String)())
    End Sub

    Public Sub New(ByVal fileNames As IEnumerable(Of String))
        _fileNames = fileNames.ToList()
    End Sub

    Public ReadOnly Property FileNames(index As Integer) As String
        Get
            Return _fileNames(index)
        End Get
    End Property
End Class