
Public Class TestOverloadedByArgumentNumbersParameterizedProperty
    Private _grid(5, 5) As String

    Public Sub New()
        For x As Integer = 0 To 5
            For y As Integer = 0 To 5
                _grid(x, y) = String.Format("{0}, {1}", x, y)
            Next
        Next
    End Sub

    Public Property Grid(x As Integer) As String
        Get
            Return _grid(x, 0)
        End Get
        Set(value As String)
            _grid(x, 0) = value
        End Set
    End Property

    Public Property Grid(x As Integer, y As Integer) As String
        Get
            Return _grid(x, y)
        End Get
        Set(value As String)
            _grid(x, y) = value
        End Set
    End Property
End Class
