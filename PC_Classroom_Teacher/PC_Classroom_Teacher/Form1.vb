Public Class Form1
    Dim strPasswd As String = "" '�o��i�H�]�w�Юv�ݪ��K�X
    Dim intFail As Integer = 1
    Dim boolLock As Boolean = False
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        '�N�K�X�����w��b�e��������
        Me.Left = Screen.PrimaryScreen.Bounds.Width / 2 - Width / 2
        Me.Top = Screen.PrimaryScreen.Bounds.Height / 2 - Height / 2
    End Sub

    Private Sub check_permission()
        boolLock = True
        If txtInputPassword.Text = strPasswd Then
            Form2.Show()
            If strPasswd.Length = 0 Then
                MsgBox("�ثe�S���K�X�A��ĳ�]�w�@�ձK�X�W�h")
            End If
            Me.Visible = False
            txtInputPassword.Text = ""
        Else
            If intFail < 5 Then
                MsgBox("�Э��s��J�K�X�I", , "�K�X���~")
                txtInputPassword.Text = ""
                intFail = intFail + 1
            Else
                Me.Visible = False
                MsgBox("�K�X���~�F5���A�{�������I", MsgBoxStyle.Critical, "�K�X���~")
                End
            End If
        End If
    End Sub

    Private Sub confirm_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles confirm.Click
        check_permission()
    End Sub

    Private Sub cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cancel.Click
        Me.Visible = False
        MsgBox("�{�������I")
        End
    End Sub

    Private Sub TextBox1_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtInputPassword.KeyUp
        If e.KeyValue = System.Windows.Forms.Keys.Enter Then '���U Enter ��
            If boolLock Then
                boolLock = False
            Else
                check_permission()
            End If
        End If
    End Sub
End Class


