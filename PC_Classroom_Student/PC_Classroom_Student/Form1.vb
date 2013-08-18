Imports System.Net
Imports System.Net.Dns
Imports System.Net.NetworkInformation
Imports System.net.sockets
Imports System.Text
Imports System.IO
Imports Microsoft.Win32

Public Class Form1

    Private Declare Function SetComputerName Lib "kernel32" Alias "SetComputerNameA" (ByVal lpComputerName As String) As Long
    Dim boolAutoChangeName As Boolean = False '�Ұʮɦ۰��ˬd&���q���W��
    Dim strListPath As String = "C:\Program Files\MMLab\MAC.txt" '�s��ǥ;���ƪ��a��
    Dim strIP_Prefix As String = "192.168.55"  '�T�wIP���q
    Dim strNetMask As String = "255.255.255.0" '���־B�n
    Dim strGateway As String = "192.168.55.1"  '�w�]Gateway
    Dim strListURL As String = "http://"       'MAC��r�ɤU����}
    Dim strNamePrefix = "S"                    '�ǥ;��s���e�q
    Dim PC_Name, PC_MAC, PC_FNo As String      '�x�s��ơG�q���W�١BMAC�B�s��

    Dim UDPS, UDPC As UdpClient
    Dim recvive_port As Integer = 5566
    Dim send_port As Integer = 5567

    Dim source, teacher As IPEndPoint
    Dim SendB, RecvB As Byte()
    Dim strRecv, message As String
    Dim lastRecv As String
    Dim addr() As IPAddress
    Dim WC As New WebClient

    Dim strNAME(55) As String
    Dim strMAC(55) As String
    Dim strNo(55) As String
    Dim boolInList As String = False

    '���õ���
    Private Sub Form1_Layout(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LayoutEventArgs) Handles MyBase.Layout
        Me.Hide()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim nics() As NetworkInterface = NetworkInterface.GetAllNetworkInterfaces
        Dim properties As IPInterfaceProperties
        Dim adapter As NetworkInterface
        Dim strLine As String = ""
        Dim tmpMAC As String = ""
        Dim tmp() As String

        Control.CheckForIllegalCrossThreadCalls = False
        Try
            'UDP Server: �������O��
            UDPS = New UdpClient(recvive_port)
            UDPS.EnableBroadcast = True
            source = New IPEndPoint(IPAddress.Any, recvive_port)

        Catch ex As Exception

            End

        End Try

        Timer1.Start()

        If My.Computer.FileSystem.FileExists(strListPath) = False Then
            Exit Sub
        End If

        Try
            'Ū���ǥ;������
            Dim SR As StreamReader = New StreamReader(strListPath, False)
            For i = 1 To 55
                strLine = SR.ReadLine      '��r�ɳv����X
                tmp = Split(strLine, ",")  '�̾ڳr�I���Φ��ⳡ��  SS1(0)���W��  SS1(1)��MAC��}
                strNAME(i) = tmp(0)
                strMAC(i) = tmp(1)
                strNo(i) = tmp(2)
            Next

            SR.Close()

            '���oMAC Address
            For Each adapter In nics
                properties = adapter.GetIPProperties()
                If adapter.NetworkInterfaceType = NetworkInterfaceType.Ethernet Then
                    tmpMAC = adapter.GetPhysicalAddress.ToString

                    '�L�o���~��MAC��}�G���קP�_�B�h���������d
                    If tmpMAC.Length = 12 Then
                        For i = 1 To 55
                            If tmpMAC.ToUpper = strMAC(i).ToUpper Then
                                PC_MAC = strMAC(i)
                                PC_Name = strNAME(i)
                                PC_FNo = strNo(i)
                                boolInList = True '�N��bList�ɮק�����������

                                '�۰��ˬd&���q���W��
                                If boolAutoChangeName And boolInList Then
                                    change_pc_name()
                                End If
                                Exit Sub

                            End If
                        Next
                    End If

                End If
            Next adapter

            '�S�����N�H�K���@�Ӵ����W�h�A�����ЫǤ������ƦW�٪�PC�Y�i
            PC_MAC = tmpMAC.ToUpper
            PC_Name = strNamePrefix + Mid(PC_MAC, 7, 6)

        Catch ex As Exception

        End Try
    End Sub

    '���q���W��
    Sub change_pc_name()
        If Environment.MachineName <> PC_Name Then
            SetComputerName(PC_Name)
            MsgBox("�q���W�٤w�ܧ�""" + PC_Name + """�A�Э��s�}���H�K�ͮ�!!")
        End If
    End Sub

    'Timer�\��G�����Ӧ۱Юv�������O
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        '����Ӧ۱Юv�ݪ��R�O
        If UDPS.Available > 0 Then
            Try
                RecvB = UDPS.Receive(source)
                strRecv = Encoding.Default.GetString(RecvB, 0, RecvB.Length)

                '�N�T����� Debug form �W��
                debug.ListBox1.Items.Add("Received: " + strRecv)
                If debug.ListBox1.Items.Count > 0 Then
                    debug.ListBox1.SelectedIndex = debug.ListBox1.Items.Count - 1
                End If

            Catch ex As Exception
                Exit Sub
            End Try

            check_recv(strRecv)
        End If

    End Sub

    '���R�����쪺���O
    Private Sub check_recv(ByVal strRecv As String)

        Dim Count, cmd As Integer
        Dim i As Integer
        Dim temp1(), temp2() As String

        '�ˬd�O�_���Ʀ���
        If lastRecv = strRecv Then
            Exit Sub
        End If

        lastRecv = strRecv

        If Mid(strRecv, 1, 6) = "@,Scan" Then

            UDPC = New UdpClient()

            Try
                temp1 = Split(strRecv, ",")
                addr = Dns.GetHostAddresses(temp1(2))
                send_port = Int(temp1(3))
            Catch ex As Exception
                addr = Dns.GetHostAddresses("teacher")
                teacher = New IPEndPoint(addr(1), send_port)
            End Try

            ' ���oIPv4��}
            For i = 1 To addr.Length.ToString
                If addr(i).AddressFamily = AddressFamily.InterNetwork Then
                    teacher = New IPEndPoint(addr(i), send_port)
                    Exit For
                End If
            Next

            Try
                If PC_Name = "Teacher" Then
                    SendB = Encoding.Default.GetBytes("#," + PC_Name) '�ǰe�q���W��
                Else
                    SendB = Encoding.Default.GetBytes("#," + Mid(PC_Name, strNamePrefix.ToString.Length + 1, 2)) '�ǰe�q���W��
                End If

                UDPC.Send(SendB, SendB.Length, teacher)
                debug.ListBox1.Items.Add("Sent:      " + "#," + Mid(PC_Name, strNamePrefix.ToString.Length, 2))
            Catch ex As Exception

            End Try

        ElseIf Mid(strRecv, 1, 1) = "@" Then
            Try
                temp1 = Split(strRecv, ",") 'temp1(1):�ƶq  temp1(2):�q��   temp1(3):cmd  temp1(4):message����  temp1(5): Message
                temp2 = Split(temp1(2), "-") '�q���s��
                Count = Int(temp1(1)) '       �q���ƶq
                cmd = Int(temp1(3))  '        cmd�s��
                If cmd > 50 Then
                    message = temp1(4) '          �T��
                End If

            Catch ex As Exception
                Exit Sub
            End Try

            For i = 1 To Count
                '�s���P�����ۦP or ALL1(�]�tTeacher) or ALL2
                If (Mid(PC_Name, 2, 2) = temp2(i - 1)) Or (temp1(2) = "ALL1") Or ((temp1(2) = "ALL2") And PC_Name <> "Teacher") Then
                    do_cmd(cmd)
                    Exit For
                End If
            Next

        End If
    End Sub

    '����R�O
    Sub do_cmd(ByVal cmd As Integer)

        Select Case cmd

            Case 1 '����
                Shell("shutdown -s -t 0 ", AppWinStyle.Hide, False)

            Case 2 '���}��
                Shell("shutdown -r -t 0 ", AppWinStyle.Hide, False)

            Case 3 '����(�˼�30s)
                Shell("shutdown -s -t 30 -c ""�ѦѮv�q���ǰe�������O""", AppWinStyle.Hide, False)
                If MsgBox("�t�αN�b30�������A�����u�����v�Ы��u�T�w�v", MsgBoxStyle.OkOnly) = MsgBoxResult.Ok Then
                    Shell("shutdown -a", AppWinStyle.Hide, False)
                End If

            Case 4 '���}��(�˼�30s)
                Shell("shutdown -r -t 30 -c ""�ѦѮv�q���ǰe���s�}�����O""", AppWinStyle.Hide, False)
                If MsgBox("�t�αN�b30�����s�����A�����}���Ы�""�T�w""", MsgBoxStyle.OkOnly) = MsgBoxResult.Cancel Then
                    Shell("shutdown -a", AppWinStyle.Hide, False)
                End If

            Case 5 '��ܮୱ
                Dim clsidShell As New Guid("13709620-C279-11CE-A49E-444553540000")
                Dim shell As Object = Activator.CreateInstance(Type.GetTypeFromCLSID(clsidShell))
                shell.ToggleDesktop()

            Case 6 '�}���ɮ��`��
                Shell("c:\windows\explorer.exe", AppWinStyle.NormalFocus, False)

            Case 7 '��ܹq����T
                Information.Label1.Text = "Computer Name: " + PC_Name
                Information.Label2.Text = "MAC Address: " + Mid(PC_MAC, 1, 2) + "-" + Mid(PC_MAC, 3, 2) + "-" + Mid(PC_MAC, 5, 2) + "-" + Mid(PC_MAC, 7, 2) + "-" + Mid(PC_MAC, 9, 2) + "-" + Mid(PC_MAC, 11, 2)
                Information.Label3.Text = "Case Number: " + PC_FNo
                Information.Visible = True
                Information.Show()

            Case 8 '���ùq����T
                Information.Hide()

            Case 9 '��s�q���W��
                change_pc_name()

            Case 10 '��s�W�٦C��
                Try
                    WC.DownloadFile(strListURL, "tempListFile.txt")
                    My.Computer.FileSystem.MoveFile("tempListFile", strListPath, True)
                Catch ex As Exception

                End Try

            Case 11  'ipconfig
                Shell("cmd /K ipconfig", AppWinStyle.NormalFocus, False)

            Case 12  'ping google
                Shell("cmd /K ping www.google.com.tw", AppWinStyle.NormalFocus, False)

            Case 13  'trace dns
                Shell("cmd /K tracert 168.95.1.1", AppWinStyle.NormalFocus, False)

            Case 14  'trace google
                Shell("cmd /K tracert www.google.com.tw", AppWinStyle.NormalFocus, False)

            Case 15  '�]�w���T�wIP
                If Mid(PC_Name, 1, strNamePrefix.ToString.Length) = strNamePrefix And PC_Name.Length = 3 Then
                    Shell("netsh interface ip set address ""�ϰ�s�u"" static " + strIP_Prefix + "." + Int(Mid(PC_Name, 2, 2)).ToString + " " + strNetMask + " " + strGateway + " 1", AppWinStyle.NormalFocus, False)
                End If

            Case 16  '�]�w��DHCP
                Shell("netsh interface ip set address ""�ϰ�s�u"" dhcp", AppWinStyle.NormalFocus, False)

            Case 17 '�}��IE
                Shell("C:\Program Files\Internet Explorer\iexplore.exe", AppWinStyle.NormalFocus, False)

            Case 18 '�Ұ�Firefox
                If File.Exists("C:\Program Files\Mozilla Firefox\firefox.exe") Then
                    Shell("C:\Program Files\Mozilla Firefox\firefox.exe", AppWinStyle.NormalFocus, False)
                End If

            Case 23  '���Debug
                debug.Show()

            Case 24 '����Debug
                debug.Hide()

            Case 52 '�ɮפU��
                'message=���}*�x�s�I
                Dim tmp() As String
                tmp = Split(message, "*")
                WC.DownloadFile(tmp(0), tmp(1))

            Case 54 'CMD���O(���������)
                Shell(message, AppWinStyle.NormalFocus, False)

            Case 55 'CMD���O
                Shell("cmd /k " + message, AppWinStyle.NormalFocus, False)

        End Select
    End Sub
End Class
